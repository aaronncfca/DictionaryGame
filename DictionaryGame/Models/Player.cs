using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DictionaryGame.Models
{
    public class Player
    {
        public Player(string name)
        {
            Name = name;
            Points = 0;
        }


        public string Name { get; private set; }

        public int Points { get; set; }
    }
}
