import React, { useState } from "react";
import { HelpText } from "../shared/HelpText";
import { Collapse, Button, Container } from 'reactstrap';
import { Link } from 'react-router-dom';

import css from './Home.module.css'

export function Home() {
    const [helpOpen, setHelpOpen] = useState(false);

    return (
        <div>
            <img src="/assets/dictgame-icon-color.svg" className={"d-none d-sm-block " + css.dicticon} />

            <h1>The Dictionary Game</h1>
            <p>Welcome to the dictionary game! This is an implementation of the classic guess-the-word game Fictionary.
                    If you've ever played Balderdash (copyright Mattel Games), you'll recognize how this works!</p>
            <p>This is a great game for videoconference parties or around the table.</p>

            <Container>
                <Link to="/Create-Game">
                    <Button color="primary" className="m-2">Create a New Game</Button>
                </Link>

                <Link to="/Join-Game">
                    <Button className="m-2">Join a Game</Button>
                </Link>
            </Container>

            <Button color="info" className="my-4" onClick={() => { setHelpOpen(!helpOpen) }} >How do I play?</Button>
            <Collapse isOpen={helpOpen}>
                <HelpText />
            </Collapse>
        </div>
    );
}
