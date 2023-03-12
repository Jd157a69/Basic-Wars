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
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace Basic_Wars_V2
{
    public class BasicWarsGame : Game
    {
        readonly private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private const string ASSET_NAME_IN_GAME_ASSETS = "InGameAssets";
        private const string ASSET_NAME_GAMEFONT = "Font";

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
         *      - A lot of repeated code in places
         *  
         *  TODO: Create a main game loop
         *      - Update method should loop through a list of players, going through each game state before moving onto the next player
         *      - This will introduce the Player class: potential use of built in Enum PlayerIndex?
         *      
         *  TODO: PausedGame state
         *      - Display options to user: Resume, Save, Menu, Quit
         *      
         *  TODO: GameOver condition and state
         *      - GameOver screen
         *      - Display Winner
         *      - Return to menu
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
         *  DONE: Ability for units to attack each other 
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
         *      - Get data method in MapManager, UnitManager, Player and BasicWarGame classes
         *      - Method in BasciWarsGame class to load and save data
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

                case MenuState.SaveGame:
                    break;

                case MenuState.PlayingGame:
                    PlayingGame(gameTime);
                    break;

                case MenuState.GamePaused:
                    ProcessButtonsOnly = true;
                    menuState = _gameUI.PausedGame(gameTime, PressedButton);
                    gameState = GameState.PlayerSelect;
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
                    _gameUI.DrawSelectedUI = false;
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

            _gameUI.ClearAttackableOverlay();

            //Need some indication of when a unit has been used
            //For if return is selected as a unit action
            if (SelectedUnit != null)           //Need to handle when player selects Idle for a unit
            {
                if (SelectedUnit.State != UnitState.Used
                    && SelectedUnit.State != UnitState.Moved
                   )
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

                if (SelectedUnit.Team == CurrentPlayer.Team + 1
                    && SelectedUnit.State != UnitState.Used
                   )    //Doesn't work properly?
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
                    gameState = GameState.PlayerProduceUnit;
                }
            }
        }

        private void PlayerSelectAction(GameTime gameTime, Button PressedButton)
        {
            ProcessButtonsOnly = true;
            _gameUI.DisplayAttributes(SelectedUnit);

            //Likely somewhere else I can put this
            if (SelectedUnit.State != UnitState.Moved)
            {
                reachableTiles = _gameUI.GetReachableTiles(SelectedUnit, _unitManager.GetUnitPositions(), _inputController.GetUnitTile(SelectedUnit));
            }
            else
            {
                reachableTiles.Clear();
                _gameUI.ClearMoveableOverlay();
            }
            
            attackableTiles = _gameUI.GetAttackableTiles(SelectedUnit, _inputController.GetUnitTile(SelectedUnit));

            DrawRan = false;

            bool displayCapture = false;
            Tile unitTile = _inputController.GetUnitTile(SelectedUnit);

            if ((unitTile.Type == TileType.City
                || unitTile.Type == TileType.Factory
                || unitTile.Type == TileType.HQ)
                && SelectedUnit.Type != UnitType.Tank
                && SelectedUnit.Type != UnitType.APC
                && SelectedUnit.Team != unitTile.Team
                )
            {
                displayCapture = true;
            }

            gameState = _gameUI.DisplayPlayerActions(gameTime, PressedButton, displayCapture);
        }

        private void PlayerMove(GameTime gameTime) 
        {
            if (SelectedUnit.State != UnitState.Moved 
                && reachableTiles.Count != 0
               )
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

            if (SelectedUnit.State != UnitState.Used 
                && SelectedUnit.Type != UnitType.APC 
                && attackableTiles.Count != 0
               )
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

                            defendingUnit.Health -= CalculateDamage(SelectedUnit, defendingUnit);
                            SelectedUnit.Health -= CalculateDamage(defendingUnit, SelectedUnit);

                            SelectedUnit.State = UnitState.Used;
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
                    unitTile.CreateTileSprite(-6 + SelectedUnit.Team);
                    break;

                case TileType.Factory:
                    unitTile.CreateTileSprite(-6 + SelectedUnit.Team, 1);
                    break;

                case TileType.HQ:
                    unitTile.CreateTileSprite(-6 + SelectedUnit.Team, 2);
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

                        newUnit.State = UnitState.Used;            //Really need to make the player team and unitType universal values to avoid the +1, -1
                        _unitManager.AddUnit(newUnit);

                        gameState = GameState.PlayerSelect;
                    }
  
                }
            }
        }

        private int CalculateDamage(Unit attackingUnit, Unit defendingUnit)
        {
            attackingUnit.Ammo--;

            int baseDamage = baseDamageDictionary[(attackingUnit.Type, defendingUnit.Type)];

            double defenceMultiplier = (double)_inputController.GetUnitTile(defendingUnit).DefenceBonus / 100;

            double HealthMultiplier = (double)attackingUnit.Health / 100;

            //return (int)(HealthMultiplier * (baseDamage - (int)(baseDamage * ((double)defenceMultiplier / 100))));
            return (int)(HealthMultiplier * (baseDamage - (baseDamage * defenceMultiplier)));
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