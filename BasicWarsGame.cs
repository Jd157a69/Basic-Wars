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
                {(UnitType.Mech, UnitType.Tank), 65 },
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

        //DEBUG
        private bool DebugUnitFreeMove = true;

        /*  TODO: Code optimisations
         *      - A lot of repeated code in places
         *  
         *  DONE: Create a main game loop
         *      - Update method should loop through a list of players, going through each game state before moving onto the next player
         *      - This will introduce the Player class: potential use of built in Enum PlayerIndex?
         *      
         *  DONE: PausedGame state
         *      - Display options to user: Resume, Save, Menu, Quit
         *      
         *  DONE: GameOver condition and state
         *      - GameOver screen
         *      - Display Winner
         *      - FIXED: Return to menu not working
         *      - FIXED: Units are not removed after a team has lost
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
         *  DONE: Universalise the UnitTeam, PlayerTeam, UnitType
         *  
         *  TODO: Resupply using APC
         *      
         *  DONE: Adjustable map size
         *  
         *  DONE: Dead units are not removed from the game
         *  
         *  DONE: Fix memory leak
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
            IsFixedTimeStep = true;
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
        }

        protected override void Update(GameTime gameTime)
        {
            //_entityManager.Refresh();

            _inputController.ProcessControls(gameTime, ProcessButtonsOnly);
            PressedButton = _inputController.GetButtonPressed();

            _entityManager.Update(gameTime);

            switch (menuState)
            {
                case MenuState.Initial:
                    menuState = _gameUI.Init(gameTime, PressedButton);
                    break;

                case MenuState.NewGame:
                    _unitManager.ClearUnits();
                    TurnNumber = 0;
                    menuState = _gameUI.NewGame(gameTime, PressedButton);
                    break;

                case MenuState.IncreaseMapSize:
                    RefreshMap(_gameMap.MapWidth += 1, _gameMap.MapHeight += 1);
                    menuState = MenuState.NewGame;
                    break;

                case MenuState.DecreaseMapSize:
                    if (_gameMap.MapWidth > 6 && _gameMap.MapHeight > 6)
                    {
                        RefreshMap(_gameMap.MapWidth -= 1, _gameMap.MapHeight -= 1);
                    }
                    menuState = MenuState.NewGame;
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

                case MenuState.GameOver:
                    ProcessButtonsOnly = true;
                    menuState = _gameUI.GameOver(gameTime, PressedButton, CurrentPlayer);
                    break;

                case MenuState.RefreshMap:
                    RefreshMap(_gameMap.MapWidth, _gameMap.MapHeight);
                    break;

                case MenuState.LoadGame:
                    break;

                case MenuState.QuitGame:
                    Exit();
                    break;
            }

            DrawRan = true;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            _entityManager.Draw(_spriteBatch, gameTime);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void PlayingGame(GameTime gameTime)
        {
            if (TurnNumber == 0)
            {
                TurnNumber = 1;
                Players = _gameUI.GetPlayers();
                _gameMap.DrawMap = true;
                _unitManager.DrawUnits = true;

                PlayerIndex = 0;
                NextPlayer = true;
            }

            //Turns
            if (NextPlayer)
            {
                _unitManager.ResetUnitStates(CurrentPlayer);

                _gameUI.DrawSelectedUI = false;
                NextPlayer = false;

                if (PlayerIndex > Players.Count - 1)
                {
                    TurnNumber++;
                }

                if (PlayerIndex > Players.Count - 1)
                {
                    PlayerIndex = 0;
                }

                CurrentPlayer = Players[PlayerIndex];
                Income(CurrentPlayer);
                CheckUnitResupply(CurrentPlayer.Team);

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

                case GameState.UnitIdle:
                    SelectedUnit.State = UnitState.Used;
                    gameState = GameState.PlayerSelect;
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
            }
        }

        private void RefreshMap(int Width = 16, int Height = 16)
        {
            Players = _gameUI.GetPlayers();
            _entityManager.RemoveEntity(_gameMap);
            _gameMap = new MapManager(InGameAssets, Width, Height, Players.Count);
            _entityManager.AddEntity(_gameMap);

            _gameUI.ChangeMap(_gameMap);
            _inputController.ChangeMap(_gameMap);

            menuState = MenuState.NewGame;
        }

        private void LoadGame(GameTime gameTime)
        {

        }

        //Player Select logic needs tidying up
        private void PlayerSelect(GameTime gameTime)
        {
            ProcessButtonsOnly = false;
            UpdateUnitStats();

            _gameUI.ClearAttackableOverlay();

            //For if return or idle is selected as a unit action 
            if (SelectedUnit != null)
            {
                _gameUI.DisplayAttributes(SelectedUnit);

                if (SelectedUnit.State != UnitState.Used
                    && SelectedUnit.State != UnitState.Moved
                    && SelectedUnit.State != UnitState.Dead
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
                Console.WriteLine($"{SelectedUnit.State}");

                _gameUI.ChangeSelectedPosition(SelectedUnit.Position);
                _gameUI.DrawSelectedUI = true;
                SelectedUnit.Selected = true;

                _gameUI.DisplayAttributes(SelectedUnit);

                if (SelectedUnit.Team == CurrentPlayer.Team
                    && SelectedUnit.State != UnitState.Used
                   )
                {
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

                if ((SelectedTile.Type == TileType.Factory 
                    || SelectedTile.Type == TileType.HQ)
                    && SelectedTile.Team == CurrentPlayer.Team
                   )
                {
                    ProcessButtonsOnly = true;
                    gameState = GameState.PlayerProduceUnit;
                }
            }
        }

        private void PlayerSelectAction(GameTime gameTime, Button PressedButton)
        {
            ProcessButtonsOnly = true;
            _gameUI.DisplayAttributes(SelectedUnit);

            //Repeated code
            if (SelectedUnit.State != UnitState.Moved 
                && SelectedUnit.State != UnitState.Used
                //DEBUG
                || DebugUnitFreeMove
               )
            {
                reachableTiles = _gameUI.GetReachableTiles(SelectedUnit, _unitManager.GetUnitPositions(), _inputController.GetUnitTile(SelectedUnit));
            }
            else
            {
                reachableTiles.Clear();
                _gameUI.ClearMoveableOverlay();
            }
            
            attackableTiles = _gameUI.GetAttackableTiles(SelectedUnit, _inputController.GetUnitTile(SelectedUnit));  

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
            if (reachableTiles.Count != 0)
            {
                while (gameState == GameState.PlayerMove && DrawRan)
                {
                    DrawRan = false;

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

                    if (SelectedUnit.Fuel <= 0)
                    {
                        gameState = GameState.SelectAction;
                    }
                }
            }
            else
            {
                gameState = GameState.PlayerSelect;
            }
        }

        private void PlayerAttack(GameTime gameTime)
        {
            if (SelectedUnit.State != UnitState.Used 
                && SelectedUnit.Type != UnitType.APC 
                && attackableTiles.Count != 0
               )
            {
                while (gameState == GameState.PlayerAttack && DrawRan)
                {
                    DrawRan = false;

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
                            if (defendingUnit.Health > 0)
                            {
                                SelectedUnit.Health -= CalculateDamage(defendingUnit, SelectedUnit);
                            }

                            SelectedUnit.State = UnitState.Used;
                            gameState = GameState.PlayerSelect;
                        }
                    }
                }
            }
            else
            {
                gameState = GameState.PlayerSelect;
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
                    unitTile.CreateTileSprite(-5 + SelectedUnit.Team);
                    break;

                case TileType.Factory:
                    unitTile.CreateTileSprite(-5 + SelectedUnit.Team, 1);
                    break;

                case TileType.HQ:
                    unitTile.CreateTileSprite(-5 + SelectedUnit.Team, 2);
                    break;
            }

            bool GameOver = CheckWinner(CurrentPlayer.Team);
            CheckHQ();

            if (GameOver)
            {
                menuState = MenuState.GameOver;
            }

            gameState = GameState.PlayerSelect;
        }

        private void PlayerProduceUnit(GameTime gameTime)               
        {
            while (gameState == GameState.PlayerProduceUnit && DrawRan)
            {
                DrawRan = false;

                int unitType = _gameUI.ProcessUnitProduction(gameTime, PressedButton);

                if (unitType == -1)
                {
                    gameState = GameState.PlayerSelect;
                }
                
                if (unitType != -1 && unitType != -2)
                {
                    Unit newUnit = new Unit(InGameAssets, SelectedTile.Position, unitType, CurrentPlayer.Team);

                    if (CurrentPlayer.Funds >= newUnit.CostToProduce)
                    {
                        CurrentPlayer.Funds -= newUnit.CostToProduce;

                        newUnit.State = UnitState.Used;
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
                if (structure.Team == player.Team)
                {
                    player.Funds += 1000;
                }
            }
        }

        private void CheckUnitResupply(int Team)
        {
            foreach (Unit unit in _unitManager.units)
            {
                Tile unitTile = _inputController.GetUnitTile(unit);

                if (unit.Team == Team
                    && (unitTile.Type == TileType.HQ
                    || unitTile.Type == TileType.City
                    || unitTile.Type == TileType.Factory)
                   )
                {
                    unit.RefreshUnitAttributes();
                }
            }
        }

        private bool CheckWinner(int Team)
        {
            foreach (Tile HQ in _gameMap.HQs)
            {
                if (HQ.Team != Team)
                {
                    return false;
                }
            }
            return true;
        }

        private void CheckHQ()
        {
            List<Player> playersToRemove = new List<Player>();

            foreach (Player player in Players)
            {
                foreach (Tile HQ in _gameMap.HQs)
                {
                    if (HQ.Team == player.Team)
                    {
                        player.HasHQ = true;
                        break;
                    }
                    else
                    {
                        player.HasHQ = false;
                    }
                }

                if (!player.HasHQ)
                {
                    playersToRemove.Add(player);
                    foreach (Unit unit in _unitManager.units)
                    {
                        if (unit.Team == player.Team)
                        {
                            _unitManager.RemoveUnit(unit);
                        }
                    }

                    foreach (Tile structure  in _gameMap.structures)
                    {
                        if (structure.Team == player.Team)
                        {
                            structure.Team = -1;

                            switch (structure.Type)
                            {
                                case TileType.City:
                                    structure.CreateTileSprite(-6);
                                    break;

                                case TileType.Factory:
                                    structure.CreateTileSprite(-6, 1);
                                    break;
                            }
                        }
                    }

                }
            }

            foreach (Player player in playersToRemove)
            {
                Players.Remove(player);
            }
        }
    }
}