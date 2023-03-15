using Basic_Wars_V2.Enums;
using Basic_Wars_V2.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Basic_Wars_V2.Entities
{
    public class GameUI : IGameEntity
    {
        private readonly Dictionary<(UnitType, UnitType), int> baseDamageDictionary = new()
        {
                {(UnitType.Infantry, UnitType.Infantry), 55 },
                {(UnitType.Infantry, UnitType.Mech), 40 },
                {(UnitType.Infantry, UnitType.Tank), 10 },
                {(UnitType.Infantry, UnitType.APC), 5 },
                {(UnitType.Mech, UnitType.Infantry), 65 },
                {(UnitType.Mech, UnitType.Mech), 55 },
                {(UnitType.Mech, UnitType.Tank), 60 },
                {(UnitType.Mech, UnitType.APC), 75 },
                {(UnitType.Tank, UnitType.Infantry), 75 },
                {(UnitType.Tank, UnitType.Mech), 70 },
                {(UnitType.Tank, UnitType.Tank), 55 },
                {(UnitType.Tank, UnitType.APC), 100 },
        };

        //Init
        private Button BasicWarsTitle;
        private Button NewGameButton;
        private Button LoadGameButton;
        private Button QuitGameButton;

        //NewGame
        private Button NumOfPlayersInfo;
        private Button Players2Button;
        private Button Players3Button;
        private Button Players4Button;
        private Button RefreshMapButton;
        private Button StartGameButton;
        private Button MenuButton;

        //Turn
        private Button TurnNumberInfo;
        private Button CurrentPlayerTeamInfo;
        private Button CurrentPlayerFundsInfo;
        private Button EndTurnButton;
        private Button PauseGameButton;

        //DisplayActions
        private Button PlayerIdleButton;
        private Button PlayerMoveButton;
        private Button PlayerAttackButton;
        private Button ReturnButton;
        private Button CaptureButton;

        //DisplayAttributes
        private Button AttributeDisplayInfo;
        private Button TypeInfo;
        private Button HealthInfo;
        private Button AmmoInfo;
        private Button FuelInfo;
        private Button DefenceInfo;

        //DisplayProductionChoices
        private Button UnitInfantryButton;
        private Button UnitMechButton;
        private Button UnitTankButton;
        private Button UnitAPCButton;

        //Paused Game
        private Button ResumeGameButton;
        private Button SaveGameButton;
        private Button MainMenuButton;

        //Game Over Screen
        private Button GameOverInfo;
        private Button WinnerInfo;

        private Button IncreaseMapSizeButton;
        private Button DecreaseMapSizeButton;

        private Button UnitResupplyButton;

        private Button AddAIInfo;
        private Button AIPlayerTrue;
        private Button AIPlayerFalse;

        private readonly Tile SelectedUI;

        public bool DrawSelectedUI { get; set; }

        private List<Tile> reachableTiles = new();
        private readonly List<Tile> moveableOverlay = new();
        private bool DrawReachable = false;

        private readonly List<Tile> attackableTiles = new();
        private readonly List<Tile> attackableOverlay = new();
        private bool DrawAttackable = false;

        private readonly List<Tile> tilesToBeRemoved = new();

        private readonly Texture2D Texture;
        private readonly SpriteFont Font;

        private readonly ButtonManager _buttonManager;
        private readonly UnitManager _unitManager;
        private Dijkstra _pathFinder;

        private MapManager _gameMap;

        private int numOfplayers = 2;

        private readonly int CentreButtonX = (1920 - 672) / 2;

        private Player CurrentPlayer;
        private Unit CurrentUnit;
        private Tile CurrentTile;

        public int DrawOrder => 2;

        public GameUI(Texture2D SpriteSheet, SpriteFont font, MapManager map, UnitManager unitManager, ButtonManager buttonManager)
        {
            Texture = SpriteSheet;
            Font = font;
            _gameMap = map;
            _pathFinder = new Dijkstra(map);
            _unitManager = unitManager;

            Player placeHolder = new(0, 0);
            CurrentPlayer = placeHolder;

            _buttonManager = buttonManager;
            InitialiseButtons();

            DrawSelectedUI = false;

            SelectedUI = new Tile(new Vector2(0, 0), Texture);
            SelectedUI.CreateTileSprite(0, 1);
        }

        public void InitialiseButtons()
        {

            BasicWarsTitle = new(Texture, Font, new Vector2(CentreButtonX, 90), 0, "Basic Wars");
            NewGameButton = new(Texture, Font, new Vector2(CentreButtonX, 360), 0, "New Game");
            LoadGameButton = new(Texture, Font, new Vector2(CentreButtonX, 540), 0, "Load Game");
            QuitGameButton = new(Texture, Font, new Vector2(CentreButtonX, 720), 0, "Quit");

            NumOfPlayersInfo = new(Texture, Font, new Vector2(CentreButtonX + 615, 0), 0, $"Number of Players: {numOfplayers}");
            Players2Button = new(Texture, Font, new Vector2(1600, 180), 1, "2");
            Players3Button = new(Texture, Font, new Vector2(1600, 304), 1, "3");
            Players4Button = new(Texture, Font, new Vector2(1600, 428), 1, "4");
            RefreshMapButton = new(Texture, Font, new Vector2(0, 720), 1, "Refresh");
            StartGameButton = new(Texture, Font, new Vector2(CentreButtonX, 900), 0, "Start Game");
            MenuButton = new(Texture, Font, new Vector2(0, 0), 1, "Menu");

            EndTurnButton = new(Texture, Font, new Vector2(1600, 925), 1, $"End Turn");

            TurnNumberInfo = new(Texture, Font, new Vector2(1600, 0), 1, $"Turn: 0");
            CurrentPlayerTeamInfo = new(Texture, Font, new Vector2(131, 0), 1, $"Player: {CurrentPlayer.Team}");
            CurrentPlayerFundsInfo = new(Texture, Font, new Vector2(0, 126), 1, $"{CurrentPlayer.Funds}");
            PauseGameButton = new(Texture, Font, new Vector2(0, 0), 3);

            PlayerIdleButton = new(Texture, Font, new Vector2(0, 300), 1, "Idle");
            PlayerMoveButton = new(Texture, Font, new Vector2(0, 425), 1, "Move");
            PlayerAttackButton = new(Texture, Font, new Vector2(0, 550), 1, "Attack");
            ReturnButton = new(Texture, Font, new Vector2(0, 925), 1, "Return");
            CaptureButton = new(Texture, Font, new Vector2(0, 675), 1, "Capture");

            AttributeDisplayInfo = new(Texture, Font, new Vector2(1600, 180), 2);
            TypeInfo = new(Texture, Font, new Vector2(1600, 550), 1);
            HealthInfo = new(Texture, Font, new Vector2(1625, 274.5f - 110));
            AmmoInfo = new(Texture, Font, new Vector2(1625, 369 - 110));
            FuelInfo = new(Texture, Font, new Vector2(1625, 463.5f - 110));
            DefenceInfo = new(Texture, Font, new Vector2(1625, 558 - 110));

            UnitInfantryButton = new(Texture, Font, new Vector2(0, 300), 1, "Infantry\n1000");
            UnitMechButton = new(Texture, Font, new Vector2(0, 425), 1, "Mech\n3000");
            UnitTankButton = new(Texture, Font, new Vector2(0, 550), 1, "Tank\n7000");
            UnitAPCButton = new(Texture, Font, new Vector2(0, 675), 1, "APC\n5000");

            ResumeGameButton = new(Texture, Font, new Vector2(CentreButtonX, 180), 0, "Resume");
            SaveGameButton = new(Texture, Font, new Vector2(CentreButtonX, 305), 0, "Save");
            MainMenuButton = new(Texture, Font, new Vector2(CentreButtonX, 430), 0, "Menu");

            GameOverInfo = new(Texture, Font, new Vector2(CentreButtonX, 90), 0, "Game Over");
            WinnerInfo = new(Texture, Font, new Vector2(CentreButtonX, 215), 0);

            IncreaseMapSizeButton = new(Texture, Font, new Vector2(1600, 600), 1, "+");
            DecreaseMapSizeButton = new(Texture, Font, new Vector2(1600, 725), 1, "-");

            UnitResupplyButton = new(Texture, Font, new Vector2(0, 800), 1, "Resupply");

            AddAIInfo = new(Texture, Font, new Vector2(0, 300), 1, "AI Player:\nFalse");
            AIPlayerTrue = new(Texture, Font, new Vector2(0, 425), 1, "True");
            AIPlayerFalse = new(Texture, Font, new Vector2(0, 550), 1, "False");

            _buttonManager.AddButtons(new List<Button>()
            {
                BasicWarsTitle,
                NewGameButton,
                LoadGameButton,
                QuitGameButton,

                NumOfPlayersInfo,
                Players2Button,
                Players3Button,
                Players4Button,
                RefreshMapButton,
                StartGameButton,
                MenuButton,

                TurnNumberInfo,
                CurrentPlayerTeamInfo,
                CurrentPlayerFundsInfo,
                EndTurnButton,
                PauseGameButton,

                PlayerIdleButton,
                PlayerMoveButton,
                PlayerAttackButton,
                ReturnButton,
                CaptureButton,

                AttributeDisplayInfo,
                HealthInfo,
                AmmoInfo,
                FuelInfo,
                DefenceInfo,
                TypeInfo,

                UnitInfantryButton,
                UnitMechButton,
                UnitTankButton,
                UnitAPCButton,

                ResumeGameButton,
                SaveGameButton,
                MainMenuButton,

                GameOverInfo,
                WinnerInfo,

                IncreaseMapSizeButton,
                DecreaseMapSizeButton,

                UnitResupplyButton,

                AddAIInfo,
                AIPlayerTrue,
                AIPlayerFalse,
            });
        }

        public void ChangeSelectedPosition(Vector2 position)
        {
            SelectedUI.Position = position;
        }

        public List<Tile> GetReachableTiles(Unit unit, List<Vector2> UnitPositions, Tile StartingTile)
        {
            reachableTiles.Clear();
            moveableOverlay.Clear();

            reachableTiles = _pathFinder.FindReachableTiles(StartingTile, unit);

            foreach (Vector2 position in UnitPositions)
            {
                foreach (Tile tile in reachableTiles)
                {
                    if (position == tile.Position && position != unit.Position)
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
                if (tile.Position != unit.Position)
                {
                    if (
                        !(tile.Type == TileType.Mountain
                        && unit.Type == UnitType.Tank)
                        && !(tile.Type == TileType.Mountain
                        && unit.Type == UnitType.APC)
                       )
                    {
                        Tile overlayTile = new(tile.Position, Texture);
                        overlayTile.CreateTileSprite(2, 1);
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
                reachableTiles.Remove(tile);
            }
            tilesToBeRemoved.Clear();

            return reachableTiles;
        }

        public List<Tile> GetAttackableTiles(Unit attackingUnit, Tile StartingTile)
        {
            attackableTiles.Clear();
            attackableOverlay.Clear();

            List<Tile> adjacentTiles = _gameMap.GetNeighbours(StartingTile);

            foreach (Tile tile in adjacentTiles)
            {
                foreach (Unit defendingUnit in _unitManager.units)
                {
                    if (defendingUnit.Position == tile.Position
                        && defendingUnit.Team != attackingUnit.Team
                       )
                    {
                        Tile overlayTile = new(tile.Position, Texture);
                        overlayTile.CreateTileSprite(1, 1);
                        attackableOverlay.Add(overlayTile);

                        attackableTiles.Add(tile);
                    }
                }
            }

            return attackableTiles;
        }

        public void ChangeMap(MapManager gameMap)
        {
            _gameMap = gameMap;
            _pathFinder = new Dijkstra(gameMap);
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(SpriteBatch _spriteBatch, GameTime gameTime)
        {
            if (DrawSelectedUI)
            {
                SelectedUI.Draw(_spriteBatch, gameTime);
            }

            if (DrawReachable)
            {
                foreach (Tile tile in moveableOverlay)
                {
                    tile.Draw(_spriteBatch, gameTime);
                }
            }

            if (DrawAttackable)
            {
                foreach (Tile tile in attackableOverlay)
                {
                    tile.Draw(_spriteBatch, gameTime);
                }
            }

            _buttonManager.Draw(_spriteBatch, gameTime);
        }

        public MenuState Init(GameTime gameTime, Button PressedButton)
        {
            _buttonManager.DrawButtonIDs(0, 3);

            if (PressedButton != null)
            {
                switch (PressedButton.ID)
                {
                    case 1:
                        return MenuState.RefreshMap;

                    case 2:
                        return MenuState.LoadGame;

                    case 3:
                        return MenuState.QuitGame;
                }
            }

            return MenuState.Initial;
        }

        public MenuState NewGame(GameTime gameTime, Button PressedButton)
        {
            _buttonManager.UpdateButtonText(CurrentPlayerTeamInfo, "");
            _buttonManager.UpdateButtonText(CurrentPlayerFundsInfo, "");
            _buttonManager.UpdateButtonText(TurnNumberInfo, "");

            _buttonManager.DrawButtonIDs(4, 10, 36, 37, 39, 41);

            if (PressedButton != null)
            {
                switch (PressedButton.ID)
                {
                    case 5:
                        numOfplayers = 2;
                        _buttonManager.UpdateButtonText(NumOfPlayersInfo, $"Number of Players: {numOfplayers}");
                        return MenuState.RefreshMap;

                    case 6:
                        numOfplayers = 3;
                        _buttonManager.UpdateButtonText(NumOfPlayersInfo, $"Number of Players: {numOfplayers}");
                        return MenuState.RefreshMap;

                    case 7:
                        numOfplayers = 4;
                        _buttonManager.UpdateButtonText(NumOfPlayersInfo, $"Number of Players: {numOfplayers}");
                        return MenuState.RefreshMap;

                    case 8:
                        return MenuState.RefreshMap;

                    case 9:
                        return MenuState.PlayingGame;

                    case 10:
                        return MenuState.Initial;

                    case 36:
                        return MenuState.IncreaseMapSize;

                    case 37:
                        return MenuState.DecreaseMapSize;

                    case 40:
                        _buttonManager.UpdateButtonText(AddAIInfo, "AI Player:\nTrue");
                        return MenuState.AddAI;

                    case 41:
                        _buttonManager.UpdateButtonText(AddAIInfo, "AI Player:\nFalse");
                        return MenuState.RemoveAI;
                }
            }

            return MenuState.NewGame;
        }

        public List<Player> GetPlayers()
        {
            List<Player> Players = new();

            for (int i = 0; i < numOfplayers; i++)
            {
                Player player = new(i, 0);
                Players.Add(player);
            }

            return Players;
        }

        public GameState Turn(GameTime gameTime, Player currentPlayer, int turnNumber, Button PressedButton)
        {
            if (CurrentUnit != null || CurrentTile != null)
            {
                _buttonManager.DrawButtonIDs(11, 15, 21, 26);
            }
            else
            {
                _buttonManager.DrawButtonIDs(11, 15);
            }

            CurrentPlayer = currentPlayer;

            _buttonManager.UpdateButtonText(CurrentPlayerTeamInfo, $"Team\n{currentPlayer.Colour}");
            _buttonManager.UpdateButtonText(CurrentPlayerFundsInfo, $"${currentPlayer.Funds}");
            _buttonManager.UpdateButtonText(TurnNumberInfo, $"Turn: {turnNumber}");

            if (PressedButton != null)
            {
                if (PressedButton.ID == 14)
                {
                    return GameState.EnemyTurn;
                }
                if (PressedButton.ID == 15)
                {
                    return GameState.PauseGame;
                }
            }

            return GameState.PlayerSelect;
        }

        public GameState DisplayPlayerActions(GameTime gameTime, Button PressedButton, bool displayCapture = false, bool displayResupply = false)
        {
            if (displayCapture && displayResupply)
            {
                _buttonManager.DrawButtonIDs(11, 20, 21, 26, 38, 38);
            }
            else if (displayCapture && !displayResupply)
            {
                _buttonManager.DrawButtonIDs(11, 20, 21, 26);
            }
            else if (!displayCapture && displayResupply)
            {
                _buttonManager.DrawButtonIDs(11, 19, 21, 26, 38, 38);
            }
            else
            {
                _buttonManager.DrawButtonIDs(11, 19, 21, 26);
            }

            if (PressedButton != null)
            {
                switch (PressedButton.ID)
                {
                    case 16:
                        return GameState.UnitIdle;

                    case 17:
                        DrawReachable = true;
                        return GameState.PlayerMove;

                    case 18:
                        DrawAttackable = true;
                        return GameState.PlayerAttack;

                    case 19:
                        DrawReachable = false;
                        DrawAttackable = false;
                        return GameState.PlayerSelect;

                    case 38:
                        return GameState.PlayerResupply;
                }

                if (displayCapture)
                {
                    if (PressedButton.ID == 20)
                    {
                        return GameState.PlayerCapture;
                    }
                }
            }

            DrawReachable = false;
            DrawAttackable = false;
            return GameState.SelectAction;
        }

        public void DisplayAttributes(Unit unit = null, Tile tile = null)
        {
            _buttonManager.UpdateButtonText(TypeInfo, "");
            _buttonManager.UpdateButtonText(HealthInfo, "");
            _buttonManager.UpdateButtonText(AmmoInfo, "");
            _buttonManager.UpdateButtonText(FuelInfo, "");
            _buttonManager.UpdateButtonText(DefenceInfo, "");

            CurrentUnit = unit;
            CurrentTile = tile;

            if (CurrentUnit != null || CurrentTile != null)
            {
                _buttonManager.DrawButtonIDs(11, 15, 24, 26);

                if (CurrentUnit != null)
                {
                    _buttonManager.UpdateButtonText(TypeInfo, $"{CurrentUnit.Type}");
                    _buttonManager.UpdateButtonText(HealthInfo, $"{CurrentUnit.Health}");
                    _buttonManager.UpdateButtonText(AmmoInfo, $"{CurrentUnit.Ammo}");
                    _buttonManager.UpdateButtonText(FuelInfo, $"{CurrentUnit.Fuel}");
                    _buttonManager.UpdateButtonText(DefenceInfo, $"{CurrentUnit.Defence}%");
                }
                if (CurrentTile != null)
                {
                    _buttonManager.UpdateButtonText(TypeInfo, $"{CurrentTile.Type}");
                    _buttonManager.UpdateButtonText(DefenceInfo, $"{CurrentTile.DefenceBonus}%");
                }
            }
        }

        public int ProcessUnitProduction(GameTime gameTime, Button PressedButton)
        {
            _buttonManager.DrawButtonIDs(11, 15, 19, 19, 27, 30);

            if (PressedButton != null)
            {
                switch (PressedButton.ID)
                {
                    case 19:
                        return -1;
                    case 27:
                        return 1;
                    case 28:
                        return 2;
                    case 29:
                        return 3;
                    case 30:
                        return 4;
                }
            }

            return -2;
        }

        public MenuState PausedGame(GameTime gameTime, Button PressedButton)
        {
            _buttonManager.DrawButtonIDs(31, 33, 3, 3);

            if (PressedButton != null)
            {
                switch (PressedButton.ID)
                {
                    case 3:
                        return MenuState.QuitGame;

                    case 31:
                        return MenuState.PlayingGame;

                    case 32:
                        return MenuState.SaveGame;

                    case 33:
                        return MenuState.Initial;
                }
            }

            return MenuState.GamePaused;
        }

        public MenuState GameOver(GameTime gameTime, Button PressedButton, Player Winner)
        {
            _buttonManager.UpdateButtonText(WinnerInfo, $"Winner: {Winner.Colour} Team");

            _buttonManager.DrawButtonIDs(3, 3, 33, 35);

            if (PressedButton != null)
            {
                switch (PressedButton.ID)
                {
                    case 3:
                        return MenuState.QuitGame;

                    case 33:
                        return MenuState.Initial;
                }
            }

            return MenuState.GameOver;
        }

        public void ClearAttackableOverlay()
        {
            attackableOverlay.Clear();
        }

        public void ClearMoveableOverlay()
        {
            moveableOverlay.Clear();
        }

        public int CalculateDamage(Unit attackingUnit, Unit defendingUnit)
        {
            int baseDamage = baseDamageDictionary[(attackingUnit.Type, defendingUnit.Type)];

            double defenceMultiplier = (double)(defendingUnit.Defence) / 100;

            double HealthMultiplier = (double)attackingUnit.Health / 100;

            return (int)(HealthMultiplier * (baseDamage - (baseDamage * defenceMultiplier)));
        }
    }
}
