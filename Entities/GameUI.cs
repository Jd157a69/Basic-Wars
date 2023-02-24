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
        private bool UnitSelected;
        private bool TileSelected;

        private Unit SelectedUnit;
        private Tile SelectedTile;

        private List<Tile> reachableTiles = new List<Tile>();
        private List<Tile> reachableOverlay = new List<Tile>();

        public Texture2D Texture;
        public SpriteFont Font;

        private InputController _inputController;
        private ButtonManager _buttonManager;
        private MapManager _gameMap;
        private UnitManager _unitManager;
        private Dijkstra _pathFinder;

        public int DrawOrder { get; set; }

        public GameUI(Texture2D SpriteSheet, SpriteFont font, MapManager map, UnitManager unitManager)
        {
            Texture = SpriteSheet;
            Font = font;
            _gameMap = map;
            _unitManager = unitManager;

            _buttonManager = new ButtonManager();
            _inputController = new InputController(_unitManager, _buttonManager, _gameMap);
            _pathFinder = new Dijkstra(_gameMap);


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

            Selected_UI = new Tile(new Vector2(0, 0), Texture);
            Selected_UI.CreateTile(0, 1);

        }

        private void GetSelected()
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
                SelectedTile.State = TileState.None;
                TileSelected = true;
            }
        }

        private void MoveUnit()
        {
            while (UnitSelected)
            {
                _inputController.UpdateMouseState();
                foreach (Tile tile in _gameMap.map)
                {
                    if (_inputController.MouseCollider.Intersects(tile.Collider)
                        && _inputController.currentMouseState.LeftButton == ButtonState.Pressed
                        && _inputController.previousMouseState.LeftButton == ButtonState.Released
                        && reachableTiles.Contains(tile))
                    {
                        SelectedUnit.Position = tile.Position;
                        SelectedUnit.State = UnitState.Idle;
                        UnitSelected = false;
                    }
                }
            }
        }

        private void ReachableTiles()
        {
            if (UnitSelected)
            {
                reachableOverlay.Clear();

                Tile startingTile = null;

                foreach (Tile tile in _gameMap.map)
                {
                    if (SelectedUnit.Position == tile.Position)
                    {
                        startingTile = tile;
                    }
                }
                
                reachableTiles = _pathFinder.FindReachableTiles(startingTile, SelectedUnit);

                foreach (Tile tile in reachableTiles)
                {
                    if (tile.Position != SelectedUnit.Position)
                    {
                        if (!(tile.Type == TileType.Mountain && SelectedUnit.Type == UnitType.Tank) && !(tile.Type == TileType.Mountain && SelectedUnit.Type == UnitType.APC))
                        {
                            Tile overlayTile = new Tile(tile.Position, Texture);
                            overlayTile.CreateTile(2, 1);
                            reachableOverlay.Add(overlayTile);
                        }
                    }
                }
            }
        }

        //private void DisplayAttributes()
        //{
        //    if (UnitSelected)
        //    {
        //        Unit previousUnit = SelectedUnit;
        //        Console.WriteLine($"Type: {previousUnit.Type}");
        //        Console.WriteLine($"Health: {previousUnit.Health}");
        //        Console.WriteLine($"Ammo: {previousUnit.Ammo}");
        //        Console.WriteLine($"Movement Range: {previousUnit.MovementPoints}");
        //    }
        //    if (TileSelected)
        //    {
        //        Tile previousTile = SelectedTile;
        //        Console.WriteLine($"Type: {previousTile.Type}");
        //        Console.WriteLine($"Defence Bonus: {previousTile.DefenceBonus}");
        //    }
        //}

        private void CheckForUnitGeneration()
        {
            int currentTeam = 1;

            if (TileSelected)
            {
                TileSelected = false;
                if (SelectedTile.Type == TileType.Factory)  //Add team check as well later
                {
                    // Using console for now
                    // UI implmentation after frame is done

                    Console.WriteLine("Enter unit to be produced:\n1. Infantry\n2. Mech\n3. Tank\n4. APC");
                    //int unitType = Convert.ToInt32(Console.ReadLine());
                    int unitType = 1;

                    Unit newUnit = new Unit(Texture, SelectedTile.Position, unitType, currentTeam); //Add current team turn
                    _unitManager.AddUnit(newUnit);
                }
            }
        }

        private void UpdateUI()
        {
            GetSelected();
            //DisplayAttributes();
            ReachableTiles();
        }


        public void Update(GameTime gameTime)
        {
            _inputController.ProcessControls(gameTime);

            MoveUnit();

            //Checking for unit generation gets stuck: when a unit is generated and you move the unit: the tile is registered as being clicked again
            //CheckForUnitGeneration();

            UpdateUI();
        }

        public void Draw(SpriteBatch _spriteBatch, GameTime gameTime)
        {
            if (UnitSelected || TileSelected)
            {
                Selected_UI.Draw(_spriteBatch, gameTime);
            }

            if (UnitSelected)
            {
                foreach (Tile tile in reachableOverlay)
                {
                    tile.Draw(_spriteBatch, gameTime);
                }
            }
        }


    }
}
 