import React, { useState, useEffect } from 'react';

export function Game(props) {
    const gameId = props.match.params.id;

    const [isLoading, setIsLoading] = useState(true);
    const [gameName, setGameName] = useState('');
    const [players, setPlayers] = useState([]);

    useEffect(() => {
        (async function f() {
            const data = await fetch('/GameApi/Game/' + gameId);
            const game = await data.json();

            setPlayers(game.players);
            setGameName(game.name);
            setIsLoading(false);
        })();
    });


    return (
        <div>
            <h1>Welcome to the Dictionary Game!</h1>
            {isLoading ?
                <div class="d-flex justify-content-center">
                    <div class="spinner-border" role="status">
                        <span class="sr-only">Loading...</span>
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
                                    {players.forEach((player) =>
                                        <li>{player.name} ({player.points} points)</li>
                                    )}
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
