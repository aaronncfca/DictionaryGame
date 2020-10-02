import React, { Component } from 'react';

export class CreateGame extends Component {
    static displayName = CreateGame.name;

    constructor(props) {
        super(props);
        this.state = {
            usrname: "",
            password: "",
            validated: false
        };

        this.handleInputChange = this.handleInputChange.bind(this);
        this.handleFormSubmitted = this.handleFormSubmitted.bind(this);
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

        fetch("GameApi/NewGame",
            {
                method: "post",
                headers: { "Content-type": "application/json" },
                body: JSON.stringify(body)
            })
            .then(response => {
                response.text().then(text => {
                    if (!response.ok) {
                        alert("Error creating game: " + text);
                    } else {
                        const gameId = Number.parseInt(text);

                        this.props.gameCreated({ id: gameId, name: this.state.usrname });

                        // Go to the game window!
                        window.open("/Game/" + text);
                    }
                });
            }).catch(e => {
                alert("Error sending request.");
            });

        // Keep window from auto-reloading. TODO: is this necessary?
        event.preventDefault();
        event.stopPropagation();
    }

    render() {
        return (
            <div>
                <h1>Create Game</h1>
                <form noValidate class={this.state.validated ? "was-validated" : ""} onSubmit={this.handleFormSubmitted}>
                    <div class="form-group">
                        <label for="cg-usrname">
                            You're user name (used to join this game):
                        </label>
                        <input class="form-control" type="text" id="cg-usrname" name="usrname" value={this.state.usrname} pattern="[a-zA-Z0-9]{4,49}"
                            onChange={this.handleInputChange} required />
                        <div class="invalid-feedback">
                            Username must be 4 to 49 characters, letters and numbers only.
                        </div>
                    </div>
                    <div class="form-group">
                        <label for="cg-password">Pasword for this game:</label>
                        <input class="form-control" type="text" id="cg-password" name="password" value={this.state.password} pattern=".{4,49}"
                            onChange={this.handleInputChange} required />
                        <div class="invalid-feedback">
                            Password must be 4 to 49 characters.
                        </div>
                    </div>
                    <button class="btn btn-primary" type="submit">Create Game</button>
                </form>
            </div>
        );
    }
}
