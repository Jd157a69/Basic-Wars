using Basic_Wars_V2.Entities;
using Basic_Wars_V2.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Basic_Wars_V2.System
{
    [Serializable]
    public class TileData
    {
        public TileType Type { get; set; }

        public Vector2 Position { get; set; }
        public Vector2 MapGridPos { get; set; }

        public int Team { get; set; }

        public TileData() { }

        public TileData(Tile tile)
        {
            Type = tile.Type;
            Position = tile.Position;
            MapGridPos = tile.MapGridPos;
            Team = tile.Team;
        }

        public Tile FromTileData(Texture2D Texture)
        {
            Tile tile = new Tile(Position, Texture, Team)
            {
                Type = Type,
                MapGridPos = MapGridPos
            };

            return tile;
        }
    }
}
