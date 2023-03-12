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
        public string Name { get; set; }
        public int Funds { get; set; }
        public bool HasHQ { get; set; }

        public Player(int team, int initialFunds)
        {
            Team = team;
            Funds = initialFunds;
            HasHQ = true;
        }
    }
}
