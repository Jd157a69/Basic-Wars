using Basic_Wars_V2.Graphics;
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


        public void Draw(SpriteBatch _spriteBatch, GameTime gameTime)
        {
            foreach (Button button in buttons)
            {
                button.Draw(_spriteBatch, gameTime);
            }
        }

        public void AddButton(Button button)
        {
            buttons.Add(button);
            button.ID = ID;
            ID++;
        }

        public void ClearButtons()
        {
            buttons.Clear();
            ID = 0;
        }
        
        public void Update(GameTime gameTime)
        {

        }
    }
}
