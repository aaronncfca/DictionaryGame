import React, { useState, useContext } from 'react';
import { UserContext } from "../UserContext.js";
import { Alert, Container, Col, Row, Button } from "reactstrap";
import { useEffectOnce } from "../hooks/UseEffectOnce.js";
import { GameStepLobby } from "./GamePages/GameStepLobby.js";
import { GameStepGetDict } from "./GamePages/GameStepGetDict.js";
import { GameStepGetDefs } from "./GamePages/GameStepGetDefs.js";
import { GameStepVote } from "./GamePages/GameStepVote.js";
import { GameStepReview } from "./GamePages/GameStepReview.js";
import { UserPending } from "./GamePages/UserPending.js";
import { HelpTextModal } from "./HelpTextModal.js";
import * as signalR from "@microsoft/signalr";

import css from "./Game.module.css";

export function Game(props) {
    const gameId = props.match.params.id;

    const [isLoading, setIsLoading] = useState(true);
    const [isPending, setIsPending] = useState(false); // Set to true if we joined a game in process.
    const [gameName, setGameName] = useState('');
    const [players, setPlayers] = useState([]);
    const [helpModalOpen, setHelpModalOpen] = useState(false);
    const [message, setMessage] = useState("");

    // TODO: rename round, setRound
    const [round, setRound] = useState({ // See Round.cs
        roundState: 0,  // 0 = not started; awaiting players
                        // 1 = Acquiring word and dictionary def
                        // 2 = Collect defs
                        // 3 = Vote on defs
                        // 4 = Display answers and scores
        playerIt: "", // Username of the player who is "it" this round
        responses: {},
        votes: {},
        accurateDefs: [],
        pointsAwarded: {},
        roundNum: null
    });
    const { user, setUser } = useContext(UserContext);

    

    // Declare hubConnection as a state variable so it doesn't get re-initialized on rerender.
    const [hubConnection] = useState(() => {
        return new signalR.HubConnectionBuilder()
            .withUrl("/gamehub")
            .configureLogging(signalR.LogLevel.Information)
            .build();
    });


    // Place game logic here which should only be executed once.
    useEffectOnce(() => {
        // Get out quick if we don't have a valid connection to a game.
        if (!user || user.gameId !== Number.parseInt(gameId) || !user.userName) {
            return;
        }

        fetch('/GameApi/Game/' + gameId).then((data) => {
            const jsonPromise = data.json();
            const hubPromise = hubConnection.start();

            Promise.all([jsonPromise, hubPromise]).then(([game]) => {

                setPlayers(game.players);
                setGameName(game.name);

                // Hub connection is alive.
                console.log(hubConnection.connectionId);

                const joinGameArgs = {
                    GameId: Number.parseInt(gameId),
                    Username: user.userName
                };

                hubConnection.invoke("joinGame", joinGameArgs)
                    .then(() => {
                        console.log("successfully sent joinGame.")
                        // We will set isLoading to false once we get a call to setPlayerList.
                    })
                    .catch((e) => {
                        console.log(e.message)
                    });
            }).catch(e => console.log(e));
        });

        hubConnection.on("updatePlayerList", (_players) => {
            // Super simple error check. Is this necessary?
            if (!_players || !_players.length) {
                console.log(_players);
                throw new Error("Invalid data received from setPlayerList hub connection!");
            }
            setPlayers(_players);

            const p = _players.find((p) => (p.name === user.userName));
            setIsPending(p.isPending);

            setIsLoading(false);
        });

        // Called to update round properties whenever we advance to a new RoundState.
        hubConnection.on("updateRound", (args) => {
            setRound({ ...args, playerIt: args.playerIt.name });
        });

        hubConnection.on("showMessage", (message) => {
            setMessage(message);
        });

        hubConnection.onclose((error) => {
            let message = "Oops, you've been disonnected! Try joining again with the same username.";
            if (error) {
                message += "\nError details: " + error.message;
            }

            setMessage(message);
        });

        return () => {
            // Note This will set player as inactive, and they will be disconnected from the game.
            // They may rejoin, however.
            // See GameHub.OnDisconnectAsync.
            hubConnection.stop(); 

            // Alert the app to the fact that we've disconnected. (Important if the user clicked
            // the back arrow.)
            setUser({});
        };
    }, []);


    //TODO: get out of here to an error page? maybe use Error Boundaries?
    if (!user || user.gameId !== Number.parseInt(gameId) || !user.userName) {
        return (
            <div>
                <h1>Error!</h1>
                <p>Invalid game. Did you refresh the page? Please try joining again!</p>
            </div>
        );
    }

    function handleStartGame() {
        hubConnection.invoke("startGame");
    }

    function handleSubmitDictDef(word, def) {
        hubConnection.invoke("submitDictDef", { Word: word, Definition: def });
    }

    function handleSubmitPlayerDef(def) {
        hubConnection.invoke("submitDef", { Definition: def });
    }

    // Players vote for the definition they think is correct. If they get it right, they get
    // points. If not, the player whose def they voted for gets points.
    function handleSubmitVote(userName) {
        hubConnection.invoke("submitVote", { UserName: userName });
    }

    // Player that's "it" chooses any definitions which are accurate to the dictionary
    // definition, and those players get points--as do any players who voted for them.
    function handleSubmitVoteIt(accurateDefs) {
        hubConnection.invoke("submitVoteIt", { AccurateDefs: accurateDefs });
    }

    function handleDoneReviewing() {
        hubConnection.invoke("submitDoneReviewing");
    }

    function renderGamePage() {
        if (isPending) {
            return (<UserPending />);
        }

        switch (round.roundState) {
            case 0:
                return (<GameStepLobby user={user} onStartGame={handleStartGame} />);
            case 1:
                return (<GameStepGetDict user={user} playerIt={round.playerIt} onSubmitDef={handleSubmitDictDef} />);
            case 2:
                return (
                    <GameStepGetDefs user={user} playerIt={round.playerIt} word={round.word}
                        onSubmitDef={handleSubmitPlayerDef} />
                );
            case 3:
                return (
                    <GameStepVote user={user} playerIt={round.playerIt} word={round.word} dictDef={round.dictDef}
                        responses={round.responses}
                        onSubmitVote={handleSubmitVote} onSubmitVoteIt={handleSubmitVoteIt} />
                );
            case 4:
                return (
                    <GameStepReview playerIt={round.playerIt} word={round.word} dictDef={round.dictDef}
                        responses={round.responses} votes={round.votes} accurateDefs={round.accurateDefs}
                        onDoneReviewing={handleDoneReviewing}/>
                );
            default:
                console.error("Invalid roundState:" + round.roundState);
                return "Error!";
        }
    }

    return (
        <div className={(user.isHost ? "player-is-host" : "") + " pt-4"}>
            {isLoading ?
                <div className="d-flex justify-content-center">
                    <div className="spinner-border" role="status">
                        <span className="sr-only">Loading...</span>
                    </div>
                </div>
                :
                <div>
                    <h4>Game: {gameName}</h4>
                    {round.roundNum &&
                        <h5>Round {round.roundNum}</h5>
                    }
                    <Container>
                        <Row>
                            <Col md="4">
                                {/*TODO: new component for each player.*/}
                                <h3>Players</h3>
                                <ul className={css.playerList}>
                                    {players.map((player) => (
                                        (!player.isActive || player.isPending) ?
                                            <li key={player.name} className="text-muted">{player.name}</li>
                                            :
                                            <li key={player.name}
                                                className={user.userName === player.name ? css.thisPlayer : ""}>
                                                {player.name} <span className={css.points}>({player.points} points)</span>
                                            </li>
                                    ))}
                                </ul>
                                <Button color="secondary" onClick={() => setHelpModalOpen(true)}>How to play</Button>
                                <HelpTextModal modalOpen={helpModalOpen} setModalOpen={setHelpModalOpen} />
                            </Col>
                            <Col md="8">
                                <Alert color="warning" isOpen={!!message} toggle={() => setMessage("")}>{message}</Alert>
                                {renderGamePage()}
                            </Col>
                        </Row>
                    </Container>
                </div>
            }
        </div>
    );
}
