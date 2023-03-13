using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Wars_V2.Entities
{
    public class Player
    {
        public int Team { get; private set; }
        public int Funds { get; set; }
        public bool HasHQ { get; set; }
        public string Colour { get; private set; }

        public Player(int team, int initialFunds)
        {
            Team = team;
            Funds = initialFunds;
            HasHQ = true;

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
