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

    public class GameConHub : Hub
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

        /// <summary>
        /// Removes this user to the SignalR group with the game ID. Must be called on game deletion!
        /// </summary>
        public async Task LeaveGame(int gameId)
        {
            string groupName = gameId.ToString();

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

        // TODO: implement submitDictDef
        // Then implement the next screen!

    }
}
