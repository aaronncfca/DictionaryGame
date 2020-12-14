﻿import React, { useState, useContext } from 'react';
import { useHistory } from 'react-router-dom'
import { UserContext } from "../../UserContext.js"


export function GameStepGetDefs({ user, playerIt, ...props }) {
    const [def, setDef] = useState("");
    const [validated, setValidated] = useState(false);
    const [submitted, setSubmitted] = useState(false);

    function handleSubmit(event) {
        const form = event.target;

        // We don't need the form taking over control and reloading or anything like that.
        // Note: since this function is async, it's important we call these before any awaits.
        event.preventDefault();
        event.stopPropagation();

        setValidated(true);

        if (form.checkValidity() === false) {
            return;
        }

        props.onSubmitDef(def);
        setSubmitted(true);
    }

    return (
        <div>
            <h2>The word is: <b>{props.word}</b>!</h2>
            {user.userName === playerIt ?
                <div>
                    <p>Waiting for players to submit a word and definition...</p>
                </div>
                : !submitted ?
                <div>
                    <p>What could that mean?? Submit your definition here!</p>
                    <div>
                        <form noValidate className={validated ? "was-validated" : ""} onSubmit={handleSubmit}>
                            <div className="form-group">
                                <textarea className="form-control" type="" id="cg-def" name="definition" value={def}
                                    onChange={(e) => setDef(e.target.value)} required />
                            </div>
                            <button>Submit!</button>
                        </form>
                    </div>
                </div >
                :
                <div>
                    <p>Got it!</p>
                    <div>
                        <p>You said that <i>{props.word}</i> means:</p>
                        <p className="ml-1">{def}</p>
                        <p>Waiting for other players to respond...</p>
                    </div>
                </div >
            }
        </div>
    );
}
