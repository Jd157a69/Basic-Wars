using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Wars_V2.System
{
    public class PoissonDiscSampling
    {
        public List<Vector2> GetPoints (double radius, Vector2 mapSize, int k = 30)
        {
            Random random = new Random();

            double cellSize = radius / Math.Sqrt(2);

            int Width = (int)(mapSize.X / cellSize) + 1;
            int Height = (int)(mapSize.Y / cellSize) + 1;

            int[,] grid = new int[Width, Height];

            List<Vector2> points = new List<Vector2>();
            List<Vector2> startPoints = new List<Vector2>();

            Vector2 initialPoint = new Vector2(random.Next((int)mapSize.X), random.Next((int)mapSize.Y));

            startPoints.Add(initialPoint);

            while (startPoints.Count > 0)
            {
                int randomIndex = random.Next(startPoints.Count);
                Vector2 startPoint = startPoints[randomIndex];

                bool potentialPointFound = false;
                for (int tries = 0; tries < k; tries++)
                {
                    double angle = random.NextDouble() * 2 * Math.PI;
                    Vector2 direction = new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle));
                    Vector2 candidatePoint = startPoint + direction * random.Next((int)radius, (int)radius * 2);

                    if (isValid(candidatePoint, mapSize, cellSize, points, grid, radius))
                    {
                        points.Add(candidatePoint);
                        startPoints.Add(candidatePoint);
                        grid[(int)(candidatePoint.X / cellSize), (int)(candidatePoint.Y / cellSize)] = points.Count;
                        potentialPointFound = true;
                        break;
                    }
                }

                if (!potentialPointFound)
                {
                    startPoints.RemoveAt(randomIndex);
                    break;
                }

            }

            return points;
        }

        private bool isValid(Vector2 potentialPoint, Vector2 mapSize, double cellSize, List<Vector2> points, int[,] grid, double radius)
        {
            if (potentialPoint.X >= 0 && potentialPoint.X < mapSize.X && potentialPoint.Y >= 0 && potentialPoint.Y < mapSize.Y) 
            {
                Vector2 cell = new Vector2((float)(potentialPoint.X / cellSize), (float)(potentialPoint.Y / cellSize));
                int startX = Math.Max(0, (int)cell.X - 2);
                int endX = Math.Min((int)cell.X + 2, grid.GetLength(0) - 1);
                int startY = Math.Max(0, (int)cell.Y - 2);
                int endY = Math.Min((int)cell.Y + 2, grid.GetLength(1) - 1);

                for (int x = startX; x <= endX; x++)
                {
                    for (int y = startY; y <= endY; y++)
                    {
                        int pointIndex = grid[x, y] - 1;
                        if (pointIndex != -1)
                        {
                            double squrdDistance = (SquaredDistance(potentialPoint, points[pointIndex]));
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

        private double SquaredDistance(Vector2 start, Vector2 end)
        {
            double x = start.X - end.X;
            double y = start.Y - end.Y;
            return x * x + y * y;
        }
    }
}
