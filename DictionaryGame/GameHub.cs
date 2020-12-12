using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DictionaryGame.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;

namespace DictionaryGame
{

    public class GameHub : Hub
    {
        public async Task SendPlayerList(int gameId)
        {
            Game game;
            bool success;

            lock (Program.ActiveGames)
            {
                success = Program.ActiveGames.TryGetValue(gameId, out game);
            }

            if (!success) throw new ArgumentException("Game not found!");

            await Clients.Group(gameId.ToString()).SendAsync("setPlayerList", game.Players);
        }

        public class JoinGameReqArgs
        {
            public int GameId { get; set; }
            public string Username { get; set; }
        }

        /// <summary>
        /// Adds this user to the SignalR group with the game ID. Should be called on game creation.
        /// </summary>
        public async Task JoinGame(JoinGameReqArgs args)
        {
            string groupName = args.GameId.ToString();

            Context.Items.Add("gameId", args.GameId);
            Context.Items.Add("username", args.Username);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await SendPlayerList(args.GameId);
        }

        private Player GetPlayer(string userName, Game game)
        {
            Player player = game.Players.FirstOrDefault((entry) => entry.Name == userName);
            if (player == null)
            {
                // Should never happen in theory, since the hub connection is established
                // after the player calls GameController.Create/JoinGame successfully,
                // and the user is not removed from the Game until the hub connection is closed.
                throw new KeyNotFoundException("Player not found!");
            }

            return player;
        }

        /// <summary>
        /// On disconnection, remove the user from the game.
        /// If this user was the host, assign a new host (TODO).
        /// If this user was the last user, delete the Game.
        /// If this user was "it," end the round but not the game.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public async override Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);

            int gameId = (int)Context.Items["gameId"];
            string groupName = gameId.ToString();
            Game game;

            lock (Program.ActiveGames)
            {
                game = Program.ActiveGames[gameId];
            }

            Player player = GetPlayer((string)Context.Items["username"], game);

            game.Players.Remove(player);

            // Delete the game if no players remain.
            lock(Program.ActiveGames)
            {
                if(game.Players.First == null)
                {
                    Program.ActiveGames.Remove(gameId);
                }
            }

            // End the round if the player is "it." 

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await SendPlayerList(gameId);
        }


        public async Task StartGame() //TODO: rename StartRound
        {
            int gameId = (int) Context.Items["gameId"];
            Game game;

            lock (Program.ActiveGames)
            {
                game = Program.ActiveGames[gameId];
            }

            game.NewRound();

            await Clients.Group(gameId.ToString()).SendAsync("gotoStep", new
            {
                stepId = (int)RoundState.GetDict,
                playerIt = game.Round.PlayerIt.Name
            });
        }

        public class SubmitDictDefArgs
        {
            public string Word { get; set; }
            public string Definition { get; set; }
        }

        public async Task SubmitDictDef(SubmitDictDefArgs args)
        {
            int gameId = (int)Context.Items["gameId"];
            Game game;

            lock (Program.ActiveGames)
            {
                game = Program.ActiveGames[gameId];
            }

            game.Round.Word = args.Word;
            game.Round.DictDef = args.Definition;

            await Clients.Group(gameId.ToString()).SendAsync("gotoStep", new
            {
                stepId = (int)RoundState.GetDefs,
                word = game.Round.Word
            });

        }

        // TODO: implement submitDictDef
        // Then implement the next screen!

    }
}
