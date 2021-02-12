# Dictionary Game Readme

## About the Game

The Dictionary Game is an implementation of the classic guess-the-word game Fictionary. One player
chooses a word and submits its real definition, then everyone else guesses what the word means.

I began writing this game in 2020 during the COVID-19 pandemic as a party game for videoconferenced
get-togethers.

## Try It Out!

The game is currently hosted at https://dictgame.azurewebsites.net/. Get some friends together and
give it a try!

## Client Application

The client-side application (contained in the ClientApp folder) is written as a responsive web app
in react.js with react-strap UI components, using ECMAScript 6 and JSX features.

### Important Files

The react components which make up the client app are located in ClientApp/src/components.

  * App.js is the highest level component, containing the Layout and Routes for the app pages
  * home/Home.js is the index page for the app. From there, the user is invited to visit the
    home/CreateGame.js or home/JoinGame.js page to create or join a game.
  * game/Game.js is the in-game page. This file controls gameplay from the client side and handles
    connection tothe server through Microsoft SignalR.


### Create React App

The client app was bootstrapped with [Create React App](https://github.com/facebookincubator/create-react-app).

For the guide for create-react-app, see [here](https://github.com/facebookincubator/create-react-app/blob/master/packages/react-scripts/template/README.md).


## Server Side

The server-side application is built with ASP.NET Core, developed with Visual Studio 2019.

### Architecture

Source code has been organized roughly according to Model-View-Controller principles, where
the View is the React.js ClientApp.

#### Models

The highest-level class is DictionaryGame.Models.Game, which contains all the information of an
active game. Most significantly, it holds a linked list of Players (linked to facilitate iterating
through the players to select who is _it_ each round), the current Round, and a list of any
previous Rounds (currently unused, but could be used to provide a final overview at the
end of the game).

Each Round keeps track of the player who is _it_ and all data from the round (current state of the
round, definitions submitted, points scorred, etc.).

The Game class is instantiated in Program.cs as Program.ActiveGames, which stores game data for
all active games in memory (there is currently no database integration--see Known Issues below).


#### Controllers

The only ASP.NET controller is DictionaryGame.GameController, which provides an API for creating
and joining games.

Once a user creates or joins a game, the ClientApp establishes a SignalR connection with the
GameHub (GameHub.cs), which handles all in-game communication and most of the server-side logic.

### Gameplay/Logic

Here's a quick overview of how the game runs, from the server's perspective:

A player requests to create or join a game through the GameController API. On success, the API
returns a game ID which can be used to establish a SignalR connection with the GameHub.

The Game.Round property remains NULL until the host (the player which created the game) calls
GameHub.StartGame(). At this point, a new round is initialized and clients are alerted. The round
proceeds through four RoundStates, each state requesting appropriate client interaction, which
is reported through the GameHub so that the round can proceed once all players have submitted
whatever they need to. Once the round is over (after points have been tallied and reviewed),
a new round is created, with a different player selected to be _it_.

Three of the four RoundStates allow for the ClientApp to submit a timeout.

If a client is disconnected in the middle of the round, that player will be marked inactive and
they will be allowed back into the game at any time. Any new players that join in the middle of
the round will be marked pending until the beginning of the next round.

Once all clients in a given game have disconnected, that game is deleted.

### Known Issues

  * All game data is stored in memory on the server (in Program.ActiveGames). Although this
    minimizes complexity in the code and at runtime, it has the following potential disadvantages:
      * Server redundancy is impossible
      * An application or OS restart on the server would delete all game data and disconnect
        any active clients
      * Application usage is limited by the amount of memory available on the server
  * There is only minimal thread-safety protection to ensure that Program.ActiveGames is not
    corrupted. A number of scenarios have not been tested which may produce undefined results;
    such as if a user submits their answer while the server is processing a timeout request.
  * Timeouts are implemented on the client side, meaning multiple clients may simultaneously
    submit GameHub.SubmitTimeout(). It would make more sense for the server to implement the
    timeout, but this would require significant restructuring because timeout functions don't play
    well with SignalR hubs (since they are disposed and re-instantiated for every request).


## Contributing

Please feel free to fork this project on GitHub and submit a pull request! Just make sure your code
runs in a test environment such IIS on Windows before submitting.

