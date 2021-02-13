import React, { useState } from "react";
import { RandomWordModal } from "./RandomWordModal";
import { Button, Form, FormGroup, Label, Input, FormFeedback } from "reactstrap";


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
                    <Button color="secondary" onClick={() => setRwModalOpen(true)}>Find a word</Button>
                    <RandomWordModal modalOpen={rwModalOpen} setModalOpen={setRwModalOpen} />
                    <Form noValidate className={(validated ? "was-validated" : "") + "mt-2"} onSubmit={handleSubmit}>
                        <FormGroup>
                            <Label for="gd-word">The word:</Label>
                            <Input type="text" id="gd-word" name="word" value={word} pattern=".{1,100}"
                                onChange={(e) => setWord(e.target.value)} required />
                            <FormFeedback>Word must be 1 to 100 characters long.</FormFeedback>
                        </FormGroup>
                        <FormGroup>
                            <Label for="cg-def">Dictionary definition:</Label>
                            <Input type="textarea" id="cg-def" name="definition" value={def}
                                onChange={(e) => setDef(e.target.value)} required />
                        </FormGroup>
                        <Button color="primary" block>Submit!</Button>
                    </Form>
                </div>
                :
                <div>
                    <p>Waiting for <span className="text-primary">{playerIt}</span> to submit a word and definition...</p>
                </div>
            }
        </div>
    );
}
