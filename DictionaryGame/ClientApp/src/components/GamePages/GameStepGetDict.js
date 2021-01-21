import React, { useState } from 'react';
import { RandomWordModal } from "./../RandomWordModal.js";


export function GameStepGetDict({ user, playerIt, ...props }) {
    const [word, setWord] = useState("");
    const [def, setDef] = useState("");
    const [validated, setValidated] = useState(false);
    const [rwModalOpen, setRwModalOpen] = useState(false);

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

        props.onSubmitDef(word, def);
    }

    return (
        <div>
            {user.userName === playerIt ?
                <div>
                    <h2>You're up!</h2>
                    <p>Please look up a nice obscure word in the dictonary. Type in the word and the definition below.</p>
                    <p>
                        No dictionary on hand? Click below to find a word
                        from <a href="https://randomword.com/" target="_blank" rel="noopener noreferrer">randomword.com</a>.
                    </p>
                    <button className="btn btn-small btn-secondary" onClick={() => setRwModalOpen(true)}>Find a word</button>
                    <RandomWordModal modalOpen={rwModalOpen} setModalOpen={setRwModalOpen} />
                    <div>
                        <form noValidate className={validated ? "was-validated" : ""} onSubmit={handleSubmit}>
                            <div className="form-group">
                                <label htmlFor="gd-word">
                                    The word:
                                </label>
                                <input className="form-control" type="text" id="gd-word" name="word" value={word} pattern=".{1,100}"
                                    onChange={(e) => setWord(e.target.value)} required />
                                <div className="invalid-feedback">
                                    Word must be 1 to 100 characters long.
                                </div>
                            </div>
                            <div className="form-group">
                                <label htmlFor="cg-def">Dictionary definition:</label>
                                <textarea className="form-control" type="" id="cg-def" name="definition" value={def}
                                    onChange={(e) => setDef(e.target.value)} required />
                            </div>
                            <button >Submit!</button>
                        </form>
                    </div>
                </div>
                :
                <div>
                    <p>Waiting for <span className="text-primary">{playerIt}</span> to submit a word and definition...</p>
                </div>
            }
        </div>
    );
}
