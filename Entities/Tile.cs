using Basic_Wars.Graphics;
using Basic_Wars_V2.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Wars_V2.Entities
{
    public class Tile : IGameEntity, ICollideable
    {
        public const int TILE_WIDTH = 56;
        public const int TILE_HEIGHT = 56;
        public const int X_SPRITE_SHEET_START_POS = 336;
        public const int Y_SPRITE_SHEET_START_POS = 224;

        //Unit unit;

        public TileType Type { get; set; }
        public TileState State { get; set; }

        public Vector2 Position { get; set; }
        public Texture2D Texture { get; set; }
        public Sprite TileSprite;

        public int DrawOrder { get; set; }
        public Unit Unit { get; set; }

        public int DefenseBonus { get; set; }

        public Tile(Vector2 position, Texture2D texture, Unit unit = null)
        {
            Position = position;
            Unit = unit;
            Texture = texture;

            State = TileState.None;
        }

        public void CreateTile(int RandomTile = 0, int TileColumn = 0, int TileRow = 0)
        {
            RandomTile = RandomTile * TILE_WIDTH;
            TileColumn = TileColumn * TILE_HEIGHT;
            TileRow = TileRow * TILE_HEIGHT;
            TileSprite = new Sprite(Texture, X_SPRITE_SHEET_START_POS + RandomTile + TileColumn, Y_SPRITE_SHEET_START_POS + TileRow, TILE_WIDTH, TILE_HEIGHT);

            SetTileAttributes();
        }

        public Rectangle Collider
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, TILE_WIDTH, TILE_HEIGHT);
            }
        }

        public void SetTileAttributes()
        {
            switch (Type)
            {
                case TileType.Plains:
                    DefenseBonus = 10;
                    break;

                case TileType.Forest:
                    DefenseBonus = 20;
                    break;

                case TileType.Mountain:
                    DefenseBonus = 40;
                    break;

                case TileType.Road:
                    DefenseBonus = 0;
                    break;

                case TileType.City:
                    DefenseBonus = 20;
                    break;

                case TileType.Factory:
                    DefenseBonus= 20;
                    break;

                case TileType.HQ:
                    DefenseBonus = 20;
                    break;
            }
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            TileSprite.Draw(spriteBatch, Position);
        }

        public void Update(GameTime gameTime)
        {
        }
    }
}
