import React from 'react';
import { Button } from "reactstrap";


export function GameStepLobby({ user, onStartGame }) {

    // TODO: allow host to configure whether/how long to wait before timeout for GetDefs;
    // how many points to play to.

    return (
        <div>
            <h1>Welcome to the dictionary game!</h1>
            {user.isHost ?
                <div>
                    <p>You're the host! Invite your friends to join, and then click the button below to start!</p>
                    <p>They will need this info to join:</p>
                    <ul>
                        <li>Game name: <b>{user.gameName}</b></li>
                        <li>Game password: <b>{user.gamePassword}</b> </li>
                    </ul>
                    <Button color="primary" block onClick={onStartGame}>Start game!</Button>
                </div>
                :
                <div>
                    <p>Waiting for host to start the game...</p>
                </div>
            }
        </div>
    );
}

