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

                foreach (Tile neighbor in mapManager.GetNeighbors(currentTile))
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
    }

}
