import React, { useState } from 'react';
import { useHistory } from 'react-router-dom'


export function CreateGame(props) {
    const [usrname, setUsrname] = useState('');
    const [password, setPassword] = useState('');
    const [validated, setValidated] = useState(false);
    const history = useHistory();


    async function handleFormSubmitted(event) {
        const form = event.target;

        // We don't need the form taking over control and reloading or anything like that.
        // Note: since this function is async, it's important we call these before any awaits.
        event.preventDefault();
        event.stopPropagation();

        setValidated(true);

        if (form.checkValidity() === false) {
            return;
        }

        const body = { Name: usrname, Password: password };

        try {
            const response = await fetch("GameApi/NewGame", {
                method: "post",
                headers: { "Content-type": "application/json" },
                body: JSON.stringify(body)
            });

            const text = await response.text();

            if (!response.ok) {
                alert("Error creating game: " + text);
            } else {
                const gameId = Number.parseInt(text);

                // TODO: lost if the window reloads. Use context?
                props.gameCreated({ id: gameId, name: usrname });

                // Go to the game window!
                history.push("/game/" + gameId);
            }

        } catch (e) {
            alert("Error sending request: " + e);
        }
    }


    return (
        <div>
            <h1>Create Game</h1>
            <form noValidate class={validated ? "was-validated" : ""} onSubmit={handleFormSubmitted}>
                <div className="form-group">
                    <label htmlFor="cg-usrname">
                        You're user name (used to join this game):
                    </label>
                    <input className="form-control" type="text" id="cg-usrname" name="usrname" value={usrname} pattern="[a-zA-Z0-9]{4,49}"
                        onChange={(e) => setUsrname(e.target.value)} required />
                    <div className="invalid-feedback">
                        Username must be 4 to 49 characters, letters and numbers only.
                    </div>
                </div>
                <div class="form-group">
                    <label htmlFor="cg-password">Pasword for this game:</label>
                    <input className="form-control" type="text" id="cg-password" name="password" value={password} pattern=".{4,49}"
                        onChange={(e) => setPassword(e.target.value)} required />
                    <div className="invalid-feedback">
                        Password must be 4 to 49 characters.
                    </div>
                </div>
                <button class="btn btn-primary" type="submit">Create Game</button>
            </form>
        </div>
    );
}

