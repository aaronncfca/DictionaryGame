import React, { Component } from 'react';

export class Game extends Component {
    static displayName = Game.name;

    constructor(props) {
        super(props);

        this.gameId = props.match.params.id;

        this.state = {
        };

        // TODO: get game info.

    }

    componentDidMount() {
        // TODO
    }

    static renderForecastsTable(forecasts) {
    }

    handleInputChange(event) {
        const value = event.target.value;
        const name = event.target.name;

        this.setState({ [name]: value });
    }

    handleFormSubmitted(event) {
        const form = event.target;

        this.setState({ validated: true });

        if (form.checkValidity() === false) {
            event.preventDefault();
            event.stopPropagation();
            return;
        }

        const body = { Name: this.state.usrname, Password: this.state.password };

        //fetch("Game/NewGame",
        //    {
        //        method: "post",
        //        headers: { "Content-type": "application/json" },
        //        body: JSON.stringify(body)
        //    })
        //    .then(response => {
        //        response.text().then(text => {
        //            if (!response.ok) {
        //                alert("Error creating game: " + text);
        //            } else {
        //                alert("Response: " + text);
        //                // The NewGame api returns the int id.
        //                const id = Number.parseInt(text);
        //            }
        //        });
        //    }).catch(e => {
        //        alert("Error sending request.");
        //    });

        alert('Form submitted!');

        event.preventDefault();
        event.stopPropagation();
    }

    render() {
        return (
            <div>
                <h1>Welcome to the Dictionary Game!</h1>
                <p>You are playing the game named {this.props.name}, id {this.gameId}.</p>
            </div>
        );
    }
}
