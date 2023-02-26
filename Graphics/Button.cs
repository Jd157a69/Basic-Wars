using Basic_Wars.Graphics;
using Basic_Wars_V2.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Basic_Wars_V2.Graphics
{
    public class Button : IGameEntity, ICollideable
    {
        public const int X_SPRITE_SHEET_START_POS = 0;
        public const int Y_SPRITE_SHEET_START_POS = 392;

        public int DrawOrder { get; set; }

        public Sprite buttonSprite;

        public Texture2D Texture;
        public Vector2 ButtonPosition;
        public Vector2 TextPosition;
        public Font text;

        public int Width { get; set; }
        public int Height { get; set; }

        public int ID { get; set; }

        public string ButtonType { get; set; }

        private int ButtonShiftX { get; set; }
        private int ButtonShiftY { get; set; }

        public Button(Texture2D texture, SpriteFont font, Vector2 position, string Text, string buttonType)
        {
            ButtonPosition = position;
            TextPosition = position;
            Texture = texture;

            ButtonType = buttonType;

            CreateButtonSprite();

            text = new Font(font, Text);
            CentreText();
        }

        public void CentreText()
        {
            TextPosition = new Vector2(TextPosition.X + Width / 2, TextPosition.Y + Height / 2);
        }

        public void GetButtonType()
        {
            switch (ButtonType)
            {
                case "Menu":
                    ButtonShiftX = 0;
                    ButtonShiftY = 0;
                    Width = 672;
                    Height = 127;
                    break;
                case "Attribute":
                    ButtonShiftX = 0;
                    ButtonShiftY = 127;
                    Width = 168;
                    Height = 282;
                    break;
                case "AltMenu":
                    ButtonShiftX = 168;
                    ButtonShiftY = 127;
                    Width = 320;
                    Height = 127;
                    break;
            }

        }

        public void CreateButtonSprite()
        {
            GetButtonType();
            buttonSprite = new Sprite(Texture, X_SPRITE_SHEET_START_POS + ButtonShiftX, Y_SPRITE_SHEET_START_POS + ButtonShiftY, Width, Height);
        }

        public Rectangle Collider
        {
            get
            {
                return new Rectangle((int)ButtonPosition.X, (int)ButtonPosition.Y, Width, Height);
            }
        }

        public void Draw(SpriteBatch _spriteBatch, GameTime gameTime)
        {
            buttonSprite.Draw(_spriteBatch, ButtonPosition);
            text.WriteText(_spriteBatch, TextPosition);
        }

        public void Update(GameTime gameTime)
        {

        }
    }
}
