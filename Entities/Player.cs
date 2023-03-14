using Basic_Wars_V2.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Wars_V2.Entities
{
    public class Player
    {
        public int Team { get; set; }
        public int Funds { get; set; }
        public bool HasHQ { get; set; }
        public string Colour { get; set; }
        public bool IsAI { get; set; }

        public Player(int team, int initialFunds, bool isAI = false)
        {
            Team = team;
            Funds = initialFunds;
            HasHQ = true;

            IsAI = isAI;

            GetTeamColour();
        }

        private void GetTeamColour()
        {
            switch (Team)
            {
                case 0:
                    Colour = "Red";
                    break;

                case 1:
                    Colour = "Blue";
                    break;

                case 2:
                    Colour = "Green";
                    break;

                case 3:
                    Colour = "Yello";
                    break;
            }
        }
    }
}
