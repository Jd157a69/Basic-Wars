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

        private List<Tile> reachableTiles = new List<Tile>();
        private List<Tile> moveableOverlay = new List<Tile>();
        private List<Tile> attackableOverlay = new List<Tile>();
        private List<Tile> tilesToBeRemoved = new List<Tile>();

        private Dictionary<(UnitType, UnitType), int> baseDamageDictionary = new Dictionary<(UnitType, UnitType), int>()
        { 
                {(UnitType.Infantry, UnitType.Infantry), 55 },
                {(UnitType.Infantry, UnitType.Mech), 45 },
                {(UnitType.Infantry, UnitType.Tank), 5 },
                {(UnitType.Infantry, UnitType.APC), 14 },
                {(UnitType.Mech, UnitType.Infantry), 65 },
                {(UnitType.Mech, UnitType.Mech), 55 },
                {(UnitType.Mech, UnitType.Tank), 55 },
                {(UnitType.Mech, UnitType.APC), 75 },
                {(UnitType.Tank, UnitType.Infantry), 75 },
                {(UnitType.Tank, UnitType.Mech), 70 },
                {(UnitType.Tank, UnitType.Tank), 55 },
                {(UnitType.Tank, UnitType.APC), 100 },
        };

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
            //Button TitleButton = new Button(Texture, font, new Vector2((1920 - 672) / 2, 90), "Basic Wars", "Menu");
            //Button NewGame = new Button(Texture, font, new Vector2((1920 - 672) / 2, 360), "New Game", "Menu");
            //Button LoadGame = new Button(Texture, font, new Vector2((1920 - 672) / 2, 540), "Load Game", "Menu");
            //Button Quit = new Button(Texture, font, new Vector2((1920 - 672) / 2, 720), "Quit", "Menu");


            //_buttonManager.AddButton(TitleButton);
            //_buttonManager.AddButton(NewGame);
            //_buttonManager.AddButton(LoadGame);
            //_buttonManager.AddButton(Quit);


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
            if (SelectedTile != null)
            {
                Selected_UI.Position = SelectedTile.Position;
                SelectedTile.State = TileState.None;
                TileSelected = true;
            }
        }

        private void GetReachableTiles()
        {
            if (UnitSelected)
            {
                moveableOverlay.Clear();

                Tile startingTile = GetCurrentUnitTile(SelectedUnit);
                
                reachableTiles = _pathFinder.FindReachableTiles(startingTile, SelectedUnit);

                //Blocking movement onto tiles already containing another unit
                foreach (Unit unit in _unitManager.units)
                {
                    foreach (Tile tile in reachableTiles)
                    {
                        if (unit.Position == tile.Position && SelectedUnit != unit)
                        {
                            tilesToBeRemoved.Add(tile);
                        }
                    }
                }
                foreach (Tile tile in tilesToBeRemoved)
                {
                    reachableTiles.Remove(tile);
                }
                tilesToBeRemoved.Clear();


                foreach (Tile tile in reachableTiles)
                {
                    if (tile.Position != SelectedUnit.Position)
                    {
                        if (
                            !(tile.Type == TileType.Mountain 
                            && SelectedUnit.Type == UnitType.Tank) 
                            && !(tile.Type == TileType.Mountain 
                            && SelectedUnit.Type == UnitType.APC)
                           )
                        {
                            Tile overlayTile = new Tile(tile.Position, Texture);
                            overlayTile.CreateTile(2, 1);
                            moveableOverlay.Add(overlayTile);
                        }
                        else
                        {
                            tilesToBeRemoved.Add(tile);
                        }
                    }
                }

                //Blocking display of mountain tiles for vehicles
                foreach (Tile tile in tilesToBeRemoved)
                {
                    moveableOverlay.Remove(tile);
                }
                tilesToBeRemoved.Clear();
            }
        }

        private void GetAttackableTiles()
        {
            if (UnitSelected)
            {
                attackableOverlay.Clear();

                Tile currentUnitTile = GetCurrentUnitTile(SelectedUnit); 
                List<Tile> adjacentTiles = _gameMap.GetNeighbours(currentUnitTile); 
                
                foreach (Tile tile in adjacentTiles)
                {
                    foreach (Unit unit in _unitManager.units)
                    {
                        if (
                            unit.Position == tile.Position 
                            && SelectedUnit.Team != unit.Team 
                            && SelectedUnit.Type != UnitType.APC
                           )
                        {
                            Tile attackingTile = new Tile(tile.Position, Texture);
                            attackingTile.CreateTile(1, 1);
                            attackableOverlay.Add(attackingTile);
                        }
                    }
                }
            }
        }

        private void DisplayAttributes()
        {
            if (UnitSelected)
            {
                Unit previousUnit = SelectedUnit;
                Console.WriteLine($"Type: {previousUnit.Type}");
                Console.WriteLine($"Health: {previousUnit.Health}");
                Console.WriteLine($"Ammo: {previousUnit.Ammo}");
                Console.WriteLine($"Movement Range: {previousUnit.MovementPoints}");
            }
            if (TileSelected)
            {
                Tile previousTile = SelectedTile;
                Console.WriteLine($"Type: {previousTile.Type}");
                Console.WriteLine($"Defence Bonus: {previousTile.DefenceBonus}");
            }
        }

        private void MoveUnit()
        {
            while (UnitSelected)
            {
                _inputController.UpdateMouseState();
                foreach (Tile tile in _gameMap.map)
                {
                    if (
                        _inputController.MouseCollider.Intersects(tile.Collider)
                        && _inputController.currentMouseState.LeftButton == ButtonState.Pressed
                        && _inputController.previousMouseState.LeftButton == ButtonState.Released
                        && reachableTiles.Contains(tile)
                       )
                    {
                        SelectedUnit.Position = tile.Position;
                        SelectedUnit.State = UnitState.Idle;
                        UnitSelected = false;
                    }
                }
            }
        }

        private void AttackUnit()
        {
            while (UnitSelected)
            {
                _inputController.UpdateMouseState();
                foreach (Unit unit in _unitManager.units)
                {
                    if (
                        _inputController.MouseCollider.Intersects(unit.Collider)
                        && _inputController.currentMouseState.LeftButton == ButtonState.Pressed
                        && _inputController.previousMouseState.LeftButton == ButtonState.Released
                        && SelectedUnit.Team != unit.Team
                        && attackableOverlay.Contains(GetCurrentUnitTile(unit))
                       )
                    {
                        SelectedUnit.Ammo--;
                        unit.Health -= DamageCalculation(SelectedUnit, unit);
                    }
                }
            }
        }

        private void CheckForUnitGeneration()
        {
            int currentTeam = 1;

            if (TileSelected)
            {
                TileSelected = false;
                if (SelectedTile.Type == TileType.Factory)
                {
                    // Using console for now
                    // UI implmentation after frame is done
                    //Add team check as well later

                    Console.WriteLine("Enter unit to be produced:\n1. Infantry\n2. Mech\n3. Tank\n4. APC");
                    //int unitType = Convert.ToInt32(Console.ReadLine());
                    int unitType = 1;

                    Unit newUnit = new Unit(Texture, SelectedTile.Position, unitType, currentTeam); //Add current team turn
                    _unitManager.AddUnit(newUnit);
                }
            }
        }

        private int DamageCalculation(Unit attackingUnit, Unit defendingUnit)
        {
            int damage = 0;

            int baseDamage = baseDamageDictionary[(attackingUnit.Type, defendingUnit.Type)];
            int defendingUnitDefenceBonus = GetCurrentUnitTile(defendingUnit).DefenceBonus;

            damage = baseDamage - (int)(baseDamage * (defendingUnitDefenceBonus/100));

            return damage;
        }

        private Tile GetCurrentUnitTile(Unit unit)
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

        private void UpdateUI()
        { 

            GetSelected();

            //DisplayAttributes();

            GetReachableTiles();
            GetAttackableTiles();
        }

        private void UpdateGameActions()
        {
            MoveUnit();
            AttackUnit();
            //Tile selection breaks after this method is run because in the draw method TileSelected will always be false due to the CheckForUnitGeneration running
            //CheckForUnitGeneration();
        }

        public void Update(GameTime gameTime)
        {
            _inputController.ProcessControls(gameTime);

            UpdateGameActions();

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
                foreach (Tile tile in moveableOverlay)
                {
                    tile.Draw(_spriteBatch, gameTime);
                }
                foreach (Tile tile in attackableOverlay)
                {
                    tile.Draw(_spriteBatch, gameTime);
                }
            }

            //foreach (Button button in _buttonManager.buttons)
            //{
            //    button.Draw(_spriteBatch, gameTime);
            //}
        }


    }
}
 