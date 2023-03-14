using System;
using System.Collections.Generic;

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
        public AIData Computer { get; set; }

        public GameData() { }

        public GameData(List<UnitData> units, List<TileData> gameMap, List<TileData> structures, List<PlayerData> players, GameStateData gameStateData, int mapWidth, int mapHeight, AIData computer = null)
        {
            Units = units;
            Map = gameMap;
            Structures = structures;
            Players = players;
            Computer = computer;
            GameStateData = gameStateData;
            MapWidth = mapWidth;
            MapHeight = mapHeight;
        }
    }
}
