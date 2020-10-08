using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DictionaryGame.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace DictionaryGame
{
    [Route("GameApi")]
    [ApiController]
    public class GameController : ControllerBase
    {
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
            public string Password { get; set; }
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [Route("NewGame")]
        [HttpPost]
        public IActionResult NewGame([FromBody] GameReqArgs data)
        {
            if (data.Name.Length > 50) return BadRequest("Name too long");
            if (data.Name.Length < 1) return BadRequest("Name is required");
            if (data.Password.Length > 50) return BadRequest("Password too long");
            if (data.Password.Length < 4) return BadRequest("Password too short");

            int id;

            lock (Program.ActiveGames)
            {

                if (!Program.ActiveGames.FirstOrDefault((entry) => entry.Value.Name == data.Name).Equals(default(KeyValuePair<int, Game>)))
                {
                    // TODO: return a nice error to the user asking for a unique name!
                    return Conflict("A game already exists by that name");
                }

                var game = new Game(data.Name, data.Password);
                var rnd = new Random();
                do {
                    id = rnd.Next();
                } while (Program.ActiveGames.ContainsKey(id));

                Program.ActiveGames.Add(id, game);
            }

            return Ok(id);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Route("JoinGame/{gameName}")]
        [HttpPost]
        public IActionResult JoinGame(string gameName, [FromBody] GameReqArgs data)
        {
            Game game;
            KeyValuePair<int, Game> gameEntry;

            lock (Program.ActiveGames)
            {
                gameEntry = Program.ActiveGames.FirstOrDefault((entry) => entry.Value.Name == gameName);

                if (gameEntry.Equals(default(KeyValuePair<int, Game>)))
                {
                    return NotFound("Game does not exist");
                }

                game = gameEntry.Value;
            }

            if (data.Name.Length > 50) return BadRequest("Name too long");
            if (data.Name.Length < 1) return BadRequest("Name is required");
            if (data.Password.Length > 50) return BadRequest("Password too long");
            if (data.Password.Length < 4) return BadRequest("Password too short");

            if (game.Password != data.Password)
            {
                // TODO: return a nice error.
                return Forbid("Incorrect password");
            }

            game.Players.Add(new Player(data.Name));

            return Ok(gameEntry.Key);
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
