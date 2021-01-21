import React, { Component } from 'react';
import { HelpText } from "./HelpText.js";

// TODO: move content to a "Rules" component that can be reused and shown in-game.

export class Home extends Component {
    static displayName = Home.name;

    render() {
        return (
            <div>
                <h1>The Dictionary Game</h1>
                <p>Welcome to the dictionary game! This is a classic guess-the-word game. Similar copyrighted games that implement the same concept include Balderdash and Fibber.</p>
                <p>This is a great game for videoconference parties or around the table.</p>
                <HelpText />
            </div>
        );
    }
}
