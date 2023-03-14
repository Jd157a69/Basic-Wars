using Basic_Wars_V2.Entities;
using Basic_Wars_V2.Enums;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Basic_Wars_V2.System
{
    public class AI : Player
    {
        private readonly Random random = new();

        private Texture2D Texture { get; set; }

        private MapManager _gameMap { get; set; }
        private UnitManager _unitManager { get; set; }
        private GameUI _gameUI { get; set; }
        private InputController _inputController { get; set; }

        private Tile HQ { get; set; }

        private readonly List<Tile> FriendlyBuildings = new();
        private readonly List<Tile> EnemyHQs = new();
        private readonly List<Unit> AIUnits = new();

        private List<Tile> reachableTiles = new();
        private readonly List<Tile> closeByStructures = new();

        private AIState State { get; set; }

        public AI(int team, int initialFunds, MapManager map, UnitManager unitManager, GameUI gameUI, InputController inputController, Texture2D texture) : base(team, initialFunds)
        {
            State = AIState.Initial;

            _gameMap = map;
            _unitManager = unitManager;
            _gameUI = gameUI;
            _inputController = inputController;

            Texture = texture;
        }

        public void RefreshAI(MapManager map, UnitManager unitManager)
        {
            _gameMap = map;
            _unitManager = unitManager;
        }

        public void RunAILogic()
        {
            GetEnemyHQs();
            GetAIUnits();

            Console.WriteLine($"\nFunds Before Turn: {Funds}");
            switch (State)
            {
                case AIState.Initial:
                    ProduceUnits();
                    break;

                case AIState.Attack:

                    foreach (Unit unit in AIUnits)
                    {
                        if (unit.State != UnitState.Moved
                            && unit.State != UnitState.Used
                            && unit.Fuel > 0
                           )
                        {
                            Tile enemyHQ = EnemyHQs[0];
                            if (unit.Type != UnitType.Tank && unit.Type != UnitType.APC)
                            {
                                MoveTowardsTile(unit, enemyHQ);
                            }
                            else
                            {
                                MoveToRandomReachable(unit);
                            }
                        }
                    }

                    break;

                case AIState.Defend:

                    foreach (Unit unit in AIUnits)
                    {
                        if (unit.State != UnitState.Moved
                            && unit.State != UnitState.Used
                            && unit.Fuel > 0
                           )
                        {
                            MoveTowardsTile(unit, HQ);
                        }
                    }

                    break;

                case AIState.CaptureStructures:

                    foreach (Unit unit in AIUnits)
                    {
                        reachableTiles = GetReachableTiles(unit);

                        foreach (Tile structure in closeByStructures)
                        {
                            if (reachableTiles.Contains(structure)
                                && structure.Team != Team
                                && unit.State != UnitState.Moved
                                && unit.State != UnitState.Used
                                && unit.Type != UnitType.Tank
                                && unit.Type != UnitType.APC
                                && unit.Fuel > 0
                               )
                            {
                                MoveTowardsTile(unit, structure);
                            }
                            else if (unit.State != UnitState.Moved
                                    && unit.State != UnitState.Used
                                    && unit.Fuel > 0
                                   )
                            {
                                MoveToRandomReachable(unit, true);
                            }
                        }
                    }

                    break;
            }

            AttackNeighbouringUnits();
            CaptureStructures();
            ProduceUnits();

            //Setting State
            if (EnemyCloseToHQ())   //Prioritises its own HQ
            {
                State = AIState.Defend;
            }
            if (AIUnits.Count >= 8) //Attacks when it has a surplus of Units
            {
                State = AIState.Attack;
            }
            else if (CloseByStructure()) //Moves towards structures if in range of its units
            {
                State = AIState.CaptureStructures;
            }

            Console.WriteLine($"Funds After Turn: {Funds}");
        }

        private void CaptureStructures()
        {
            foreach (Unit unit in AIUnits)
            {
                Tile unitTile = _inputController.GetUnitTile(unit);
                if ((unitTile.Type == TileType.City
                    || unitTile.Type == TileType.Factory
                    || unitTile.Type == TileType.HQ)
                    && unit.Type != UnitType.Tank
                    && unit.Type != UnitType.APC
                    && unitTile.Team != Team
                    && unit.State != UnitState.Used
                   )
                {
                    unitTile.Team = Team;
                    unitTile.CreateTileSpriteOnType();
                    unit.State = UnitState.Used;
                }
            }
        }

        private void MoveToRandomReachable(Unit unit, bool AwayFromHQ = false, int attempts = 5)
        {
            reachableTiles = GetReachableTiles(unit);
            int randomTile;
            if (AwayFromHQ)
            {
                Tile previousTile;
                Tile newTile = null;

                Tile destination = null;
                for (int i = 0; i < attempts; i++)
                {
                    randomTile = random.Next(reachableTiles.Count);
                    if (newTile == null)
                    {
                        previousTile = reachableTiles[randomTile];
                    }
                    else
                    {
                        previousTile = newTile;
                    }
                    newTile = reachableTiles[randomTile];

                    if (GetSquaredDistance(newTile, HQ) >= GetSquaredDistance(previousTile, HQ))
                    {
                        destination = newTile;
                    }
                }
                if (newTile != null)
                {
                    unit.Fuel--;
                    unit.State = UnitState.Moved;
                    unit.Position = destination.Position;
                }
            }
            else
            {
                unit.Fuel--;
                unit.State = UnitState.Moved;
                randomTile = random.Next(reachableTiles.Count);
                unit.Position = reachableTiles[randomTile].Position;
            }
        }

        private List<Tile> GetReachableTiles(Unit unit)
        {
            return _gameUI.GetReachableTiles(unit, _unitManager.GetUnitPositions(), _inputController.GetUnitTile(unit));
        }

        private bool EnemyCloseToHQ()
        {
            foreach (Unit unit in _unitManager.units)
            {
                reachableTiles = GetReachableTiles(unit);

                if (unit.Team != Team && reachableTiles.Contains(HQ))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CloseByStructure()
        {
            closeByStructures.Clear();
            foreach (Unit unit in AIUnits)
            {
                reachableTiles = GetReachableTiles(unit);

                foreach (Tile tile in reachableTiles)
                {
                    if ((tile.Type == TileType.City
                        || tile.Type == TileType.Factory)
                        && tile.Team != Team)
                    {
                        closeByStructures.Add(tile);
                    }
                }
            }

            if (closeByStructures.Count > 0)
            {
                return true;
            }

            return false;
        }

        private void AttackNeighbouringUnits()
        {
            foreach (Unit unit in AIUnits)
            {
                Tile startingTile = _inputController.GetUnitTile(unit);
                List<Tile> adjacentTiles = _gameMap.GetNeighbours(startingTile);

                foreach (Tile neighbour in adjacentTiles)
                {
                    Unit neighbouringUnit = _inputController.GetTileUnit(neighbour);
                    if (neighbouringUnit != null
                        && neighbouringUnit.Team != Team
                        && unit.Ammo > 0
                       )
                    {
                        unit.Health -= _gameUI.CalculateDamage(unit, neighbouringUnit);
                        unit.Ammo--;
                        if (neighbouringUnit.Health > 0 && neighbouringUnit.Ammo > 0)
                        {
                            neighbouringUnit.Health -= _gameUI.CalculateDamage(neighbouringUnit, unit);
                            neighbouringUnit.Ammo--;
                        }
                        unit.State = UnitState.Used;
                    }
                }
            }
        }

        private void MoveTowardsTile(Unit movingUnit, Tile destination)
        {
            reachableTiles.Clear();
            reachableTiles = _gameUI.GetReachableTiles(movingUnit, _unitManager.GetUnitPositions(), _inputController.GetUnitTile(movingUnit));

            if (movingUnit.Position != destination.Position)
            {
                Unit destinationUnit = _inputController.GetTileUnit(destination);
                if (destination.Team != Team || destinationUnit != null && destinationUnit.Team != Team)//HERE
                {
                    Tile closestTile = null;
                    float dist = int.MaxValue;
                    foreach (Tile tile in reachableTiles)
                    {
                        if (GetSquaredDistance(tile, destination) < dist)
                        {
                            closestTile = tile;
                            dist = GetSquaredDistance(tile, destination);
                        }
                    }

                    if (closestTile != null)
                    {
                        movingUnit.Position = closestTile.Position;
                        movingUnit.State = UnitState.Moved;
                        movingUnit.Fuel--;
                    }
                }
            }
        }

        private static float GetSquaredDistance(Tile startingTile, Tile destinationTile)
        {
            float x = startingTile.Position.X - destinationTile.Position.X;
            float y = startingTile.Position.Y - destinationTile.Position.Y;

            return x * x + y * y;
        }

        private void GetEnemyHQs()
        {
            EnemyHQs.Clear();
            foreach (Tile structure in _gameMap.HQs)
            {
                if (structure.Team != Team)
                {
                    EnemyHQs.Add(structure);
                }
                else
                {
                    HQ = structure;
                }
            }
        }

        private void GetFriendlyBuildings()
        {
            FriendlyBuildings.Clear();
            foreach (Tile structure in _gameMap.Structures)
            {
                if (structure.Team == Team)
                {
                    FriendlyBuildings.Add(structure);
                }
            }
        }

        private void GetAIUnits()
        {
            AIUnits.Clear();
            foreach (Unit unit in _unitManager.units)
            {
                if (unit.Team == Team)
                {
                    AIUnits.Add(unit);
                }
            }
        }

        private void ProduceUnits()
        {
            GetFriendlyBuildings();

            foreach (Unit unit in AIUnits)
            {
                if (unit.State != UnitState.Moved
                    && unit.State != UnitState.Used
                    && unit.Fuel > 0
                   )
                {
                    MoveToRandomReachable(unit, true);
                }
            }

            foreach (Tile structure in FriendlyBuildings)
            {
                if ((structure.Type == TileType.Factory
                    || structure.Type == TileType.HQ)
                    && _inputController.GetTileUnit(structure) == null
                   )
                {
                    int unitType = -1;
                    if (Funds >= 7000)
                    {
                        Funds -= 7000;
                        unitType = 3;

                    }
                    else if (Funds >= 5000)
                    {
                        continue;
                    }
                    else if (Funds >= 4000)
                    {
                        Funds -= 3000;
                        unitType = 2;
                    }
                    else
                    {
                        Funds -= 1000;
                        unitType = 1;
                    }

                    if (unitType != -1)
                    {
                        Unit newUnit = new(Texture, structure.Position, unitType, Team)
                        {
                            State = UnitState.Used
                        };
                        _unitManager.AddUnit(newUnit);
                    }
                }
            }
        }
    }
}
