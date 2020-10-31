import React, { useState, useEffect, useContext } from 'react';
import { UserContext } from "../UserContext.js";
import * as signalR from "@microsoft/signalr";

export function Game(props) {
    const gameId = props.match.params.id;

    const [isLoading, setIsLoading] = useState(true);
    const [gameName, setGameName] = useState('');
    const [players, setPlayers] = useState([]);
    const { user } = useContext(UserContext);

    // TODO: why does this get called multiple times?
    const hubConnection = new signalR.HubConnectionBuilder()
        .withUrl("/gameconhub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    hubConnection.start().then(() => {
        // Hub connection is alive.
        console.log(hubConnection.connectionId);

        // TODO: don't send this until we've actually successfully joined!
        // TODO: signal doesn't seem to go through!
        hubConnection.invoke("joinGame", Number.parseInt(gameId))
            .then(() => { console.log("successfully sent joinGame.")})
            .catch((e) => {
                console.log(e.message)
            });

    }).catch(e => console.log(e));

    useEffect(() => {
        (async function f() {
            const data = await fetch('/GameApi/Game/' + gameId);
            const game = await data.json();

            setPlayers(game.players);
            setGameName(game.name);
            setIsLoading(false);
        })();

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
