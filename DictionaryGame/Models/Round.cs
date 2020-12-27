using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DictionaryGame.Models
{
    public enum RoundState
    {
        Lobby = 0,
        GetDict = 1,
        GetDefs = 2,
        Vote = 3,
        Review = 4
    }

    public class Round
    {
        public Round(Player playerIt)
        {
            PlayerIt = playerIt;
            Responses = new Dictionary<string, string>();
            Votes = new Dictionary<string, string>();
            AccurateDefs = new List<string>();
            AccurateDefsSubmitted = false;
            PointsAwarded = new Dictionary<string, int>();
            DoneReviewing = new List<Player>();
        }

        public Player PlayerIt { get; private set; }
        public RoundState RoundState { get; set; }

        /// <summary>
        /// The word that the players are trying to define, specified by PlayerIt.
        /// </summary>
        public string Word { get; set; }

        /// <summary>
        /// Stores the dictionary definition as specified by PlayerIt.
        /// </summary>
        public string DictDef { get; set; }

        /// <summary>
        /// Stores the responses of all players except PlayerIt when prompted for their
        /// definitions of Word.
        /// 
        /// Key is the Player's Name. Value is their definition as given.
        /// 
        /// TODO: here and below: use the Player as the key, and implement a JSON
        /// serializer that uses Player.Name when serializing.
        /// </summary>
        public Dictionary<string, string> Responses { get; private set; }

        /// <summary>
        /// Stores the votes of all players except PlayerIt.
        /// 
        /// Key is the Player's Name. Value is the Name of the player whose definition
        /// they voted for, or PlayerIt.Name if they voted for the DictDef.
        /// (See GameHub.SubmitVote)
        /// </summary>
        public Dictionary<string, string> Votes { get; private set; }

        /// <summary>
        /// List of all players who defined the word correctly, specified by PlayerIt.
        /// (See GameHub.SubmitVoteIt)
        /// </summary>
        public List<string> AccurateDefs { get; set; }

        /// <summary>
        /// Set when AccurateDefs is set. This is necessary because AccurateDefs may
        /// remain empty.
        /// </summary>
        public bool AccurateDefsSubmitted { get; internal set; }

        /// <summary>
        /// Stores the number of points awarded to each player
        /// </summary>
        public Dictionary<string, int> PointsAwarded { get; private set; }
        
        
        public List<Player> DoneReviewing { get; private set; }
    }
}
