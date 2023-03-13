using Basic_Wars_V2.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Wars_V2.System
{
    [Serializable]
    public class PlayerData
    {
        public int Team { get; set; }
        public int Funds { get; set; }
        public bool HasHQ { get; set; }
        public string Colour { get; set; }

        public PlayerData() { }

        public PlayerData(Player player)
        {
            Team = player.Team;
            Funds = player.Funds;
            HasHQ = player.HasHQ;
            Colour = player.Colour;
        }

        public Player FromPlayerData()
        {
            Player player = new Player(Team, Funds);
            player.HasHQ = HasHQ;
            player.Colour = Colour;

            return player;
        }
    }
}
