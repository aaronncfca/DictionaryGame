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
            Players = new LinkedList<Player>();
            _PlayerIt = null;
        }


        public string Name { get; private set; }

        public string Password { get; private set; }

        public LinkedList<Player> Players { get; private set; }

        /// <summary>
        /// Advance to the next player's turn. Goes to the first player if 
        /// </summary>
        public void AdvancePlayerIt()
        {
            if(_PlayerIt == null || _PlayerIt.Next == null)
            {
                // This should never be called if there are no players!
                // (Really it shouldn't be called unless there are 2 or more.)
                if (Players.Count == 0) throw new InvalidOperationException();

                // Wrap around to first pl  ayer.
                _PlayerIt = Players.First;
            }
        }
        
        /// <summary>
        /// Retrieves the Player that is currently it.
        /// Make sure AdvancePlayerIt is called before first calling this method!
        /// </summary>
        public Player PlayerIt
        {
            get {
                return _PlayerIt?.Value;
            }
        }
        private LinkedListNode<Player> _PlayerIt;

        public Player Host { get; set; }
    }
}
