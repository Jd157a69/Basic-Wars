using Basic_Wars_V2.Entities;
using Basic_Wars_V2.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Wars_V2.System
{
    [Serializable]
    public class TileData
    {
        public TileType Type { get; set; }
        public TileState State { get; set; }

        public Vector2 Position { get; set; }
        public Vector2 MapGridPos { get; set; }

        public int Team { get; set; }
        public int DefenceBonus { get; set; }

        public TileData() { }   

        public TileData(Tile tile)
        {
            Type = tile.Type;
            State = tile.State;
            Position = tile.Position;
            MapGridPos = tile.MapGridPos;
            Team = tile.Team;
            DefenceBonus = tile.DefenceBonus;
        }

        public Tile FromTileData(Texture2D Texture)
        {
            Tile tile = new Tile(Position, Texture, Team);
            tile.Type = Type;
            tile.State = State;
            tile.MapGridPos = MapGridPos;
            tile.DefenceBonus = DefenceBonus;

            switch (tile.Type)
            {
                case TileType.Plains:
                    tile.CreateTileSprite();
                    break;
                case TileType.Forest:
                    tile.CreateTileSprite(1, 0);
                    break;
                case TileType.Mountain:
                    tile.CreateTileSprite(2, 0);
                    break;
                case TileType.City:
                    tile.CreateTileSprite(-5 + Team, 0);
                    break;
                case TileType.Factory:
                    tile.CreateTileSprite(-5 + Team, 1);
                    break;
                case TileType.HQ:
                    tile.CreateTileSprite(-5 + Team, 2);
                    break;
            }

            return tile;
        }
    }
}
