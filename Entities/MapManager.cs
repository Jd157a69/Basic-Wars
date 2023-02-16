using Basic_Wars_V2.Enums;
using Basic_Wars_V2.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Wars_V2.Entities
{
    public class MapManager
    {
        public Tile[,] map;

        public List<Tile> tempTiles = new List<Tile>();

        public int MapWidth { get; set; }
        public int MapHeight { get; set; }

        private const int TILE_DIMENSIONS = 56;
        public Vector2 MapSize { get; set; }

        public Vector2 Position { get; set; }
        public Texture2D Texture { get; set; }

        public List<Vector2> points = new List<Vector2>();

        public Rectangle MapCollider { get; set; }

        private int StructureSparsity { get; set; }

        public MapManager(Texture2D texture, Vector2 position, int mapWidth, int mapHeight)
        {
            MapWidth = mapWidth;
            MapHeight = mapHeight;
            Position = position;
            Texture = texture;

            MapSize = new Vector2(mapWidth * TILE_DIMENSIONS, mapHeight * TILE_DIMENSIONS);

            map = new Tile[MapWidth, MapHeight];
            StructureSparsity = (MapWidth / 5) * TILE_DIMENSIONS;

            GenerateMap();
            GenerateMapCollider();
        }

        public void GenerateMap()
        {
            GenerateBaseMap();
            AddStructures("City");
            AddRoads();
            AddStructures("Factory");
            AddRoads();
        }

        private void GenerateBaseMap()
        {
            float x = Position.X;
            float y = Position.Y;
            int randomTile;
            Vector2 tempPosition = new Vector2(x, y);

            for (int i = 0; i < MapWidth; i++)
            {
                x = Position.X;
                for (int j = 0; j < MapHeight; j++)
                {
                    randomTile = RandomTile();
                    tempPosition = new Vector2(x, y);
                    map[j, i] = new Tile(tempPosition, Texture);
                    map[j, i].CreateTile(randomTile);
                    switch (randomTile)
                    {
                        case 0:
                            map[j, i].Type = TileType.Plains;
                            break;
                        case 1:
                            map[j, i].Type = TileType.Forest;
                            break;
                        case 2:
                            map[j, i].Type = TileType.Mountain;
                            break;
                    }
                    x += 56;
                }
                y += 56;
            }
        }

        private void AddStructures(string StructureType)
        {
            int StructureColumnShift = 0;
            int StructureRowShift = 0;

            TileType Type = TileType.None;

            List<Vector2> points = new List<Vector2>();

            if (StructureType == "City")
            {
                StructureColumnShift = -6;
                Type = TileType.City;
            }
            else if (StructureType == "Factory")
            {
                Type = TileType.Factory;
                StructureSparsity *= 2;
                StructureColumnShift = -6;
                StructureRowShift= 1;
            }

            points = PoissonDiscSampling.GetPoints(StructureSparsity, MapSize);

            //Debug
            Console.WriteLine($"{StructureType} Grid Positions:");

            foreach (Vector2 point in points)
            {
                int newGridX = (int)(point.X) / TILE_DIMENSIONS;
                int newGridY = (int)(point.Y) / TILE_DIMENSIONS;

                Vector2 newGridPos = new Vector2(map[newGridX, newGridY].Position.X, map[newGridX, newGridY].Position.Y);

                Structure newStructure = new Structure(newGridPos, Texture);

                if (!(map[newGridX, newGridY].Type == TileType.City))
                {
                    map[newGridX, newGridY] = newStructure;
                    map[newGridX, newGridY].Type = Type;
                    map[newGridX, newGridY].CreateTile(0, StructureColumnShift, StructureRowShift);
                }

                //Debug
                Console.WriteLine($"[{newGridX}, {newGridY}]");
            }
        }

        private void AddRoads()
        {
            int xStart = 0;
            int yStart = 0;

            Vector2 firstStructureGridPos = new Vector2(0, 0);
            bool firstStructureFound = false;

            Vector2 nextStructureGridPos = new Vector2(0, 0);

            //Vertical Search
            for (int x = xStart; x < MapWidth - 1; x++)
            {
                yStart = 0;
                for (int y = 0; y < MapHeight - 1; y += 2)
                {
                    if ((map[x, y].Type == TileType.City || map[x,y].Type == TileType.Factory) && !firstStructureFound)
                    {
                        firstStructureGridPos = new Vector2(x, y);
                        firstStructureFound = true;
                        //Debug
                        Console.WriteLine("_______________________________________________________________________");
                        Console.WriteLine($"\nFirst City Grid Pos: {firstStructureGridPos.X}, {firstStructureGridPos.Y}");

                    }
                    else if (map[x, y].Type == TileType.City || map[x, y].Type == TileType.Factory)
                    {
                        nextStructureGridPos = new Vector2(x, y);
                        //Debug
                        Console.WriteLine($"Next City Grid Pos: {nextStructureGridPos.X}, {nextStructureGridPos.Y}");

                        xStart = x;
                        yStart = y;

                        BuildRoad(firstStructureGridPos, nextStructureGridPos);
                        firstStructureFound = false;
                    }
                }
            }

            //Horizontal Search
            for (int y = yStart; y < MapHeight - 1; y += 2)
            {
                yStart = 0;
                for (int x = 0; x < MapWidth - 1; x++)
                {
                    if ((map[x, y].Type == TileType.City || map[x, y].Type == TileType.Factory) && !firstStructureFound)
                    {
                        firstStructureGridPos = new Vector2(x, y);
                        firstStructureFound = true;
                        //Debug
                        Console.WriteLine("_______________________________________________________________________");
                        Console.WriteLine($"\nFirst City Grid Pos: {firstStructureGridPos.X}, {firstStructureGridPos.Y}");

                    }
                    else if (map[x, y].Type == TileType.City || map[x, y].Type == TileType.Factory)
                    {
                        nextStructureGridPos = new Vector2(x, y);
                        //Debug
                        Console.WriteLine($"Next City Grid Pos: {nextStructureGridPos.X}, {nextStructureGridPos.Y}");

                        xStart = x;
                        yStart = y;

                        BuildRoad(firstStructureGridPos, nextStructureGridPos);
                        firstStructureFound = false;
                    }
                }
            }
        }

        private void BuildRoad(Vector2 firstCityGridPos, Vector2 nextCityGridPos)
        {
            int x0 = (int)firstCityGridPos.X;
            int y0 = (int)firstCityGridPos.Y;
            int x1 = (int)nextCityGridPos.X;
            int y1 = (int)nextCityGridPos.Y;

            //Debug
            Console.WriteLine($"\nInitial x0 -> x1: {x0} -> {x1}");
            Console.WriteLine($"Initial y0 -> y1: {y0} -> {y1}\n");


            Console.WriteLine("Adjusting X:");

            while (x0 != x1)
            {
                if (x0 > x1)
                {
                    x0--;
                    CreateRoadTile(x0, y0, 3);
                }
                else if (x0 < x1)
                {
                    x0++;
                    CreateRoadTile(x0, y0, 3);
                }
            }

            //Debug
            Console.WriteLine("Adjusting Y:");
            while (y0 != y1)
            {
                if (y0 > y1)
                {
                    y0--;
                    CreateRoadTile(x0, y0, 5);
                }
                else if (y0 < y1)
                {
                    y0++;
                    CreateRoadTile(x0, y0, 5);
                }
            }

            
        }

        private void CreateRoadTile(int x, int y, int direction)
        {
            Tile roadTile = new Tile(map[x, y].Position, Texture);

            if (map[x, y].Type != TileType.City && map[x, y].Type != TileType.Factory && map[x, y].Type != TileType.Mountain)
            {
                map[x, y] = roadTile;
                map[x, y].Type = TileType.Road;
                map[x, y].CreateTile(0, direction);
                //Debug
                Console.WriteLine($"Tile Created at: {x}, {y}");
            }
            else
            {
                //Debug
                Console.WriteLine($"Tile not created due to obstruction at {x}, {y}");
            }
        }

        private void GenerateMapCollider()
        {
            MapCollider = new Rectangle((int)Position.X, (int)Position.Y, MapWidth*56, MapHeight*56);
        }

        public Tile GetSelectedTile()
        {
            foreach (Tile tile in map)
            {
                if (tile.State == TileState.Selected)
                {
                    return tile;
                }
            }
            return null;
        }

        public void DrawMap(SpriteBatch spriteBatch, GameTime gameTime)
        {
            foreach (Tile tile in map)
            {
                tile.Draw(spriteBatch, gameTime);
            }
        }

        public void UpdateMap(GameTime gameTime)
        {
            foreach (Tile tile in map)
            {
                tile.Update(gameTime);
            }
        }

        private int RandomTile()
        {
            Random random = new Random();

            int randTileNum = random.Next(0, 20);


            switch (randTileNum)
            {
                case 0:
                case 1:
                    return 1;
                case 2:
                    return 2;
                default:
                    return 0;
            }
        }
    }
}
