import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { FetchData } from './components/FetchData';
import { Counter } from './components/Counter';
import { Game } from './components/Game'
import { CreateGame } from './components/CreateGame'

import './custom.css'

export default class App extends Component {
    static displayName = App.name;

    constructor(props) {
        super(props);

        // TODO: this isn't apparently dependable.
        this.state = {
            gameName: ""
        };

        this.handleGameCreated = this.handleGameCreated.bind(this);
    }

    handleGameCreated(event) {
        this.setState({ gameName: event.name });
    }

    render() {
        return (
            <Layout>
                <Route exact path='/' component={Home} />
                <Route path='/counter' component={Counter} />
                <Route path='/fetch-data' component={FetchData} />
                <Route path='/create-game'>
                    <CreateGame gameCreated={this.handleGameCreated} />
                </Route>
                <Route path='/game/:id' render={props => <Game {...props} name={this.state.gameName}/> } />
            </Layout>
        );
    }
}
