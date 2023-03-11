﻿using Basic_Wars_V2.Entities;
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
using System.Text.RegularExpressions;
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
        private bool ProcessButtonsOnly;

        private GameUI _gameUI;
        private EntityManager _entityManager;
        private UnitManager _unitManager;
        private ButtonManager _buttonManager;
        private MapManager _gameMap;

        private Unit SelectedUnit;
        private List<Tile> reachableTiles;
        private List<Tile> attackableTiles;

        private Tile SelectedTile;

        private Button PressedButton;

        private int TurnNumber;


        private bool DrawRan;

        /*  TODO: Code optimisations
         *      - Feels like code starts to run much slower after a short period of time
         *      - Likely a lot of code that isn't necessary needs to be removed
         *  
         *  TODO: Create a main game loop
         *      - Update method should loop through a list of players, going through each game state before moving onto the next player
         *      - This will introduce the Player class: potential use of built in Enum PlayerIndex?
         *  
         *  DONE: Generate units using a factory tile
         *      - Use console to specify type and team of unit for now and implement UI version in the future
         *  
         *  DONE: Implementation of the movement point system for each unit type and displaying it with GameUI
         *  
         *  DONE: Ability to distinguish what team a unit is on and only allowing the current player to select units on their team
         *  
         *  DONE: Attributes for both units and tiles should be displayed
         *      - Use console for now and implement UI version in the future
         *      
         *  DONE: User should eneter the number of players in the game (Max 4) 
         *      - Use console for this and implement the UI version in future
         *      
         *  TODO: Ability for units to attack each other 
         *      
         *  TODO: Adjustable map size
         *      
         *  TODO: Universalise the Team number
         *  
         *  TODO: Code the AI
         *      - Use heuristics and weights to determine what is most important for the AI to do
         *      - Likely will be very simple
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
            _graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
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
            ProcessButtonsOnly = true;

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

            _inputController.ProcessControls(gameTime, ProcessButtonsOnly);
            PressedButton = _inputController.GetButtonPressed();

            switch (menuState)
            {
                case MenuState.Initial:
                    menuState = _gameUI.Init(gameTime, PressedButton);
                    break;

                case MenuState.NewGame:
                    menuState = _gameUI.NewGame(gameTime, PressedButton);
                    Players = _gameUI.GetPlayers();
                    break;

                case MenuState.PlayingGame:
                    PlayingGame(gameTime);
                    break;

                case MenuState.GamePaused:
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

            _entityManager.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            _entityManager.Draw(_spriteBatch, gameTime);
            _spriteBatch.End();

            DrawRan = true;

            base.Draw(gameTime);
        }


        private void PlayingGame(GameTime gameTime)
        {
            if (TurnNumber == 0)
            {
                TurnNumber++;
                StartGame(gameTime);
                PlayerIndex = 0;
            }

            if (NextPlayer)
            {
                _unitManager.ResetUnitStates();
                _gameUI.DrawSelectedUI = false;
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
                Income(CurrentPlayer);

                //Need to sort out this method, refresh everything at start of turn
                gameState = _gameUI.Turn(gameTime, CurrentPlayer, TurnNumber, PressedButton);       

                //DEBUG
                Console.WriteLine("\nNew Turn");
                Console.WriteLine($"Player: {CurrentPlayer.Team + 1}");
                Console.WriteLine($"Turn: {TurnNumber}");
            }

            switch (gameState)
            {
                case GameState.PlayerSelect:
                    gameState = _gameUI.Turn(gameTime, CurrentPlayer, TurnNumber, PressedButton);
                    PlayerSelect(gameTime);
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

                case GameState.PlayerCapture:
                    PlayerCapture(gameTime);
                    break;

                case GameState.PlayerProduceUnit:
                    PlayerProduceUnit(gameTime);
                    break;

                case GameState.PauseGame:
                    menuState = MenuState.GamePaused;
                    break;

                case GameState.EnemyTurn:
                    NextPlayer = true;
                    PlayerIndex++;
                    break;

                case GameState.GameOver:
                    break;
            }
        }

        private void RefreshMap(int Width = 16, int Height = 16)
        {
            _entityManager.RemoveEntity(_gameMap);
            _gameMap = new MapManager(InGameAssets, Width, Height, Players.Count);
            _entityManager.AddEntity(_gameMap);

            _gameUI.ChangeMap(_gameMap);
            _inputController.ChangeMap(_gameMap);

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

        private void PlayerSelect(GameTime gameTime)            //Select logic needs to be redone to disable allowing the selection of "Used" units
        {
            ProcessButtonsOnly = false;
            UpdateUnitStats();

            //For if return is selected as a unit action
            if (SelectedUnit != null)           //Need to handle when player selects Idle for a unit
            {
                if (SelectedUnit.State != UnitState.Used)
                {
                    SelectedUnit.State = UnitState.None;
                }

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

                if (SelectedUnit.Team == CurrentPlayer.Team + 1 && SelectedUnit.State != UnitState.Used)    //Doesn't work properly?
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

                _gameUI.DisplayAttributes(null, SelectedTile);

                if (SelectedTile.Type == TileType.Factory
                    && SelectedTile.Team == CurrentPlayer.Team + 1
                   )
                {                                                    
                    gameState = GameState.PlayerProduceUnit;            //Issues here
                }
            }
        }

        private void PlayerSelectAction(GameTime gameTime, Button PressedButton)
        {
            ProcessButtonsOnly = true;
            _gameUI.DisplayAttributes(SelectedUnit);

            //ReachableTiles displays for a split second. Clear it after it's been used maybe?
            reachableTiles = _gameUI.GetReachableTiles(SelectedUnit, _unitManager.GetUnitPositions(), _inputController.GetUnitTile(SelectedUnit));
            attackableTiles = _gameUI.GetAttackableTiles(SelectedUnit, _inputController.GetUnitTile(SelectedUnit)); //Issue with attackable tiles
            //Needs to stop displaying after PlayerAttack has run

            DrawRan = false;

            bool displayCapture = false;
            Tile unitTile = _inputController.GetUnitTile(SelectedUnit);

            if ((unitTile.Type == TileType.City
                || unitTile.Type == TileType.Factory
                || unitTile.Type == TileType.HQ)
                && SelectedUnit.Team != unitTile.Team
                )
            {
                displayCapture = true;
            }

            gameState = _gameUI.DisplayPlayerActions(gameTime, PressedButton, displayCapture);
        }

        private void PlayerMove(GameTime gameTime)      //Needs fix - Player is able to move again after return and then reselection
        {
            if (SelectedUnit.State != UnitState.Moved)
            {
                while (gameState == GameState.PlayerMove && DrawRan)
                {
                    _inputController.UpdateMouseState();

                    foreach (Tile tile in reachableTiles)
                    {
                        if (
                            _inputController.MouseCollider.Intersects(tile.Collider)
                            && _inputController.LeftMouseClicked()
                            && SelectedUnit.Fuel > 0
                           )
                        {
                            SelectedUnit.Position = tile.Position;
                            SelectedUnit.Fuel--;
                            SelectedUnit.State = UnitState.Moved;

                            _gameUI.ChangeSelectedPosition(SelectedUnit.Position);

                            gameState = GameState.SelectAction;
                        }
                    }
                }
            }
            else
            {
                gameState = GameState.SelectAction;
            }
        }

        private void PlayerAttack(GameTime gameTime)
        {
            //Starting unit is attacked for some reason?
            //Should add counterattacking after a unit has attacked - need to include health in the damage calculation (fraction of unit health * damage)

            if (SelectedUnit.Type != UnitType.APC)
            {
                while (gameState == GameState.PlayerAttack && DrawRan)
                {
                    _inputController.UpdateMouseState();

                    foreach (Tile tile in attackableTiles)
                    {
                        if (
                            _inputController.MouseCollider.Intersects(tile.Collider)        
                            && _inputController.LeftMouseClicked()
                            && SelectedUnit.Ammo > 0
                           )
                        {
                            Unit defendingUnit = _inputController.GetTileUnit(tile);

                            SelectedUnit.Ammo--;
                            SelectedUnit.State = UnitState.Used;

                            defendingUnit.Health -= CalculateDamage(SelectedUnit, defendingUnit);

                            gameState = GameState.PlayerSelect;
                        }
                    }
                }
            }
            else
            {
                gameState = GameState.SelectAction;
            }
            
        }
        private void PlayerCapture(GameTime gameTime)
        {
            Tile unitTile = _inputController.GetUnitTile(SelectedUnit);
            unitTile.Team = SelectedUnit.Team;

            SelectedUnit.State = UnitState.Used;

            switch (unitTile.Type)
            {
                case TileType.City:
                    unitTile.CreateTile(-6 + SelectedUnit.Team);
                    break;

                case TileType.Factory:
                    unitTile.CreateTile(-6 + SelectedUnit.Team, 1);
                    break;

                case TileType.HQ:
                    unitTile.CreateTile(-6 + SelectedUnit.Team, 2);
                    break;
            }
            gameState = GameState.SelectAction;

        }

        private void PlayerProduceUnit(GameTime gameTime)               //WORKING ON THIS
        {
            while (gameState == GameState.PlayerProduceUnit && DrawRan)
            {
                int unitType = _gameUI.ProcessUnitProduction(gameTime, PressedButton);
                
                DrawRan = false; 

                if (unitType == -1)
                {
                    gameState = GameState.PlayerSelect;
                }
                
                if (unitType != -1 && unitType != -2)
                {
                    Unit newUnit = new Unit(InGameAssets, SelectedTile.Position, unitType, CurrentPlayer.Team + 1);

                    if (CurrentPlayer.Funds >= newUnit.CostToProduce)
                    {
                        CurrentPlayer.Funds -= newUnit.CostToProduce;

                        newUnit.State = UnitState.Used;            //Really need to make the player team and unitType universal values to avoid the +1, -1 issue
                        _unitManager.AddUnit(newUnit);

                        gameState = GameState.PlayerSelect;
                    }
  
                }
            }
        }

        private int CalculateDamage(Unit attackingUnit, Unit defendingUnit)
        {
            int damage = 0;

            int baseDamage = baseDamageDictionary[(attackingUnit.Type, defendingUnit.Type)];
            int defendingUnitDefenceBonus = _inputController.GetUnitTile(defendingUnit).DefenceBonus;

            damage = baseDamage - (int)(baseDamage * (defendingUnitDefenceBonus / 100));

            return damage;
        }

        private void UpdateUnitStats()
        {
            foreach (Unit unit in _unitManager.units)
            {
                unit.Defence = _inputController.GetUnitTile(unit).DefenceBonus;
            }
        }

        private void Income(Player player)
        {
            foreach (Tile structure in _gameMap.structures)
            {
                if (structure.Team == player.Team + 1)
                {
                    player.Funds += 1000;
                }
            }
        }
    }
}