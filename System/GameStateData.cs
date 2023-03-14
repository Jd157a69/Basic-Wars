using Basic_Wars_V2.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Wars_V2.System
{
    [Serializable]
    public class GameStateData
    {
        public int TurnNumber { get; set; }
        public int CurrentPlayerIndex { get; set; }
        public bool AddAI { get; set; }

        public GameStateData() { }

        public GameStateData(int turnNumber, int currentPlayerIndex, bool addAI)
        {
            TurnNumber = turnNumber;
            CurrentPlayerIndex = currentPlayerIndex;    
            AddAI = addAI;
        }
    }
}
