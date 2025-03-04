﻿using Basic_Wars_V2.Entities;
using Basic_Wars_V2.Enums;
using Basic_Wars_V2.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Basic_Wars_V2
{
    public class BasicWarsGame : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private const string ASSET_NAME_IN_GAME_ASSETS = "InGameAssets";
        private const string ASSET_NAME_GAMEFONT = "Font";
        private const string SAVE_GAME_PATH = "GameData.xml";

        private Texture2D InGameAssets;
        private SpriteFont Font;

        private const int WINDOW_WIDTH = 1920;
        private const int WINDOW_HEIGHT = 1080;

        private GameState gameState;
        private MenuState menuState;

        private List<Player> Players;

        private AI Computer;
        private bool AddAI;

        private Player CurrentPlayer;
        private int CurrentPlayerIndex;
        private bool NextPlayer;

        private InputController _inputController;
        private bool ProcessButtonsOnly;

        private GameUI _gameUI;
        private EntityManager _entityManager;
        private UnitManager _unitManager;
        private ButtonManager _buttonManager;
        private MapManager _gameMap;

        private Unit SelectedUnit;
        private Tile CurrentUnitTile;
        private List<Tile> reachableTiles;
        private List<Tile> attackableTiles;

        private Tile SelectedTile;

        private Button PressedButton;

        private int TurnNumber;

        private bool DrawRan;

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

            AddAI = false;

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
            _inputController.ProcessControls(gameTime, ProcessButtonsOnly);
            PressedButton = _inputController.GetButtonPressed();

            _entityManager.Update(gameTime);

            switch (menuState)
            {
                case MenuState.Initial:
                    _unitManager.ClearUnits();
                    _gameMap.DrawMap = false;
                    menuState = _gameUI.Init(gameTime, PressedButton);
                    break;

                case MenuState.NewGame:
                    _unitManager.ClearUnits();
                    TurnNumber = 0;
                    _gameMap.DrawMap = true;
                    menuState = _gameUI.NewGame(gameTime, PressedButton);
                    break;

                case MenuState.AddAI:
                    AddAI = true;
                    menuState = MenuState.NewGame;
                    break;

                case MenuState.RemoveAI:
                    AddAI = false;
                    menuState = MenuState.NewGame;
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
                    SaveGame(gameTime);
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
                    LoadGame(gameTime);
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
                _unitManager.DrawUnits = true;
                CurrentPlayerIndex = 0;
                NextPlayer = true;

                if (AddAI)
                {
                    Players[Players.Count - 1].IsAI = true;
                    Computer = new AI(Players.Count - 1, 0, _gameMap, _unitManager, _gameUI, _inputController, InGameAssets);
                }
            }

            if (NextPlayer)
            {
                _unitManager.ResetUnitStates(CurrentPlayer);

                _gameUI.DrawSelectedUI = false;
                NextPlayer = false;

                if (CurrentPlayerIndex > Players.Count - 1)
                {
                    TurnNumber++;
                }

                if (CurrentPlayerIndex > Players.Count - 1)
                {
                    CurrentPlayerIndex = 0;
                }

                CurrentPlayer = Players[CurrentPlayerIndex];
                Income(CurrentPlayer);

                if (CurrentPlayer.IsAI)
                {
                    gameState = GameState.AITurn;
                }
                else
                {
                    gameState = _gameUI.Turn(gameTime, CurrentPlayer, TurnNumber, PressedButton);
                }
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

                case GameState.PlayerResupply:
                    PlayerResupply(gameTime);
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
                    CurrentPlayerIndex++;
                    break;

                case GameState.AITurn:
                    foreach (Tile structure in _gameMap.Structures)
                    {
                        if (structure.Team == Computer.Team)
                        {
                            Computer.Funds += 1000;
                        }
                    }
                    Computer.RunAILogic();
                    bool GameOver = CheckWinner(Computer.Team);
                    if (GameOver)
                    {
                        menuState = MenuState.GameOver;
                    }
                    else
                    {
                        gameState = GameState.EnemyTurn;
                    }
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
            CurrentUnitTile = _inputController.GetUnitTile(SelectedUnit);

            ProcessButtonsOnly = true;
            _gameUI.DisplayAttributes(SelectedUnit);

            if (SelectedUnit.State != UnitState.Moved
                && SelectedUnit.State != UnitState.Used
               )
            {
                reachableTiles = _gameUI.GetReachableTiles(SelectedUnit, _unitManager.GetUnitPositions(), CurrentUnitTile);
            }
            else
            {
                reachableTiles.Clear();
                _gameUI.ClearMoveableOverlay();
            }

            attackableTiles = _gameUI.GetAttackableTiles(SelectedUnit, CurrentUnitTile);

            bool displayCapture = false;
            bool displayResupply = false;

            if ((CurrentUnitTile.Type == TileType.City
                || CurrentUnitTile.Type == TileType.Factory
                || CurrentUnitTile.Type == TileType.HQ)
                && SelectedUnit.Type != UnitType.Tank
                && SelectedUnit.Type != UnitType.APC
                && SelectedUnit.Team != CurrentUnitTile.Team
                )
            {
                displayCapture = true;
            }

            if (
                (
                 (CurrentUnitTile.Type == TileType.City
                 || CurrentUnitTile.Type == TileType.Factory
                 || CurrentUnitTile.Type == TileType.HQ
                )
                 && SelectedUnit.Team == CurrentUnitTile.Team
                )
                 || SelectedUnit.Type == UnitType.APC
               )
            {
                displayResupply = true;
            }

            gameState = _gameUI.DisplayPlayerActions(gameTime, PressedButton, displayCapture, displayResupply);
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

                            defendingUnit.Health -= _gameUI.CalculateDamage(SelectedUnit, defendingUnit);
                            if (defendingUnit.Health > 0 && defendingUnit.Ammo > 0)
                            {
                                SelectedUnit.Health -= _gameUI.CalculateDamage(defendingUnit, SelectedUnit);
                                defendingUnit.Ammo--;
                            }

                            SelectedUnit.State = UnitState.Used;
                            gameState = GameState.PlayerSelect;
                        }

                        if (SelectedUnit.Ammo <= 0)
                        {
                            gameState = GameState.SelectAction;
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
            CurrentUnitTile.Team = SelectedUnit.Team;
            SelectedUnit.State = UnitState.Used;

            CurrentUnitTile.CreateTileSpriteOnType();

            bool GameOver = CheckWinner(CurrentPlayer.Team);

            CheckHQ();

            if (GameOver)
            {
                menuState = MenuState.GameOver;
            }

            gameState = GameState.PlayerSelect;
        }

        private void PlayerResupply(GameTime gameTime)
        {
            if (SelectedUnit.Type == UnitType.APC)
            {
                SelectedUnit.State = UnitState.Used;

                List<Tile> neighbours = _gameMap.GetNeighbours(CurrentUnitTile);

                foreach (Tile neighbour in neighbours)
                {
                    Unit neighbouringUnit = _inputController.GetTileUnit(neighbour);
                    if (neighbouringUnit != null && neighbouringUnit.Team == SelectedUnit.Team)
                    {
                        neighbouringUnit.RefreshUnitAttributes();
                    }
                }

                if ((CurrentUnitTile.Type == TileType.City
                    || CurrentUnitTile.Type == TileType.Factory
                    || CurrentUnitTile.Type == TileType.HQ)
                    && CurrentUnitTile.Team == SelectedUnit.Team
                    )
                {
                    SelectedUnit.RefreshUnitAttributes();
                }

                gameState = GameState.PlayerSelect;
            }
            else
            {
                SelectedUnit.State = UnitState.Used;
                SelectedUnit.RefreshUnitAttributes();

                gameState = GameState.PlayerSelect;
            }
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
                    Unit newUnit = new(InGameAssets, SelectedTile.Position, unitType, CurrentPlayer.Team);

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

        private void UpdateUnitStats()
        {
            foreach (Unit unit in _unitManager.units)
            {
                unit.Defence = _inputController.GetUnitTile(unit).DefenceBonus;
            }
        }

        private void Income(Player player)
        {
            foreach (Tile structure in _gameMap.Structures)
            {
                if (structure.Team == player.Team)
                {
                    player.Funds += 1000;
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
            List<Player> playersToRemove = new();

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

                    foreach (Tile structure in _gameMap.Structures)
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

        private void SaveGame(GameTime gameTime)
        {

            List<UnitData> unitData = new();
            List<TileData> mapData = new();
            List<TileData> structuresData = new();
            List<PlayerData> playerData = new();

            GameStateData gameStateData = new(TurnNumber, CurrentPlayerIndex, AddAI);

            AIData ComputerData = null;

            if (AddAI)
            {
                ComputerData = new AIData(Computer);
            }

            foreach (Unit unit in _unitManager.units)
            {
                unitData.Add(new UnitData(unit));
            }

            foreach (Tile tile in _gameMap.Map)
            {
                mapData.Add(new TileData(tile));
            }

            foreach (Tile tile in _gameMap.Structures)
            {
                if (tile.Type != TileType.HQ)
                {
                    structuresData.Add(new TileData(tile));
                }
            }

            foreach (Player player in Players)
            {
                playerData.Add(new PlayerData(player));
            }

            GameData gameData;
            if (AddAI)
            {
                gameData = new GameData(unitData, mapData, structuresData, playerData, gameStateData, _gameMap.MapWidth, _gameMap.MapHeight, ComputerData);
            }
            else
            {
                gameData = new GameData(unitData, mapData, structuresData, playerData, gameStateData, _gameMap.MapWidth, _gameMap.MapHeight);
            }

            XmlSerializer serializer = new(typeof(GameData));
            using (StreamWriter streamWriter = new(SAVE_GAME_PATH))
            {
                serializer.Serialize(streamWriter, gameData);
            }

            menuState = MenuState.PlayingGame;
        }

        private void LoadGame(GameTime gameTime)
        {
            if (File.Exists(SAVE_GAME_PATH))
            {
                _unitManager.DrawUnits = true;
                _gameMap.DrawMap = true;

                _unitManager.ClearUnits();
                Players.Clear();
                _gameMap.ClearMap();
                _gameMap.Structures.Clear();
                _gameMap.HQs.Clear();

                GameData gameData;

                List<Tile> Map = new();

                XmlSerializer serializer = new(typeof(GameData));
                using (StreamReader streamReader = new(SAVE_GAME_PATH))
                {
                    gameData = (GameData)serializer.Deserialize(streamReader);
                }

                foreach (UnitData unitData in gameData.Units)
                {
                    _unitManager.AddUnit(unitData.FromUnitData(InGameAssets));
                }

                foreach (TileData tileData in gameData.Map)
                {
                    Map.Add(tileData.FromTileData(InGameAssets));
                }

                foreach (PlayerData players in gameData.Players)
                {
                    Players.Add(players.FromPlayerData());
                }

                foreach (TileData structure in gameData.Structures)
                {
                    _gameMap.Structures.Add(structure.FromTileData(InGameAssets));
                }

                _gameMap.MapWidth = gameData.MapWidth;
                _gameMap.MapHeight = gameData.MapHeight;

                _gameMap.ResetMapSize();

                foreach (Tile tile in Map)
                {
                    _gameMap.Map[(int)tile.MapGridPos.X, (int)tile.MapGridPos.Y] = tile;

                    if (tile.Type == TileType.HQ)
                    {
                        _gameMap.HQs.Add(tile);
                    }
                }

                _gameMap.RegenerateMap();

                AddAI = gameData.GameStateData.AddAI;

                CurrentPlayerIndex = gameData.GameStateData.CurrentPlayerIndex;
                TurnNumber = gameData.GameStateData.TurnNumber;

                CurrentPlayer = Players[CurrentPlayerIndex];

                if (CurrentPlayer.IsAI)
                {
                    Computer = gameData.Computer.FromAIData(InGameAssets, _gameMap, _unitManager, _gameUI, _inputController);
                    gameState = GameState.AITurn;
                }

                menuState = MenuState.PlayingGame;
            }
            else
            {
                menuState = MenuState.Initial;
            }
        }
    }
}