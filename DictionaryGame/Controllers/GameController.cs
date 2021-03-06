﻿using System;
using System.Collections.Generic;
using System.Linq;
using DictionaryGame.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace DictionaryGame
{

    [Route("GameApi")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IHubContext<GameHub> _gameHubContext;

        public const string SessionPlayerName = "_Name";
        public const string SessionGameId = "_GameId";

        public GameController([NotNull] IHubContext<GameHub> gameHubContext)
        {
            _gameHubContext = gameHubContext;
        }

        // GET: api/<GameController>
        //[HttpGet]
        //public IEnumerable<string> GetPlayers()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        // GET api/<GameController>/GetPlayers/5
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("Game/{id}")]
        [HttpGet]
        public IActionResult GetGame(int id)
        {
            Game game;
            bool success;

            lock(Program.ActiveGames)
            {
                success = Program.ActiveGames.TryGetValue(id, out game);
            }

            if (!success) return NotFound("Game does not exist.");

            return new JsonResult(game);
        }

        public class GameReqArgs
        {
            public string Name { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [Route("NewGame")]
        [HttpPost]
        public IActionResult NewGame([FromBody] GameReqArgs data)
        {
            if (data.Name == null) return BadRequest("Game name is required");
            if (data.Username == null) return BadRequest("User name is required");
            if (data.Password == null) return BadRequest("Password is required");

            if (data.Name.Length > 50) return BadRequest("Name too long");
            if (data.Name.Length < 1) return BadRequest("Name is required");
            if (data.Username.Length > 50) return BadRequest("User name too long");
            if (data.Username.Length < 1) return BadRequest("User name is required");
            if (data.Password.Length > 50) return BadRequest("Password too long");
            if (data.Password.Length < 4) return BadRequest("Password too short");

            int id;
            Game game;

            lock (Program.ActiveGames)
            {

                if (!Program.ActiveGames.FirstOrDefault((entry) => entry.Value.Name == data.Name).Equals(default(KeyValuePair<int, Game>)))
                {
                    return Conflict("A game already exists by that name");
                }

                game = new Game(data.Name, data.Password);
                var rnd = new Random();
                do {
                    id = rnd.Next();
                } while (Program.ActiveGames.ContainsKey(id));

                Program.ActiveGames.Add(id, game);
            }

            var player = new Player(data.Username);
            game.Players.AddLast(player);

            // TODO: would probably be better to add an IsHost field to Player.
            // However, this field is currently only important in that the host
            // chooses when to start the game.
            game.Host = player;

            return Ok(id);
        }


        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Route("JoinGame")]
        [HttpPost]
        public IActionResult JoinGame([FromBody] GameReqArgs data)
        {
            if (data.Name == null) return BadRequest("Game name is required");
            if (data.Username == null) return BadRequest("User name is required");
            if (data.Password == null) return BadRequest("Password is required");

            if (data.Username.Length > 50) return BadRequest("User name too long");
            if (data.Username.Length < 1) return BadRequest("User name is required");

            Game game;
            int gameId;
            KeyValuePair<int, Game> gameEntry;

            lock (Program.ActiveGames)
            {
                gameEntry = Program.ActiveGames.FirstOrDefault((entry) => entry.Value.Name == data.Name);

                if (gameEntry.Equals(default(KeyValuePair<int, Game>)))
                {
                    return NotFound("Game does not exist");
                }

                game = gameEntry.Value;
                gameId = gameEntry.Key;
            }

            if (game.Password != data.Password)
            {
                return Forbid("Incorrect password");
            }

            var samePlayer = game.Players.FirstOrDefault((entry) => entry.Name == data.Username);
            if (samePlayer != null)
            {
                if (samePlayer.IsActive == true)
                {
                    return Conflict("A player by that name has aready joined");
                }
                else
                {
                    // Special case: user is returning after getting disconnected.
                    // NOTE: this allows a new person to sign in on top of the old
                    // player just by using the same username. Possibly a bug, but
                    // hard to get away from.

                    samePlayer.IsActive = true;
                    return Ok(gameId);
                }
            }

            var player = new Player(data.Username);
            game.Players.AddLast(player);

            // If the game is already in motion, let the player in after the end of this round.
            if (game.Round != null
                && game.Round.RoundState != RoundState.Lobby
                && game.Round.RoundState != RoundState.GetDict)
            {
                player.IsPending = true;
            }

            return Ok(gameId);
        }

        // NOTE: once the player has joined, a SignalR hub (GameHub) is openned and all
        // further communcation is handled there.
    }
}
