using Basic_Wars_V2.Entities;
using System.Collections.Generic;

namespace Basic_Wars_V2.System
{
    public class Dijkstra
    {
        private readonly MapManager mapManager;

        private readonly Tile[,] gameMap;

        public Dijkstra(MapManager Map)
        {
            mapManager = Map;

            gameMap = mapManager.Map;
        }

        public List<Tile> FindReachableTiles(Tile startingTile, Unit unit)
        {
            List<Tile> reachableTiles = new List<Tile>();
            PriorityQueue<Tile, int> queue = new PriorityQueue<Tile, int>();

            startingTile.TotalCost = 0;
            queue.Enqueue(startingTile, 0);

            while (queue.Count > 0)
            {
                Tile currentTile = queue.Dequeue();
                reachableTiles.Add(currentTile);

                foreach (Tile neighbor in mapManager.GetNeighbours(currentTile))
                {
                    int cost = currentTile.TotalCost + mapManager.GetCost(currentTile, unit);

                    if (!reachableTiles.Contains(neighbor) && cost <= unit.MovementPoints)
                    {
                        neighbor.TotalCost = cost;

                        queue.Enqueue(neighbor, cost);
                    }
                }
            }

            return reachableTiles;
        }
    }

}
