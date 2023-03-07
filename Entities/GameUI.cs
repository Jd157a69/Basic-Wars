﻿using Basic_Wars_V2.Enums;
using Basic_Wars_V2.Graphics;
using Basic_Wars_V2.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Wars_V2.Entities
{
    public class GameUI : IGameEntity
    {
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
        private Button Menu;

        //Turn
        private Button TurnNumberInfo;
        private Button CurrentPlayerTeamInfo;
        private Button CurrentPlayerFundsInfo;
        private Button EndTurnButton;

        //DisplayActions
        private Button PlayerIdleButton;
        private Button PlayerMoveButton;
        private Button PlayerAttackButton;
        private Button ReturnButton;
        private Button CaptureButton;

        //DisplayAttributes
        private Button AttributeDisplayInfo;
        //private Button TypeInfo;
        private Button HealthInfo;
        private Button AmmoInfo;
        private Button FuelInfo;
        private Button DefenceInfo;

        //DisplayProductionChoices
        private Button UnitInfantryButton;
        private Button UnitMechButton;
        private Button UnitTankButton;
        private Button UnitAPCButton;

        private Tile SelectedUI;
        public bool DrawSelectedUI { get; set; }

        private List<Tile> reachableTiles = new List<Tile>();
        private List<Tile> moveableOverlay = new List<Tile>();
        private bool DrawReachable = false;

        private List<Tile> attackableTiles = new List<Tile>();
        private bool DrawAttackable = false;

        private List<Tile> tilesToBeRemoved = new List<Tile>();

        private Texture2D Texture;
        private SpriteFont Font;

        private ButtonManager _buttonManager;
        private UnitManager _unitManager;
        private Dijkstra _pathFinder;

        public MapManager _gameMap;

        private int numOfplayers = 2;

        private int CentreButtonX = (1920 - 672) / 2;

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

            Player placeHolder = new Player(0, 0);
            CurrentPlayer = placeHolder;

            _buttonManager = buttonManager;
            InitialiseButtons();

            DrawSelectedUI = false;

            SelectedUI = new Tile(new Vector2(0, 0), Texture);
            SelectedUI.CreateTile(0, 1);
        }

        public void InitialiseButtons()
        {
            BasicWarsTitle = new Button(Texture, Font, new Vector2(CentreButtonX, 90), "Menu", "Basic Wars");
            NewGameButton = new Button(Texture, Font, new Vector2(CentreButtonX, 360), "Menu", "New Game");
            LoadGameButton = new Button(Texture, Font, new Vector2(CentreButtonX, 540), "Menu", "Load Game");
            QuitGameButton = new Button(Texture, Font, new Vector2(CentreButtonX, 720), "Menu", "Quit");

            NumOfPlayersInfo = new Button(Texture, Font, new Vector2(CentreButtonX + 615, 0), "Menu", $"Number of Players: {numOfplayers}");
            Players2Button = new Button(Texture, Font, new Vector2(0, 180), "AltMenu", "2");
            Players3Button = new Button(Texture, Font, new Vector2(0, 304), "AltMenu", "3");
            Players4Button = new Button(Texture, Font, new Vector2(0, 428), "AltMenu", "4");
            RefreshMapButton = new Button(Texture, Font, new Vector2(0, 720), "AltMenu", "Refresh Map");
            StartGameButton = new Button(Texture, Font, new Vector2(CentreButtonX, 900), "Menu", "Start Game");
            Menu = new Button(Texture, Font, new Vector2(0, 0), "AltMenu", "Main Menu");

            EndTurnButton = new Button(Texture, Font, new Vector2(1600, 925), "AltMenu", $"End Turn");

            TurnNumberInfo = new Button(Texture, Font, new Vector2(1600, 0), "AltMenu", $"Turn: 0");
            CurrentPlayerTeamInfo = new Button(Texture, Font, new Vector2(0, 0), "AltMenu", $"Player: {CurrentPlayer.Team}");
            CurrentPlayerFundsInfo = new Button(Texture, Font, new Vector2(0, 126), "AltMenu", $"{CurrentPlayer.Funds}");

            PlayerIdleButton = new Button(Texture, Font, new Vector2(0, 300), "AltMenu", "Idle");
            PlayerMoveButton = new Button(Texture, Font, new Vector2(0, 425), "AltMenu", "Move");
            PlayerAttackButton = new Button(Texture, Font, new Vector2(0, 550), "AltMenu", "Attack");
            ReturnButton = new Button(Texture, Font, new Vector2(0, 675), "AltMenu", "Return");
            CaptureButton = new Button(Texture, Font, new Vector2(0, 125), "AltMenu", "Capture");

            AttributeDisplayInfo = new Button(Texture, Font, new Vector2(1600, 180), "Attribute");
            //TypeInfo = new Button(Texture, Font, new Vector2(1600, 180), "None");
            HealthInfo = new Button(Texture, Font, new Vector2(1625, 274.5f - 110), "None");
            AmmoInfo = new Button(Texture, Font, new Vector2(1625, 369 - 110), "None");
            FuelInfo = new Button(Texture, Font, new Vector2(1625, 463.5f - 110), "None");
            DefenceInfo = new Button(Texture, Font, new Vector2(1625, 558 - 110), "None");

            _buttonManager.AddButton(BasicWarsTitle);
            _buttonManager.AddButton(NewGameButton);
            _buttonManager.AddButton(LoadGameButton);
            _buttonManager.AddButton(QuitGameButton);

            _buttonManager.AddButton(NumOfPlayersInfo);
            _buttonManager.AddButton(Players2Button);
            _buttonManager.AddButton(Players3Button);
            _buttonManager.AddButton(Players4Button);
            _buttonManager.AddButton(RefreshMapButton);
            _buttonManager.AddButton(StartGameButton);
            _buttonManager.AddButton(Menu);

            _buttonManager.AddButton(TurnNumberInfo);
            _buttonManager.AddButton(CurrentPlayerTeamInfo);
            _buttonManager.AddButton(CurrentPlayerFundsInfo);
            _buttonManager.AddButton(EndTurnButton);

            _buttonManager.AddButton(PlayerIdleButton);
            _buttonManager.AddButton(PlayerMoveButton);
            _buttonManager.AddButton(PlayerAttackButton);
            _buttonManager.AddButton(ReturnButton);
            _buttonManager.AddButton(CaptureButton);

            _buttonManager.AddButton(AttributeDisplayInfo);
            //_buttonManager.AddButton(TypeInfo);
            _buttonManager.AddButton(HealthInfo);
            _buttonManager.AddButton(AmmoInfo);
            _buttonManager.AddButton(FuelInfo);
            _buttonManager.AddButton(DefenceInfo);
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

            return reachableTiles;
        }

        public List<Tile> GetAttackableTiles(Unit attackingUnit, Tile StartingTile)
        {
            attackableTiles.Clear();

            List<Tile> adjacentTiles = _gameMap.GetNeighbours(StartingTile);

            foreach (Tile tile in adjacentTiles)
            {
                foreach (Unit defendingUnit in _unitManager.units)
                {
                    if (
                        defendingUnit.Position == tile.Position
                        && defendingUnit.Team != attackingUnit.Team
                        && attackingUnit.Type != UnitType.APC
                       )
                    {
                        Tile attackingTile = new Tile(tile.Position, Texture);
                        attackingTile.CreateTile(1, 1);
                        attackableTiles.Add(attackingTile);
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

        public void Draw(SpriteBatch _spriteBatch, GameTime gameTime, float Scale = 1.0f)
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
                foreach (Tile tile in attackableTiles)
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
                        return MenuState.NewGame;

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
            _buttonManager.DrawButtonIDs(4, 10);

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
                }
            }

            return MenuState.NewGame;
        }

        public List<Player> GetPlayers()
        {
            List<Player> Players = new List<Player>();

            for (int i = 0; i < numOfplayers; i++)
            {
                Player player = new Player(i, 1000);
                Players.Add(player);
            }

            return Players;
        }

        public GameState Turn(GameTime gameTime, Player currentPlayer, int turnNumber, Button PressedButton)
        {
            if (CurrentUnit != null || CurrentTile != null)
            {
                _buttonManager.DrawButtonIDs(11, 14, 20, 24);
            }
            else
            {
                _buttonManager.DrawButtonIDs(11, 14);
            }

            CurrentPlayer = currentPlayer;

            _buttonManager.UpdateButtonText(CurrentPlayerTeamInfo, $"Team: {currentPlayer.Team + 1}");
            _buttonManager.UpdateButtonText(CurrentPlayerFundsInfo, $"Funds: {currentPlayer.Funds}");
            _buttonManager.UpdateButtonText(TurnNumberInfo, $"Turn: {turnNumber}");

            if (PressedButton != null)
            {
                if (PressedButton.ID == 14)
                {
                    return GameState.EnemyTurn;
                }
            }

            return GameState.PlayerSelect;
        }

        public GameState DisplayPlayerActions(GameTime gameTime, Button PressedButton)
        {
            _buttonManager.DrawButtonIDs(11, 18, 20, 24);       //Need to be able to display both actions and attributes but how?

            if (PressedButton != null)
            {
                switch (PressedButton.ID)
                {
                    case 15:
                        return GameState.SelectAction;

                    case 16:
                        DrawReachable = true;
                        return GameState.PlayerMove;

                    case 17:
                        DrawAttackable = true;
                        return GameState.PlayerAttack;

                    case 18:
                        DrawReachable = false;
                        DrawAttackable = false;
                        return GameState.PlayerSelect;
                }
            }

            DrawReachable = false;
            DrawAttackable = false;
            return GameState.SelectAction;
        }

        public void DisplayAttributes(Unit unit = null, Tile tile = null)
        {
            _buttonManager.UpdateButtonText(HealthInfo, "");
            _buttonManager.UpdateButtonText(AmmoInfo, "");
            _buttonManager.UpdateButtonText(FuelInfo, "");
            _buttonManager.UpdateButtonText(DefenceInfo, "");

            CurrentUnit = unit;
            CurrentTile = tile;

            if (CurrentUnit != null)
            {
                _buttonManager.DrawButtonIDs(11, 14, 20, 24);
                _buttonManager.UpdateButtonText(HealthInfo, $"{CurrentUnit.Health}");
                _buttonManager.UpdateButtonText(AmmoInfo, $"{CurrentUnit.Ammo}");
                _buttonManager.UpdateButtonText(FuelInfo, $"{CurrentUnit.Fuel}");
                _buttonManager.UpdateButtonText(DefenceInfo, $"{CurrentUnit.Defence}%");
            }
            if (CurrentTile != null)
            {
                _buttonManager.DrawButtonIDs(11, 14, 23, 24);
                _buttonManager.UpdateButtonText(DefenceInfo, $"{CurrentTile.DefenceBonus}%");
            }
        }

        public int ProcessUnitProduction(GameTime gameTime, Button PressedButton)
        {

            switch (PressedButton.ID) 
            {
                case 0:
                    break;
            }

            return -1;
        }
    }
}
 