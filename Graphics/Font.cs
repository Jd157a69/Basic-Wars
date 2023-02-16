using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Basic_Wars_V2.Graphics
{
    public class Font
    {
        public SpriteFont font { get; set; }

        public string Text { get; set; }

        public Vector2 textMiddlePoint { get; set; }

        public Color TintColour { get; set; } = Color.White;

        public Font(SpriteFont font, string text) 
        {
            this.font = font;
            this.Text = text;
            textMiddlePoint = font.MeasureString(text) / 2;
        }

        public void WriteText(SpriteBatch _spriteBatch, Vector2 Position)
        {
            _spriteBatch.DrawString(font, Text, Position, TintColour, 0, textMiddlePoint, 1.0f, SpriteEffects.None, 0.5f);
        }
    }
}
