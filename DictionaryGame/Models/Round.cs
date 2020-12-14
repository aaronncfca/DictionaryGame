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
        }

        public Player PlayerIt { get; private set; }
        public RoundState RoundState { get; set; }
        public string Word { get; set; }
        public string DictDef { get; set; }
        public Dictionary<string, string> Responses { get; private set; }
    }
}
