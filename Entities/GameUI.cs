using Basic_Wars_V2.Enums;
using Basic_Wars_V2.Graphics;
using Basic_Wars_V2.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Wars_V2.Entities
{
    public class GameUI : IGameEntity
    {
        private Tile Selected_UI;
        private bool UnitSelected = false;
        private bool TileSelected = false;

        private Unit SelectedUnit;
        private Tile SelectedTile;

        public Texture2D texture;
        public InputController _inputController;
        public ButtonManager _buttonManager;
        public MapManager _gameMap;
        public UnitManager _unitManager;

        public int DrawOrder { get; set; }

        public GameUI(Texture2D SpriteSheet, SpriteFont font, MapManager map, UnitManager unitManager)
        {
            texture = SpriteSheet;
            _gameMap = map;
            _unitManager = unitManager;

            _buttonManager = new ButtonManager();
            _inputController = new InputController(_unitManager, _buttonManager, _gameMap);

            //      TESTING
            /*Button TitleButton = new Button(texture, font, new Vector2(1080/2, 50), "Basic Wars", "Menu");
            Button NewGame = new Button(texture, font, new Vector2(1080/2, 270), "New Game", "Menu");
            Button LoadGame = new Button(texture, font, new Vector2(1080/2, 420), "Load Game", "Menu");
            Button Quit = new Button(texture, font, new Vector2(1080 / 2, 570), "Quit", "AltMenu");


            _buttonManager.AddButton(TitleButton);
            _buttonManager.AddButton(NewGame);
            _buttonManager.AddButton(LoadGame);
            _buttonManager.AddButton(Quit);
            */
            
            Selected_UI = new Tile(new Vector2(0, 0), texture);
            Selected_UI.CreateTile(0, 0, 1);

        }

        public void GetSelected()
        {
            SelectedUnit = _inputController.GetSelectedUnit();
            SelectedTile = _inputController.GetSelectedTile();

            if (SelectedUnit != null)
            {
                Selected_UI.Position= SelectedUnit.Position;
                UnitSelected = true;
            }
            else if (SelectedTile != null)
            {
                Selected_UI.Position= SelectedTile.Position;
                TileSelected = true;
            }
        }

        public void MoveUnit()
        {
            while (UnitSelected)
            { 
                Unit SelectedUnit = _inputController.GetSelectedUnit();
                _inputController.UpdateMouseState();
                foreach (Tile tile in _gameMap.map)
                {
                    if (_inputController.MouseCollider.Intersects(tile.Collider) 
                        && _inputController.currentMouseState.LeftButton == ButtonState.Pressed 
                        && _inputController.previousMouseState.LeftButton == ButtonState.Released)
                    {
                        SelectedUnit.Position = tile.Position;
                        SelectedUnit.State = UnitState.Idle;
                        UnitSelected = false;
                    }
                }
            }
        }

        public void CheckForUnitGeneration()
        {
            int currentTeam = 1;

            if (TileSelected)
            {
                if (SelectedTile.Type == TileType.Factory)  //Add team check as well later
                {
                    // Using console for now
                    // UI implmentation after frame is done

                    Console.WriteLine("Enter unit to be produced:\n1. Infantry\n2. Mech\n3. Tank\n4. APC");
                    int unitType = Convert.ToInt32(Console.ReadLine());

                    Unit newUnit = new Unit(texture, SelectedTile.Position, unitType, currentTeam); //Add current team turn
                    _unitManager.AddUnit(newUnit);
                }
                TileSelected = false;
            }
        }

        public void UpdateUI()
        {
            GetSelected();

            //Display movement tiles
        }

        

        public void Update(GameTime gameTime)
        {
            _inputController.ProcessControls(gameTime);
            MoveUnit();
            //CheckForUnitGeneration();
            UpdateUI();
        }

        public void Draw(SpriteBatch _spriteBatch, GameTime gameTime)
        {
            Selected_UI.Draw(_spriteBatch, gameTime);
        }


    }
}
 