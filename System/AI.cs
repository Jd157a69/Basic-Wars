using Basic_Wars_V2.Entities;
using Basic_Wars_V2.Enums;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Wars_V2.System
{
    public class AI : Player
    {
        private Texture2D Texture { get; set; }

        private MapManager _gameMap { get; set; }
        private UnitManager _unitManager { get; set; }
        private GameUI _gameUI { get; set; }
        private InputController _inputController { get; set; }

        private Tile HQ { get; set; }

        private List<Tile> FriendlyBuildings = new List<Tile>();
        private List<Tile> EnemyHQs = new List<Tile>();
        private List<Unit> AIUnits = new List<Unit>();
             
        private List<Tile> reachableTiles = new List<Tile>();

        private AIState State { get; set; }

        public AI(int team, int initialFunds, MapManager map, UnitManager unitManager, GameUI gameUI, InputController inputController, Texture2D texture) : base(team, initialFunds) 
        {
            State = AIState.ProduceUnit;

            _gameMap = map;
            _unitManager = unitManager;
            _gameUI = gameUI;
            _inputController = inputController;

            Texture = texture;

            GetEnemyHQs();
        }

        public void RunAILogic()
        {
            GetAIUnits();

            switch (State)
            {
                case AIState.Attack:

                    //Move towards enemy HQs
                    foreach (Unit unit in AIUnits)
                    {
                        if (unit.State != UnitState.Moved)
                        {
                            foreach (Tile HQ in EnemyHQs)
                            {
                                unit.State = UnitState.Moved;
                                unit.Fuel--;
                                MoveTowardsTile(unit, HQ);

                                Console.WriteLine($"AI moved: {unit.Type} towards player {HQ.Team} HQ");
                            }
                        }
                    }

                    //Attack neighbouring Units
                    foreach (Unit unit in AIUnits)
                    {
                        Tile startingTile = _inputController.GetUnitTile(unit);
                        List<Tile> adjacentTiles = _gameMap.GetNeighbours(startingTile);

                        foreach (Tile neighbour in adjacentTiles)
                        {
                            Unit neighbouringUnit = _inputController.GetTileUnit(neighbour);
                            if (neighbouringUnit != null
                                && neighbouringUnit.Team != Team
                               )
                            {
                                unit.Health -= _gameUI.CalculateDamage(unit, neighbouringUnit);
                                if (neighbouringUnit.Health > 0)
                                {
                                    neighbouringUnit.Health -= _gameUI.CalculateDamage(neighbouringUnit, unit);
                                }
                                unit.State = UnitState.Used;

                                Console.WriteLine($"AI attacked {neighbouringUnit.Type} on player {neighbouringUnit.Team} with {unit.Type}");
                            }
                        }
                    }

                    //Capture Structures
                    foreach (Unit unit in AIUnits)
                    {
                        Tile unitTile = _inputController.GetUnitTile(unit);
                        if ((unitTile.Type == TileType.City
                            || unitTile.Type == TileType.Factory
                            || unitTile.Type == TileType.HQ)
                            && unitTile.Team != Team
                           )
                        {
                            unitTile.Team = Team;
                            unitTile.CreateTileSpriteOnType();
                        }
                    }

                    break;

                case AIState.Defend:

                    foreach (Unit unit in AIUnits)
                    {
                        if (unit.State != UnitState.Moved)
                        {
                            unit.State = UnitState.Moved;
                            unit.Fuel--;
                            MoveTowardsTile(unit, HQ);
                        }
                    }

                    break;

                case AIState.ProduceUnit:

                    GetFriendlyBuildings();

                    foreach (Tile structure in FriendlyBuildings)
                    {
                        if (structure.Type == TileType.Factory 
                            || structure.Type == TileType.HQ
                            && _inputController.GetTileUnit(structure) == null
                           )
                        {
                            int unitType = -1;
                            if (Funds >= 7000)
                            {
                                Funds -= 7000;
                                unitType = 2;
                                
                            }
                            else if (Funds >= 4000)
                            {
                                Funds -= 3000;
                                unitType = 1;
                            }
                            else
                            {
                                unitType = 0;
                            }

                            if (unitType != -1)
                            {
                                Unit newUnit = new Unit(Texture, structure.Position, unitType, Team);
                                _unitManager.AddUnit(newUnit);
                            }
                        }
                    }

                    break;
            }

            //Setting State
            //if ()
            //{
            //    State = AIState.Attack;
            //}
            //else if () An enemy unit is too close to the HQ or a captured building
            //{
            //    State = AIState.Defend;
            //}
            //else if (AIUnits.Count < 5)
            //{
            //    State = AIState.ProduceUnit;
            //}
        }

        private void MoveTowardsTile (Unit movingUnit, Tile destination)
        {
            reachableTiles.Clear();
            reachableTiles = _gameUI.GetReachableTiles(movingUnit, _unitManager.GetUnitPositions(), _inputController.GetUnitTile(movingUnit));

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
            }
        }

        private float GetSquaredDistance(Tile startingTile, Tile destinationTile)
        {
            float x = startingTile.Position.X - destinationTile.Position.X;
            float y = startingTile.Position.Y - destinationTile.Position.Y;

            return x * x + y * y;
        }

        private void GetEnemyHQs()
        {
            EnemyHQs.Clear();
            foreach (Tile structure in _gameMap.structures)
            {
                if (structure.Team != Team
                    && structure.Type == TileType.HQ
                   )
                {
                    EnemyHQs.Add(structure);
                }
                else if (structure.Type == TileType.HQ)
                {
                    HQ = structure; 
                }
            }
        }

        private void GetFriendlyBuildings()
        {
            FriendlyBuildings.Clear();
            foreach (Tile structure in _gameMap.structures)
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
    }
}
