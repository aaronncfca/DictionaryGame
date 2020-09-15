using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DictionaryGame.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace DictionaryGame.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        // GET: api/<GameController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<GameController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<GameController>
        [HttpPost]
        public void NewGame([FromBody] JObject data)
        {
            string name = data["name"].ToString();
            string pwd = data["password"].ToString();


            if (name.Length > 50) throw new ArgumentException("Name too long");
            if (pwd.Length > 50) throw new ArgumentException("Password too long");

            lock (Program.ActiveGames)
            {

                if (!Program.ActiveGames.FirstOrDefault((entry) => entry.Value.Name == name).Equals(default))
                {
                    // TODO: return a nice error to the user asking for a unique name!
                    throw new InvalidOperationException("Name already exists");
                }

                var game = new Game(name, pwd);
                int id;
                var rnd = new Random();
                do {
                    id = rnd.Next();
                } while (Program.ActiveGames.ContainsKey(id));

                Program.ActiveGames.Add(id, game);
            }
        }

        // POST api/<GameController>
        [HttpPost]
        public void JoinGame([FromBody] JObject data)
        {
            string name = data["name"].ToString();
            string hostName = data["hostName"].ToString();
            string pwd = data["password"].ToString();


            if (hostName.Length > 50) throw new ArgumentException("Host name too long");
            if (name.Length > 50) throw new ArgumentException("Name too long");
            if (pwd.Length > 50) throw new ArgumentException("Password too long");

            lock (Program.ActiveGames)
            {
                var gameEntry = Program.ActiveGames.FirstOrDefault((entry) => entry.Value.Name == hostName);

                if (gameEntry.Equals(default))
                {
                    // TODO: return a nice error to the user!
                    throw new InvalidOperationException("Game does not exist");
                }

                Game game = gameEntry.Value;

                if (game.Password != pwd)
                {
                    // TODO: return a nice error.
                    throw new InvalidOperationException("Incorrect password");
                }

                game.Players.Add(new Player(name));
            }
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
