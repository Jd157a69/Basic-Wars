using Basic_Wars_V2.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Basic_Wars_V2.System
{
    public class InputController
    {
        private UnitManager _unitManager;
        private ButtonManager _buttonManager;
        private MapManager _gameMap;
        private GameUI GameUI;

        private MouseState currentMouseState;
        private MouseState previousMouseState;

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

        public bool LeftMouseClicked()
        {
            if (previousMouseState.LeftButton == ButtonState.Released
                && currentMouseState.LeftButton == ButtonState.Pressed
               )
            {
                return true;
            }

            return false;
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
                        && LeftMouseClicked()
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
                            && LeftMouseClicked()
                           )
                        {
                            tile.Selected = true;
                        }
                    }
                }
            }

            foreach (Button button in _buttonManager.buttons)
            {
                if (
                    MouseCollider.Intersects(button.Collider)
                    && LeftMouseClicked()
                   )
                {
                    button.Pressed = true;
                }
            }
        }

        public void ChangeMap(MapManager newMap)
        {
            _gameMap = newMap;
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
                if (tile.Selected)
                {
                    tile.Selected = false;
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

        public Unit GetTileUnit(Tile tile)
        {
            foreach (Unit unit in _unitManager.units)
            {
                if (tile.Position == unit.Position)
                {
                    return unit;
                }
            }
            return null;
        }

        public void Update(UnitManager unitManager)
        {
            _unitManager = unitManager;
        }
    }
}
