using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DictionaryGame.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

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

        /// <summary>
        /// Adds this user to the SignalR group with the game ID. Should be called on game creation.
        /// </summary>
        public async Task JoinGame(int gameId)
        {
            string groupName = gameId.ToString();

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await SendPlayerList(gameId);
        }


        /// <summary>
        /// Removes this user to the SignalR group with the game ID. Must be called on game deletion!
        /// </summary>
        public async Task RemoveFromGame(int gameId)
        {
            string groupName = gameId.ToString();

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await SendPlayerList(gameId);
        }

    }
}
