using Basic_Wars.Graphics;
using Basic_Wars_V2.Entities;
using Basic_Wars_V2.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Wars_V2.System
{
    public class InputController
    {
        public UnitManager _unitManager;
        public ButtonManager _buttonManager;
        public MapManager _gameMap;
        public GameUI GameUI;

        public MouseState currentMouseState;
        public MouseState previousMouseState;

        public Rectangle MouseCollider { get; private set; }


        public InputController(UnitManager unitManager, ButtonManager buttonManager, MapManager gameMap)
        {
            _unitManager = unitManager;
            _buttonManager = buttonManager;
            _gameMap = gameMap;
        }

        public void UpdateMouseState()
        {
            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
            MouseCollider = new Rectangle(currentMouseState.X, currentMouseState.Y, 1, 1);
        }

        public void ProcessControls(GameTime gameTime, bool ProcessButtonsOnly)
        {
            bool UnitSelected = false;

            UpdateMouseState();

            if (!ProcessButtonsOnly)
            {
                foreach (Unit unit in _unitManager.units)
                {
                    if (
                        MouseCollider.Intersects(unit.Collider)
                        && currentMouseState.LeftButton == ButtonState.Pressed
                        && previousMouseState.LeftButton == ButtonState.Released
                       )
                    {
                        UnitSelected = true;
                        unit.Selected = true;
                    }
                }

                if (!UnitSelected)
                {
                    foreach (Tile tile in _gameMap.map)
                    {
                        if (
                            MouseCollider.Intersects(tile.Collider)
                            && currentMouseState.LeftButton == ButtonState.Pressed
                            && previousMouseState.LeftButton == ButtonState.Released
                           )
                        {
                            tile.State = TileState.Selected;
                        }
                    }
                }
            }
            
            foreach (Button button in _buttonManager.buttons)
            {
                if (
                    MouseCollider.Intersects(button.Collider)
                    && currentMouseState.LeftButton == ButtonState.Pressed
                    && previousMouseState.LeftButton == ButtonState.Released
                   )
                {
                    button.Pressed = true;
                }
            }
        }

        public Unit GetSelectedUnit()
        {
            foreach (Unit unit in _unitManager.units)
            {
                if (unit.Selected)
                {
                    return unit;
                }
            }
            return null;
        }

        public Tile GetSelectedTile()
        {
            foreach (Tile tile in _gameMap.map)
            {
                if (tile.State == TileState.Selected)
                {
                    return tile;
                }
            }
            return null;
        }

        public Button GetButtonPressed()
        {
            foreach (Button button in _buttonManager.buttons)
            {
                if (button.Pressed)
                {
                    button.Pressed = false;
                    return button;
                }
            }
            return null;
        }

        public List<Vector2> GetUnitPositions()
        {
            List<Vector2> unitPositions = new List<Vector2>();

            foreach (Unit unit in _unitManager.units)
            {
                unitPositions.Add(unit.Position);
            }

            return unitPositions;
        }

        public Tile GetUnitTile(Unit unit)
        {
            foreach (Tile tile in _gameMap.map)
            {
                if (unit.Position == tile.Position)
                {
                    return tile;
                }
            }
            return null;
        }
    }
}
