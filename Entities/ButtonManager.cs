using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Wars_V2.Entities
{
    public class ButtonManager : IGameEntity
    {
        public List<Button> buttons = new List<Button>();
        public int ID = 0;

        public int DrawOrder { get; set; }

        public void AddButton(Button button)
        {
            buttons.Add(button);
            button.ID = ID;
            ID++;
        }

        public void DrawButtonIDs(int initialID = -1, int finalID = -1, int initial2ID = -1, int final2ID = -1, int initial3ID = -1, int final3ID = -1)
        {
            foreach (Button button in buttons)
            {
                if ((button.ID >= initialID && button.ID <= finalID) || (button.ID >= initial2ID && button.ID <= final2ID) || (button.ID >= initial3ID && button.ID <= final3ID))
                {
                    button.DrawButton = true;
                }
                else
                {
                    button.DrawButton = false;  
                }
            }
        }

        public void UpdateButtonText(Button ButtonToChange, string Text)
        {
            foreach (Button button in buttons)
            {
                if (button == ButtonToChange)
                {
                    button.UpdateButtonText(Text);
                }
            }
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(SpriteBatch _spriteBatch, GameTime gameTime)
        {
            foreach (Button button in buttons)
            {
                if (button.DrawButton)
                {
                    button.Draw(_spriteBatch, gameTime);
                }
            }
        }
    }
}
