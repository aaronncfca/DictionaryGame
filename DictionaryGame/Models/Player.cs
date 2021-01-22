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
            IsPending = false;
            IsActive = true;
        }


        public string Name { get; private set; }

        public int Points { get; set; }

        /// <summary>
        /// True if the player joined mid-round and will be let in once
        /// the next round starts.
        /// </summary>
        public bool IsPending { get; set; }

        /// <summary>
        /// True if the player is currently connected.
        /// </summary>
        public bool IsActive { get; set; }
    }
}
