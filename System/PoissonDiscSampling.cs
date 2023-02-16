using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Wars_V2.System
{
    public static class PoissonDiscSampling
    {
        public static List<Vector2> GetPoints (double radius, Vector2 regionsSize, int k = 30)
        {
            Random random = new Random();
            int numOfDimensions = 2;

            double cellSize = radius / Math.Sqrt(numOfDimensions);

            int nCellsWidth = (int)(regionsSize.X / cellSize) + 1;
            int nCellsHeight = (int)(regionsSize.Y / cellSize) + 1;

            int[,] grid = new int[nCellsWidth, nCellsHeight];

            List<Vector2> points = new List<Vector2>();
            List<Vector2> spawnPoints = new List<Vector2>();

            Vector2 initialPoint = new Vector2(random.Next((int)regionsSize.X), random.Next((int)regionsSize.Y));

            spawnPoints.Add(initialPoint);

            while (spawnPoints.Count > 0)
            {
                int spawnIndex = random.Next(spawnPoints.Count);
                Vector2 spawnPoint = spawnPoints[spawnIndex];

                bool candidateFound = false;
                for (int tries = 0; tries < k; tries++)
                {
                    double angle = random.NextDouble() * 2 * Math.PI;
                    Vector2 direction = new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle));
                    Vector2 candidatePoint = spawnPoint + direction * random.Next((int)radius, (int)radius * 2);

                    if (isValid(candidatePoint, regionsSize, cellSize, points, grid, radius))
                    {
                        points.Add(candidatePoint);
                        spawnPoints.Add(candidatePoint);
                        grid[(int)(candidatePoint.X / cellSize), (int)(candidatePoint.Y / cellSize)] = points.Count;
                        candidateFound = true;
                        break;
                    }
                }

                if (!candidateFound)
                {
                    spawnPoints.RemoveAt(spawnIndex);
                    break;
                }

            }

            return points;
        }

        static bool isValid(Vector2 candidatePoint, Vector2 regionSize, double cellSize, List<Vector2> points, int[,] grid, double radius)
        {
            if (candidatePoint.X >= 0 && candidatePoint.X < regionSize.X && candidatePoint.Y >= 0 && candidatePoint.Y < regionSize.Y) 
            {
                int cellX = (int)(candidatePoint.X / cellSize);
                int cellY = (int)(candidatePoint.Y / cellSize);
                int searchStartX = Math.Max(0, cellX - 2);
                int searchEndX = Math.Min(cellX + 2, grid.GetLength(0) - 1);
                int searchStartY = Math.Max(0, cellY - 2);
                int searchEndY = Math.Min(cellY + 2, grid.GetLength(1) - 1);

                for (int x = searchStartX; x <= searchEndX; x++)
                {
                    for (int y = searchStartY; y <= searchEndY; y++)
                    {
                        int pointIndex = grid[x, y] - 1;
                        if (pointIndex != -1)
                        {
                            double squrdDistance = (Vector2.DistanceSquared(candidatePoint, points[pointIndex]));
                            if (squrdDistance < radius * radius)
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }
    }
}
