using Basic_Wars_V2.Entities;
using Basic_Wars_V2.Enums;
using Basic_Wars_V2.Graphics;
using Basic_Wars_V2.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using static System.Net.Mime.MediaTypeNames;

namespace Basic_Wars_V2
{
    public class BasicWarsGame : Game
    {
        readonly private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        const string ASSET_NAME_IN_GAME_ASSETS = "InGameAssets";
        const string ASSET_NAME_GAMEFONT = "Font";

        private Texture2D InGameAssets;
        private SpriteFont Font;

        private const int WINDOW_WIDTH = 1920;
        private const int WINDOW_HEIGHT = 1080;

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

        private GameState gameState;
        private MenuState menuState;

        private List<Player> Players;

        private Player CurrentPlayer;
        private int PlayerIndex;
        private bool NextPlayer;

        private InputController _inputController;

        private GameUI _gameUI;
        private EntityManager _entityManager;
        private UnitManager _unitManager;
        private ButtonManager _buttonManager;
        private MapManager _gameMap;

        private Unit SelectedUnit;
        private List<Tile> reachableTiles;
        private List<Tile> attackableTiles;

        private Tile SelectedTile;

        private int TurnNumber;

        /*  
         *  TODO: Create a main game loop
         *      - Update method should loop through a list of players, going through each game state before moving onto the next player
         *      - This will introduce the Player class: potential use of built in Enum PlayerIndex?
         *  
         *  TODO: Generate units using a factory tile
         *      - Use console to specify type and team of unit for now and implement UI version in the future
         *  
         *  DONE: Implementation of the movement point system for each unit type and displaying it with GameUI
         *  
         *  TODO: Ability to distinguish what team a unit is on and only allowing the current player to select units on their team
         *  
         *  TODO: Ability for units to attack each other 
         *  
         *  TODO: Attributes for both units and tiles should be displayed
         *      - Use console for now and implement UI version in the future
         *      
         *  TODO: User should eneter the number of players in the game (Max 4) 
         *      - Use console for this and implement the UI version in future
         *  
         *  TODO: Code A* for use with the AI
         *      - Computerphile video on YouTube: https://www.youtube.com/watch?v=ySN5Wnu88nE
         *      - Simple A* Path Finding in Monogame: https://youtu.be/FflEY83irJo
         *  
         *  TODO: JSON or XML read and write to files
         *      - Serialization and deserialization of files: 
         *          https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/how-to?pivots=dotnet-7-0
         *          https://www.c-sharpcorner.com/article/working-with-json-in-C-Sharp/
         *  
         */

        public BasicWarsGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            _graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
            _graphics.ApplyChanges();


            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            InGameAssets = Content.Load<Texture2D>(ASSET_NAME_IN_GAME_ASSETS);
            Font = Content.Load<SpriteFont>(ASSET_NAME_GAMEFONT);

            menuState = MenuState.Initial;

            Players = new List<Player>();

            NextPlayer = true;

            TurnNumber = 0;

            _entityManager = new EntityManager();
            _unitManager = new UnitManager();
            _buttonManager = new ButtonManager();
            _gameMap = new MapManager(InGameAssets, 16, 16, 2);

            _gameUI = new GameUI(InGameAssets, Font, _gameMap, _unitManager, _buttonManager);
            _inputController = new InputController(_unitManager, _buttonManager, _gameMap);

            _entityManager.AddEntity(_gameMap);
            _entityManager.AddEntity(_unitManager);
            _entityManager.AddEntity(_gameUI);

            //      TESTING
            for (int i = 0; i < 4; i++)
            {
                int temp = 56 * i;
                Unit unit = new Unit(InGameAssets, new Vector2(512 + temp, 92), i + 1, i + 1);
                _unitManager.AddUnit(unit);
            }

        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _inputController.ProcessControls(gameTime);
            Button PressedButton = _inputController.GetButtonPressed();

            if (menuState != MenuState.PlayingGame)
            {
                switch (menuState)
                {
                    case MenuState.Initial:
                        Init(gameTime, PressedButton);
                        break;

                    case MenuState.NewGame:
                        NewGame(gameTime, PressedButton);
                        break;

                    case MenuState.RefreshMap:
                        RefreshMap();
                        break;

                    case MenuState.LoadGame:
                        break;

                    case MenuState.QuitGame:
                        Exit();
                        break;
                }
            }
            

            if (menuState == MenuState.PlayingGame)
            {

                if (TurnNumber == 0)
                {
                    TurnNumber++;
                    StartGame(gameTime);
                    PlayerIndex = 0;
                }

                if (NextPlayer) //Change turn number after Players.Count number of players has passed
                {
                    NextPlayer = false;

                    if (PlayerIndex + 1 > Players.Count)
                    {
                        TurnNumber++;
                    }

                    if (PlayerIndex > Players.Count - 1)
                    {
                        PlayerIndex = 0;
                    }
                    
                    CurrentPlayer = Players[PlayerIndex];
                    
                    gameState = _gameUI.Turn(gameTime, CurrentPlayer, TurnNumber, PressedButton);

                    //DEBUG
                    Console.WriteLine($"\nPlayer: {CurrentPlayer.Team}");
                    Console.WriteLine($"Turn: {TurnNumber}");
                }

                switch (gameState)
                {
                    case GameState.PlayerSelect:
                        gameState = _gameUI.Turn(gameTime, CurrentPlayer, TurnNumber, PressedButton);
                        PlayerSelect(gameTime);     //Stop registering other actions that are not related to PlayerSelectAction
                        break;

                    case GameState.SelectAction:
                        PlayerSelectAction(gameTime, PressedButton);
                        break;

                    case GameState.PlayerMove:
                        PlayerMove(gameTime);
                        break;

                    case GameState.PlayerAttack:
                        PlayerAttack(gameTime);
                        break;

                    case GameState.EnemyTurn:
                        NextPlayer = true;
                        PlayerIndex++;
                        break;

                    case GameState.GameOver:
                        break;
                }
            }

            _entityManager.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            _entityManager.Draw(_spriteBatch, gameTime);
            _spriteBatch.End();

            base.Draw(gameTime);
        }


        private void Init(GameTime gameTime, Button PressedButton)
        {
            menuState = _gameUI.Init(gameTime, PressedButton);
        }

        private void NewGame(GameTime gameTime, Button PressedButton) 
        {
            menuState = _gameUI.NewGame(gameTime, PressedButton);
            Players = _gameUI.GetPlayers();
        }

        private void RefreshMap(int Width = 16, int Height = 16)
        {
            _entityManager.RemoveEntity(_gameMap);
            _gameMap = new MapManager(InGameAssets, Width, Height, Players.Count);
            _entityManager.AddEntity(_gameMap);

            _gameUI.ChangeMap(_gameMap);

            menuState = MenuState.NewGame;
        }

        private void StartGame(GameTime gameTime)
        {
            _gameMap.DrawMap = true;
            _unitManager.DrawUnits = true;
        }

        private void LoadGame(GameTime gameTime)
        {

        }

        private void PlayerSelect(GameTime gameTime)
        {
            if (SelectedUnit != null)
            {
                SelectedUnit.State = UnitState.None;
                SelectedUnit.Selected = false;
            }

            SelectedUnit = _inputController.GetSelectedUnit();
            SelectedTile = _inputController.GetSelectedTile();

            if (SelectedUnit != null)
            {
                _gameUI.ChangeSelectedPosition(SelectedUnit.Position);
                _gameUI.DrawSelectedUI = true;
                SelectedUnit.Selected = true;

                _gameUI.DisplayAttributes(SelectedUnit);

                if (SelectedUnit.Team == CurrentPlayer.Team + 1)        
                {
                    Console.WriteLine($"Player Team: {CurrentPlayer.Team + 1}");
                    Console.WriteLine($"Unit team: {SelectedUnit.Team}");

                    gameState = GameState.SelectAction;
                }
                else
                {
                    SelectedUnit.Selected = false;
                }
            }
            if (SelectedTile != null)
            {
                _gameUI.ChangeSelectedPosition(SelectedTile.Position);
                _gameUI.DrawSelectedUI = true;
                SelectedTile.State = TileState.None;
            }
        }

        private void PlayerSelectAction(GameTime gameTime, Button PressedButton)
        {
            reachableTiles = _gameUI.GetReachableTiles(SelectedUnit, _inputController.GetUnitPositions(), _inputController.GetUnitTile(SelectedUnit));
            attackableTiles = _gameUI.GetAttackableTiles(SelectedUnit, _inputController.GetUnitTile(SelectedUnit));
            gameState = _gameUI.DisplayPlayerActions(gameTime, PressedButton);
        }
        
        private void PlayerMove(GameTime gameTime)
        {
            SelectedUnit.State = UnitState.Moving;

            while (SelectedUnit.State == UnitState.Moving)    //while loop stops the ability to draw and causes draw after it has run, reachable tiles must be run first and displayed
            {
                _inputController.UpdateMouseState();

                foreach (Tile tile in _gameMap.map)
                {
                    if (
                        _inputController.MouseCollider.Intersects(tile.Collider)
                        && _inputController.currentMouseState.LeftButton == ButtonState.Pressed
                        && _inputController.previousMouseState.LeftButton == ButtonState.Released
                        && reachableTiles.Contains(tile)
                        && SelectedUnit.Fuel > 0
                       )
                    {
                        MoveUnit(SelectedUnit, tile, reachableTiles);
                    }
                }
            }
        }

        private void PlayerAttack(GameTime gameTime)
        {
            SelectedUnit.State = UnitState.Attacking;

            while (SelectedUnit.State == UnitState.Attacking)
            {
                _inputController.UpdateMouseState();

                foreach (Unit unit in _unitManager.units)
                {
                    if (
                        _inputController.MouseCollider.Intersects(unit.Collider)
                        && _inputController.currentMouseState.LeftButton == ButtonState.Pressed
                        && _inputController.previousMouseState.LeftButton == ButtonState.Released   
                        && attackableTiles.Contains(_inputController.GetUnitTile(unit)) //Something wrong with attackable tiles here
                       )
                    {
                        AttackUnit(SelectedUnit, unit);
                    }
                }
            }
        }
         
        private void MoveUnit(Unit movingUnit, Tile unitDestination, List<Tile> reachableTiles)
        {
            movingUnit.Position = unitDestination.Position;
            movingUnit.Fuel--;

            movingUnit.State = UnitState.None;

            _gameUI.ChangeSelectedPosition(SelectedUnit.Position);
            
            gameState = GameState.SelectAction;
        }

        private void AttackUnit(Unit attackingUnit, Unit defendingUnit)
        {
            if (attackingUnit.Ammo > 0)
            {
                Console.WriteLine($"Defending unit health before: {defendingUnit.Health}");
                attackingUnit.Ammo--;
                defendingUnit.Health -= DamageCalculation(attackingUnit, defendingUnit);
                Console.WriteLine($"Defending unit health after: {defendingUnit.Health}");

                attackingUnit.State = UnitState.None;

                gameState = GameState.SelectAction;
            }
        }

        private int DamageCalculation(Unit attackingUnit, Unit defendingUnit)
        {
            int damage = 0;

            int baseDamage = baseDamageDictionary[(attackingUnit.Type, defendingUnit.Type)];
            int defendingUnitDefenceBonus = _inputController.GetUnitTile(defendingUnit).DefenceBonus;

            damage = baseDamage - (int)(baseDamage * (defendingUnitDefenceBonus / 100));

            return damage;
        }

        public Unit CheckForUnitGeneration(Tile tile, int unitType, int currentTeam)
        {
            //int currentTeam = 1;

            //if (TileSelected)
            //{
            //    TileSelected = false;
            //    if (SelectedTile.Type == TileType.Factory)
            //    {
            //        // Using console for now
            //        // UI implmentation after frame is done
            //        //Add team check as well later

            //        Console.WriteLine("Enter unit to be produced:\n1. Infantry\n2. Mech\n3. Tank\n4. APC");
            //        //int unitType = Convert.ToInt32(Console.ReadLine());
            //        int unitType = 1;

            //        Unit newUnit = new Unit(Texture, SelectedTile.Position, unitType, currentTeam); //Add current team turn
            //        _unitManager.AddUnit(newUnit);
            //    }
            //}

            if (tile.Type == TileType.Factory)
            {
                Unit newUnit = new Unit(InGameAssets, tile.Position, unitType, currentTeam);
                return newUnit;
            }
            return null; // Temporary
        }
    }
}