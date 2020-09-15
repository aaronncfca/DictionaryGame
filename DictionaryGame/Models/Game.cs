using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DictionaryGame.Models
{
    public class Game
    {
        public Game(string name, string pwd)
        {
            Name = name;
            Password = pwd;
        }

        public string Name { get; private set; }

        public string Password { get; private set; }

        public List<Player> Players { get; private set; }

        public Player Host { get; set; }
    }
}
