using Basic_Wars_V2.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Wars_V2.System
{
    public class Dijkstra
    {
        private MapManager mapManager;

        private Tile[,] gameMap;

        public Dijkstra(MapManager Map)
        {
            mapManager = Map;

            gameMap = mapManager.map;
        }

        public List<Tile> FindReachableTiles(Tile startingTile, Unit unit)
        {
            List<Tile> reachableTiles = new List<Tile>();
            PriorityQueue<Tile, int> openSet = new PriorityQueue<Tile, int>();

            startingTile.TotalCost = 0;
            openSet.Enqueue(startingTile, 0);

            while (openSet.Count > 0)
            {
                Tile currentTile = openSet.Dequeue();
                reachableTiles.Add(currentTile);

                foreach (Tile neighbor in GetNeighbors(currentTile))
                {
                    int cost = currentTile.TotalCost + mapManager.GetCost(currentTile, unit);

                    if (!reachableTiles.Contains(neighbor) && cost <= unit.MovementPoints)
                    {
                        neighbor.TotalCost = cost;
                        neighbor.Parent = currentTile;

                        openSet.Enqueue(neighbor, cost);
                    }
                }
            }

            return reachableTiles;
        }

        private List<Tile> GetNeighbors(Tile tile)
        {
            List<Tile> neighbors = new List<Tile>();

            int X = (int)tile.MapGridPos.X;
            int Y = (int)tile.MapGridPos.Y;

            if (X > 0)
            {
                neighbors.Add(gameMap[X - 1, Y]);
            }
            if (X < gameMap.GetLength(0) - 1)
            {
                neighbors.Add(gameMap[X + 1, Y]);
            }
                
            if (Y > 0)
            {
                neighbors.Add(gameMap[X, Y - 1]);
            }
            if (Y < gameMap.GetLength(1) - 1)
            {
                neighbors.Add(gameMap[X, Y + 1]);
            }
                
            return neighbors;
        }
    }

}
