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
        private const int TILE_WIDTH = 56;
        private const int TILE_HEIGHT = 56;
        private const int X_SPRITE_SHEET_START_POS = 336;
        private const int Y_SPRITE_SHEET_START_POS = 224;

        public TileType Type { get; set; }

        public Vector2 Position { get; set; }
        public Vector2 MapGridPos { get; set; }

        public int TotalCost { get; set; }

        private Texture2D Texture { get; set; }
        public Sprite TileSprite { get; set; }

        public int Team { get; set; }

        public int DrawOrder { get; set; }

        public int DefenceBonus { get; set; }

        public bool Selected { get; set; }

        public Tile(Vector2 position, Texture2D texture, int team = -1)
        {
            Position = position;
            Texture = texture;

            Team = team;

            Selected = false;
        }

        public void CreateTileSprite(int TileColumn = 0, int TileRow = 0)
        {
            TileColumn = TileColumn * TILE_WIDTH;
            TileRow = TileRow * TILE_HEIGHT;
            TileSprite = new Sprite(Texture, X_SPRITE_SHEET_START_POS + TileColumn, Y_SPRITE_SHEET_START_POS + TileRow, TILE_WIDTH, TILE_HEIGHT);

            SetTileAttributes();
        }

        public void CreateTileSpriteOnType()
        {
            switch (Type)
            {
                case TileType.Plains:
                    CreateTileSprite();
                    break;

                case TileType.Forest:
                    CreateTileSprite(1, 0);
                    break;

                case TileType.Mountain:
                    CreateTileSprite(2, 0);
                    break;

                case TileType.City:
                    CreateTileSprite(-5 + Team, 0);
                    break;

                case TileType.Factory:
                    CreateTileSprite(-5 + Team, 1);
                    break;

                case TileType.HQ:
                    CreateTileSprite(-5 + Team, 2);
                    break;
            }
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
                    DefenceBonus = 10;
                    break;

                case TileType.Forest:
                    DefenceBonus = 20;
                    break;

                case TileType.Mountain:
                    DefenceBonus = 40;
                    break;

                case TileType.Road:
                    DefenceBonus = 0;
                    break;

                case TileType.City:
                    DefenceBonus = 20;
                    break;

                case TileType.Factory:
                    DefenceBonus= 20;
                    break;

                case TileType.HQ:
                    DefenceBonus = 20;
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
