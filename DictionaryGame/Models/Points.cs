using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DictionaryGame.Models
{
    public class Points
    {
        /// <summary>
        /// Awarded for writing a definition that accurately reflects the dictionary definition.
        /// </summary>
        public const int AccurateDef = 2;

        /// <summary>
        /// Awarded for each person that votes for the player's definition.
        /// </summary>
        public const int VotedForMe = 1;

        /// <summary>
        /// Awarded for guessing the dictionary definition (or another player's whose definition is
        /// identical to the dictionary definition).
        /// </summary>
        public const int AccurateVote = 1;

        /// <summary>
        /// Awarded to PlayerIt for selecting a word that noone can guess the definition of
        /// (either in writing their definition or in voting for the correct definition).
        /// </summary>
        public const int UnguessableWord = 3;
    }
}
