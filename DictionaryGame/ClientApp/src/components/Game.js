import React, { useState, useEffect, useContext } from 'react';
import { UserContext } from "../UserContext.js";
import { useEffectOnce } from "../hooks/UseEffectOnce.js"
import { GameStepLobby } from "./GamePages/GameStepLobby.js"
import * as signalR from "@microsoft/signalr";

export function Game(props) {
    const gameId = props.match.params.id;

    const [isLoading, setIsLoading] = useState(true);
    const [gameName, setGameName] = useState('');
    const [players, setPlayers] = useState([]);
    const [turn, setTurn] = useState({
        stepId: 0, //0 = not started; awaiting players
                // 1 = Acquiring word and dictionary def
				// 2 = Collect defs
				// 3 = Vote on defs
                // 4 = Display answers and scores
        playerIt: ""

    })
    const { user } = useContext(UserContext);

    // Declare hubConnection as a state variable so it doesn't get re-initialized on rerender.
    const [hubConnection] = useState(() => {
        return new signalR.HubConnectionBuilder()
            .withUrl("/gameconhub")
            .configureLogging(signalR.LogLevel.Information)
            .build();
    });

    // Place game logic here which should only be executed once.
    useEffectOnce(() => {
        fetch('/GameApi/Game/' + gameId).then((data) => {
            const jsonPromise = data.json();
            const hubPromise = hubConnection.start();

            Promise.all([jsonPromise, hubPromise]).then(([game]) => {

                setPlayers(game.players);
                setGameName(game.name);

                // Hub connection is alive.
                console.log(hubConnection.connectionId);

                hubConnection.invoke("joinGame", Number.parseInt(gameId))
                    .then(() => {
                        console.log("successfully sent joinGame.")

                        setIsLoading(false);
                    })
                    .catch((e) => {
                        console.log(e.message)
                    });
            }).catch(e => console.log(e));
        });

        hubConnection.on("setPlayerList", (_players) => {
            // Super simple error check. Is this necessary?
            if (!_players || !_players.length) {
                console.log(_players);
                throw new Error("Invalid data received from setPlayerList hub connection!");
            }
            setPlayers(_players);
        });

        hubConnection.on("gotoStep", ({ stepId, playerIt }) => {
            if (!playerIt) {
                playerIt = turn.playerIt;
            }
            setTurn({ stepId: stepId, playerIt: playerIt });
        })

        return () => {
            hubConnection.off("setPlayerList");
            hubConnection.stop();
        };
    }, []);

    //TODO: better: get out of here to an error page; maybe use Error Boundaries.
    if (!user || user.gameId !== Number.parseInt(gameId) || !user.userName) {
        return (
            <div>
                <h1>Error!</h1>
                <p>Invalid game. Did you refresh the page? Please try joining again!</p>
            </div>
        );
    }

    function handleStartGame() {
        hubConnection.invoke("startGame");
    }

    function renderGamePage() {
        switch (turn.stepId) {
            case 0:
                return (<GameStepLobby user={user} onStartGame={handleStartGame} />);
            //case 1:
            //    return (<GameStepGetDict />);
            //case 2:
            //    return (<GameStepGetDefs />);
            //case 3:
            //    return (<GamestepVote />);
            //case 4:
            //    return (<GameStepReview />);
            default:
                console.error("Invalid step ID!");
                return "Error!";
        }
    }

    return (
        <div className={user.isHost ? "player-is-host" : ""}>
            <h1>Welcome to the Dictionary Game!</h1>
            {isLoading ?
                <div className="d-flex justify-content-center">
                    <div className="spinner-border" role="status">
                        <span className="sr-only">Loading...</span>
                    </div>
                </div>
                :
                <div>
                    <h4>Game: {gameName}</h4>
                    <div className="container">
                        <div className="row">
                            <div className="col">
                                <h3>Players</h3>
                                <ul>
                                    {players.map((player) => (
                                        <li key={player.name}
                                            className={user.userName.normalize() === player.name.normalize() ? "text-primary" : "" }>
                                            {player.name} ({player.points} points)
                                        </li>
                                    ))}
                                </ul>
                            </div>
                            <div className="col">
                                <h3>Game (TODO)</h3>
                                { renderGamePage() }
                            </div>
                        </div>
                    </div>
                </div>
            }
        </div>
    );
}
