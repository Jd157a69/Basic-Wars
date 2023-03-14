﻿using Basic_Wars_V2.Entities;
using Basic_Wars_V2.Enums;
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
        public bool IsAI { get; set; }

        public PlayerData() { }

        public PlayerData(Player player)
        {
            Team = player.Team;
            Funds = player.Funds;
            HasHQ = player.HasHQ;
            Colour = player.Colour;
            IsAI = player.IsAI;
        }

        public Player FromPlayerData()
        {
            Player player = new Player(Team, Funds);
            player.HasHQ = HasHQ;
            player.Colour = Colour;
            player.IsAI = IsAI;

            return player;
        }
    }
}
