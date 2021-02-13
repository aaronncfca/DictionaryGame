using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
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

        public IEnumerable<Player> ActivePlayers
        {
            get {
                return Players.Where((p) => (p.IsActive && !p.IsPending));
            }
        }
        
        // Quietly keep track of the player who's it as a linked list node so
        // we can access Next when it's time for a new round.
        // Check Round.PlayerIt to check who's it from outside.
        private LinkedListNode<Player> _PlayerIt;

        public Player Host { get; set; }

        /// <summary>
        /// List of all rounds. The current round will be History.Last.
        /// </summary>
        [JsonIgnore]
        public LinkedList<Round> History { get; private set; }

        /// <summary>
        /// The current round. Will be null until NewRound() has been called.
        /// </summary>
        public Round Round { get; private set; }

        public void NewRound()
        {
            // Let in any pending players.
            foreach (var p in Players)
            {
                p.IsPending = false;
            }

            var lastPlayerIt = _PlayerIt;

            do
            {
                if (_PlayerIt == null || _PlayerIt.Next == null)
                {
                    // This should never be called if there are no players!
                    if (Players.Count == 0) throw new InvalidOperationException();

                    // Wrap around to first player.
                    _PlayerIt = Players.First;
                }
                else
                {
                    _PlayerIt = _PlayerIt.Next;
                }
            } while (!_PlayerIt.Value.IsActive && _PlayerIt != lastPlayerIt);
            // Skip this player if they aren't active; but stop looking if it loops
            // all the way around (resulting in the same player being _it_, which is ok
            // if there are no other active users!

            int roundNum = (Round == null) ? 1 : Round.RoundNum + 1;

            Round = new Round(_PlayerIt.Value, roundNum);
            History.AddLast(Round);
        }
    }
}
