﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Wars_V2.System
{
    [Serializable]
    public class GameData
    {
        public int MapWidth { get; set; }
        public int MapHeight { get; set; }

        public List<UnitData> Units { get; set; }
        public List<TileData> Map { get; set; }
        public List<TileData> Structures { get; set; }
        public List<PlayerData> Players { get; set; }
        public GameStateData GameStateData { get; set; }

        public GameData() { }

        public GameData(List<UnitData> units, List<TileData> gameMap, List<TileData> structures, List<PlayerData> players, GameStateData gameStateData, int mapWidth, int mapHeight)
        {
            Units = units;
            Map = gameMap;
            Structures = structures;
            Players = players;
            GameStateData = gameStateData;
            MapWidth = mapWidth;
            MapHeight = mapHeight;
        }
    }
}
