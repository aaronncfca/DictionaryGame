import React, { useState, useEffect, useContext } from 'react';
import { UserContext } from "../UserContext.js";
import { useEffectOnce } from "../hooks/UseEffectOnce.js"
import * as signalR from "@microsoft/signalr";

export function Game(props) {
    const gameId = props.match.params.id;

    const [isLoading, setIsLoading] = useState(true);
    const [gameName, setGameName] = useState('');
    const [players, setPlayers] = useState([]);
    const { user } = useContext(UserContext);


    // Place game logic here which should only be executed once.
    useEffectOnce(() => {

        const hubConnection = new signalR.HubConnectionBuilder()
            .withUrl("/gameconhub")
            .configureLogging(signalR.LogLevel.Information)
            .build();

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
                            </div>
                        </div>
                    </div>
                </div>
            }
        </div>
    );
}
