import React, { useState, useContext } from 'react';
import { UserContext } from "../UserContext.js";
import { useEffectOnce } from "../hooks/UseEffectOnce.js"
import { GameStepLobby } from "./GamePages/GameStepLobby.js"
import { GameStepGetDict } from "./GamePages/GameStepGetDict.js"
import { GameStepGetDefs } from "./GamePages/GameStepGetDefs.js"
import { GameStepVote } from "./GamePages/GameStepVote.js"
import { GameStepReview } from "./GamePages/GameStepReview.js"
import * as signalR from "@microsoft/signalr";

export function Game(props) {
    const gameId = props.match.params.id;

    const [isLoading, setIsLoading] = useState(true);
    const [gameName, setGameName] = useState('');
    const [players, setPlayers] = useState([]);
    // TODO: rename round, setRound
    const [round, setRound] = useState({ // See Round.cs
        roundState: 0,  // 0 = not started; awaiting players
                        // 1 = Acquiring word and dictionary def
                        // 2 = Collect defs
                        // 3 = Vote on defs
                        // 4 = Display answers and scores
        playerIt: "", // Username of the player who is "it" this round
        responses: {},
        votes: {},
        accurateDefs: [],
        pointsAwarded: {}
    });
    const { user } = useContext(UserContext);

    // Declare hubConnection as a state variable so it doesn't get re-initialized on rerender.
    const [hubConnection] = useState(() => {
        return new signalR.HubConnectionBuilder()
            .withUrl("/gamehub")
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

                const joinGameArgs = {
                    GameId: user.gameId,
                    Username: user.userName
                };

                hubConnection.invoke("joinGame", joinGameArgs)
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

        // Called to update round properties whenever we advance to a new RoundState.
        hubConnection.on("updateRound", (args) => {
            setRound({ ...args, playerIt: args.playerIt.name });
        });

        return () => {
            hubConnection.off("setPlayerList");
            // Note This will automatically/forcefully remove the player from the game!
            // See GameHub.OnDisconnectAsync.
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

    function handleSubmitDictDef(word, def) {
        hubConnection.invoke("submitDictDef", { Word: word, Definition: def });
    }

    function handleSubmitPlayerDef(def) {
        hubConnection.invoke("submitDef", { Definition: def });
    }

    // Players vote for the definition they think is correct. If they get it right, they get
    // points. If not, the player whose def they voted for gets points.
    function handleSubmitVote(userName) {
        hubConnection.invoke("submitVote", { UserName: userName });
    }

    // Player that's "it" chooses any definitions which are accurate to the dictionary
    // definition, and those players get points--as do any players who voted for them.
    function handleSubmitVoteIt(accurateDefs) {
        hubConnection.invoke("submitVoteIt", { AccurateDefs: accurateDefs });
    }

    function handleDoneReviewing() {
        hubConnection.invoke("submitDoneReviewing");
    }

    function renderGamePage() {
        switch (round.roundState) {
            case 0:
                return (<GameStepLobby user={user} onStartGame={handleStartGame} />);
            case 1:
                return (<GameStepGetDict user={user} playerIt={round.playerIt} onSubmitDef={handleSubmitDictDef} />);
            case 2:
                return (
                    <GameStepGetDefs user={user} playerIt={round.playerIt} word={round.word}
                        onSubmitDef={handleSubmitPlayerDef} />
                );
            case 3:
                return (
                    <GameStepVote user={user} playerIt={round.playerIt} word={round.word} dictDef={round.dictDef}
                        responses={round.responses}
                        onSubmitVote={handleSubmitVote} onSubmitVoteIt={handleSubmitVoteIt} />
                );
            case 4:
                return (
                    <GameStepReview playerIt={round.playerIt} word={round.word} dictDef={round.dictDef}
                        responses={round.responses} votes={round.votes} accurateDefs={round.accurateDefs}
                        onDoneReviewing={handleDoneReviewing}/>
                );
            default:
                console.error("Invalid roundState:" + round.roundState);
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
                    {round.roundNum &&
                        <h5>Round {round.roundNum}</h5>
                    }
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
                                { renderGamePage() }
                            </div>
                        </div>
                    </div>
                </div>
            }
        </div>
    );
}
