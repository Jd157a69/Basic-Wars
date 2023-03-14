using Basic_Wars_V2.Entities;
using Basic_Wars_V2.Enums;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Wars_V2.System
{
    [Serializable]
    public class AIData : PlayerData
    {
        public AIState AIState { get; set; }

        public AIData() { }

        public AIData(AI Computer)
        {
            Team = Computer.Team;
            Funds = Computer.Funds;
            HasHQ = Computer.HasHQ;
            Colour = Computer.Colour;
        }

        public AI FromAIData(Texture2D Texture, MapManager mapManager, UnitManager unitManager, GameUI gameUI, InputController inputController)
        {
            AI Computer = new AI(Team, Funds, mapManager, unitManager, gameUI, inputController, Texture);
            Computer.HasHQ = HasHQ;
            Computer.Colour = Colour;

            return Computer;
        }
    }
}
