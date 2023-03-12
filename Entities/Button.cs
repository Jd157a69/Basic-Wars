using Basic_Wars.Graphics;
using Basic_Wars_V2.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Basic_Wars_V2.Entities
{
    public class Button : IGameEntity, ICollideable
    {
        private const int X_SPRITE_SHEET_START_POS = 0;
        private const int Y_SPRITE_SHEET_START_POS = 392;

        public int DrawOrder { get; set; }

        private Sprite buttonSprite;

        private Texture2D Texture;
        private Vector2 ButtonPosition;
        private Vector2 TextPosition;
        private SpriteFont spriteFont;
        private Font text;

        private int Width;
        private int Height;

        public int ID { get; set; }

        private int ButtonType { get; set; }

        private int ButtonShiftX { get; set; }
        private int ButtonShiftY { get; set; }

        public bool Pressed { get; set; }
        public bool DrawButton { get; set;}

        public Button(Texture2D texture, SpriteFont font, Vector2 position, int buttonType = -1, string Text = "")
        {
            ButtonPosition = position;

            Texture = texture;
            spriteFont = font;
            DrawButton = false;

            ButtonType = buttonType;

            CreateButtonSprite();

            UpdateButtonText(Text);

            Pressed = false;
        }

        public void CentreText()
        {
            TextPosition = new Vector2(ButtonPosition.X + Width / 2, ButtonPosition.Y + Height / 2);
        }

        public void CreateButtonSprite()
        {
            GetButtonType();
            buttonSprite = new Sprite(Texture, X_SPRITE_SHEET_START_POS + ButtonShiftX, Y_SPRITE_SHEET_START_POS + ButtonShiftY, Width, Height);
        }

        public void GetButtonType()
        {
            switch (ButtonType)
            {
                case 0:
                    //Menu
                    ButtonShiftX = 0;
                    ButtonShiftY = 0;
                    Width = 672;
                    Height = 125;
                    break;
                case 1:
                    //AltMenu
                    ButtonShiftX = 252;
                    ButtonShiftY = 127;
                    Width = 289;
                    Height = 125;
                    break;
                case 2:
                    //Attribute
                    ButtonShiftX = 0;
                    ButtonShiftY = 127;
                    Width = 252;
                    Height = 378;
                    break;
                case 3:
                    //Pause
                    ButtonShiftX = 252;
                    ButtonShiftY = 252;
                    Width = 131;
                    Height = 131;
                    break;
                case 4:
                    //Tile
                    ButtonShiftX = 500;
                    ButtonShiftY = 500;
                    Width = 56;
                    Height = 56;
                    break;
                default:
                    ButtonShiftX = 500;
                    ButtonShiftY = 500;
                    Width = 289;
                    Height = 127;
                    break;
            }
        }

        public Rectangle Collider
        {
            get
            {
                return new Rectangle((int)ButtonPosition.X, (int)ButtonPosition.Y, Width, Height);
            }
        }

        public void UpdateButtonText(string displayedText)
        {
            text = new Font(spriteFont, displayedText);
            CentreText();   //Should only centre text once, currently doing it every update
        }

        public void Draw(SpriteBatch _spriteBatch, GameTime gameTime, float Scale = 1.0f)
        {
            buttonSprite.Draw(_spriteBatch, ButtonPosition, Scale);
            text.WriteText(_spriteBatch, TextPosition, Scale);
        }

        public void Update(GameTime gameTime)
        {

        }
    }
}
