﻿using System;
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

            History = new LinkedList<Round>();
            Round = null;
        }


        public string Name { get; private set; }

        public string Password { get; private set; }

        public LinkedList<Player> Players { get; private set; }
        
        // Quietly keep track of the player who's it as a linked list node so
        // we can access Next when it's time for a new round.
        // Check Round.PlayerIt to check who's it from outside.
        private LinkedListNode<Player> _PlayerIt;

        public Player Host { get; set; }

        // TODO: don't serialize attribute on this this probably?
        // If client needs it, it should probably have it's own API call.
        /// <summary>
        /// List of all rounds. The current round will be History.Last.
        /// </summary>
        public LinkedList<Round> History { get; private set; }

        /// <summary>
        /// The current round.
        /// </summary>
        public Round Round { get; private set; }

        public void NewRound()
        {
            if (_PlayerIt == null || _PlayerIt.Next == null)
            {
                // This should never be called if there are no players!
                if (Players.Count == 0) throw new InvalidOperationException();

                // Wrap around to first player.
                _PlayerIt = Players.First;
            }

            Round = new Round(_PlayerIt.Value);
            History.AddLast(Round);
        }
    }
}
