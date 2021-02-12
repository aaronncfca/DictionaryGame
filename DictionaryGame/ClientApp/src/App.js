import React, { useState } from 'react'
import { Route } from 'react-router'
import { Layout } from './components/home/Layout'
import { Home } from './components/home/Home'
import { Game } from './components/game/Game'
import { CreateGame } from './components/home/CreateGame'
import { JoinGame } from './components/home/JoinGame'
import { About } from './components/home/About.js'
import { UserContext } from './UserContext.js'

import './custom.css'

export default function App(props) {
    // Holds the user information while a game is active.
    // Should be set to contain gameId, userName, and isHost.
    const [user, setUser] = useState({});

    return (
        <UserContext.Provider value={{ user, setUser }}>
            <Layout noNavBar={!!user.gameId}> {/* Hide nav bar while in-game */}
                <Route exact path='/' component={Home} />
                <Route path='/create-game' component={CreateGame} />
                <Route path='/join-game' component={JoinGame} />
                <Route path='/about' component={About} />
                <Route path='/game/:id' component={Game} />
            </Layout>
        </UserContext.Provider>
    );
}
