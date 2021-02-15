import React, { useState, useContext } from "react";
import { useHistory } from "react-router-dom";
import { UserContext } from "../../UserContext";
import { Button, Form, FormGroup, Label, Input, FormFeedback } from "reactstrap";


export function JoinGame(props) {
    const [gameName, setGameName] = useState('');
    const [usrname, setUsrname] = useState('');
    const [password, setPassword] = useState('');
    const [validated, setValidated] = useState(false);
    const history = useHistory();

    const { setUser } = useContext(UserContext);

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

        const body = { Name: gameName, Username: usrname, Password: password };

        try {
            const response = await fetch("GameApi/JoinGame", {
                method: "post",
                headers: { "Content-type": "application/json" },
                body: JSON.stringify(body)
            });

            const text = await response.text();

            if (!response.ok) {
                alert("Error joining game: " + text);
            } else {
                const gameId = Number.parseInt(text);

                // Set the global user context. This passes the game id on to the Game component.
                setUser({
                    gameId: gameId,
                    userName: usrname,
                    isHost: false
                });

                // Go to the game window!
                history.push("/game/" + gameId);
            }

        } catch (e) {
            alert("Error sending request: " + e);
        }
    }


    return (
        <div>
            <h1>Join Game</h1>
            <Form noValidate className={validated ? "was-validated" : ""} onSubmit={handleFormSubmitted}>
                <FormGroup>
                    <Label for="cg-gamename">
                        The game name:
                    </Label>
                    <Input type="text" id="cg-gamename" name="gamename" value={gameName} pattern="[a-zA-Z0-9 ]{4,49}"
                        onChange={(e) => setGameName(e.target.value)} required />
                    <FormFeedback>
                        Username must be 4 to 49 characters, letters and numbers only.
                    </FormFeedback>
                </FormGroup>
                <FormGroup>
                    <Label for="cg-password">Password for this game:</Label>
                    <Input type="text" id="cg-password" name="password" value={password} pattern=".{4,49}"
                        onChange={(e) => setPassword(e.target.value)} required />
                    <FormFeedback>
                        Password must be 4 to 49 characters.
                    </FormFeedback>
                </FormGroup>
                <FormGroup>
                    <Label for="cg-usrname">
                        Your user name:
                    </Label>
                    <Input className="form-control" type="text" id="cg-usrname" name="usrname" value={usrname} pattern="[a-zA-Z0-9 ]{4,49}"
                        onChange={(e) => setUsrname(e.target.value)} required />
                    <FormFeedback>
                        Username must be 4 to 49 characters, letters and numbers only.
                    </FormFeedback>
                </FormGroup>
                <Button color="primary">Join Game</Button>
            </Form>
        </div>
    );
}

