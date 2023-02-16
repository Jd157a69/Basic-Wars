using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Wars.Graphics
{
    public class Sprite
    {
        public Texture2D Texture { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public Color TintColour { get; set; }

        public Sprite(Texture2D texture, int x, int y, int width, int height)
        {
            Texture = texture;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            TintColour = Color.White;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 Position, float Scale = 1f)
        {
            spriteBatch.Draw(Texture, Position, new Rectangle(X, Y, Width, Height), TintColour, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
        }
    }
}
