import React, { useState, useContext } from 'react';
import { useHistory } from 'react-router-dom'
import { UserContext } from "../../UserContext.js"


export function GameStepLobby(props) {
    const user = props.user;

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
                    <button onClick={props.onStartGame}>Start game!</button>
                </div>
                :
                <div>
                    <p>Waiting for host to start the game...</p>
                </div>
            }
        </div>
    );
}

