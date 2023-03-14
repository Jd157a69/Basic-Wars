﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

        public void WriteText(SpriteBatch _spriteBatch, Vector2 Position, float Scale = 1.0f)
        {
            _spriteBatch.DrawString(font, Text, Position, TintColour, 0, textMiddlePoint, Scale, SpriteEffects.None, 0.5f);
        }
    }
}
