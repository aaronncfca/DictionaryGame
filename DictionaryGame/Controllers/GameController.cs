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
        const string SessionPlayerName = "_Name";
        const string SessionGameId = "_GameId";

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
                    // TODO: return a nice error to the user asking for a unique name!
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
            game.Players.Add(player);
            game.Host = player; //TODO: would probably be better to add an IsHost field to Player

            HttpContext.Session.SetString(SessionPlayerName, data.Username);
            HttpContext.Session.SetInt32(SessionGameId, id);

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
            KeyValuePair<int, Game> gameEntry;

            lock (Program.ActiveGames)
            {
                gameEntry = Program.ActiveGames.FirstOrDefault((entry) => entry.Value.Name == data.Name);

                if (gameEntry.Equals(default(KeyValuePair<int, Game>)))
                {
                    return NotFound("Game does not exist");
                }

                game = gameEntry.Value;
            }

            if (game.Password != data.Password)
            {
                return Forbid("Incorrect password");
            }

            game.Players.Add(new Player(data.Username));

            // TODO: use SignInManager instead of Session.
            HttpContext.Session.SetString(SessionPlayerName, data.Username);
            HttpContext.Session.SetInt32(SessionGameId, gameEntry.Key);

            return Ok(gameEntry.Key);
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
