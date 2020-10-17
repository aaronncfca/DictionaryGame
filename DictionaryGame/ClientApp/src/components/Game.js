import React, { useState, useEffect, useContext } from 'react';
import { UserContext } from "../UserContext.js"

export function Game(props) {
    const gameId = props.match.params.id;

    const [isLoading, setIsLoading] = useState(true);
    const [gameName, setGameName] = useState('');
    const [players, setPlayers] = useState([]);
    const { user } = useContext(UserContext);

    useEffect(() => {
        (async function f() {
            const data = await fetch('/GameApi/Game/' + gameId);
            const game = await data.json();

            setPlayers(game.players);
            setGameName(game.name);
            setIsLoading(false);
        })();
    }, []);

    //TODO: better: get out of here to an error page; maybe use Error Boundaries.
    if (!user || user.gameId !== gameId || !user.userName) {
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
