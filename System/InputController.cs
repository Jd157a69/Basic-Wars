using Basic_Wars.Graphics;
using Basic_Wars_V2.Entities;
using Basic_Wars_V2.Enums;
using Basic_Wars_V2.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Wars_V2.System
{
    public class InputController
    {
        public UnitManager UnitManager;
        public ButtonManager ButtonManager;
        public MapManager GameMap;
        public GameUI GameUI;

        public MouseState currentMouseState;
        public MouseState previousMouseState;


        public Rectangle MouseCollider { get; private set; }


        public InputController(UnitManager unitManager, ButtonManager buttonManager, MapManager gameMap)
        {
            UnitManager = unitManager;
            ButtonManager = buttonManager;
            GameMap = gameMap;
        }

        public void ProcessControls(GameTime gameTime)
        {
            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
            MouseCollider = new Rectangle(currentMouseState.X, currentMouseState.Y, 1, 1);

            bool Selected = false;

            //Testing

            foreach (Unit unit in UnitManager.units)
            {
                if (MouseCollider.Intersects(unit.Collider) && currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
                {
                    unit.State = UnitState.Selected;
                    Selected = true;
                }
                while (Selected)
                {
                    previousMouseState = currentMouseState;
                    currentMouseState = Mouse.GetState();
                    MouseCollider = new Rectangle(currentMouseState.X, currentMouseState.Y, 1, 1);

                    foreach (Tile tile in GameMap.map)
                    {
                        //tile.ContainsUnit does not work properly
                        if (MouseCollider.Intersects(tile.Collider) && currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
                        {
                            unit.Position = tile.Position;
                            tile.Unit = unit;
                            Selected = false;
                            unit.State = UnitState.Idle;
                        }
                    }
                }
            }

            foreach (Tile tile in GameMap.map)
            {
                if (MouseCollider.Intersects(tile.Collider) && currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
                {
                    tile.State = TileState.Selected;
                }
            }

            //foreach (Button button in ButtonManager.buttons)
            //{
            //    if (MouseCollider.Intersects(button.Collider) && currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
            //    {
            //        //button.ButtonPosition = new Vector2(button.ButtonPosition.X, button.ButtonPosition.Y - 56);
            //        //button.TextPosition = new Vector2(button.TextPosition.X, button.TextPosition.Y - 56);
            //    }
            //}
        }
    }
}
