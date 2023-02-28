using Basic_Wars_V2.Enums;
using Basic_Wars_V2.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
    public class MapManager : IGameEntity, ICollideable
    {
        public Tile[,] map;

        private List<Tile> tempTiles = new List<Tile>();

        private const int WINDOW_WIDTH = 1920;
        private const int WINDOW_HEIGHT = 1080;

        private int MapWidth { get; set; }
        private int MapHeight { get; set; }
        private int NumOfPlayers { get; set; }

        private const int TILE_DIMENSIONS = 56;
        public Vector2 MapSize { get; set; }
        public Vector2 Position { get; set; }
        public Texture2D Texture { get; set; }
        public Rectangle MapCollider { get; set; }

        public int DrawOrder => 0;

        public List<Vector2> structurePoints = new List<Vector2>();

        public List<Vector2> structureCoordinates = new List<Vector2>();
        private int StructureSparsity { get; set; }

        public MapManager(Texture2D texture, int mapWidth, int mapHeight, int numOfPlayers = 2)
        {
            MapWidth = mapWidth;
            MapHeight = mapHeight;
            Texture = texture;
            NumOfPlayers= numOfPlayers;

            Position = new Vector2(WINDOW_WIDTH / 2 - (MapWidth * TILE_DIMENSIONS / 2), WINDOW_HEIGHT / 2 - (MapHeight * TILE_DIMENSIONS / 2));
            MapSize = new Vector2(mapWidth * TILE_DIMENSIONS, mapHeight * TILE_DIMENSIONS);

            map = new Tile[MapWidth, MapHeight];
            StructureSparsity = (MapWidth / 6) * TILE_DIMENSIONS;

            GenerateMap();
        }

        private void GenerateMap()
        {
            GenerateBaseMap();
            GenerateStructure("City");
            GenerateStructure("Factory");
            GenerateRoads();
            GenerateHQs();
        }

        private void GenerateBaseMap()
        {
            float x = Position.X;
            float y = Position.Y;
            int randomTile;
            Vector2 tempPosition = new Vector2(x, y);

            for (int i = 0; i < MapHeight; i++)
            {
                x = Position.X;
                for (int j = 0; j < MapWidth; j++)
                {
                    randomTile = RandomTile();

                    tempPosition = new Vector2(x, y);
                    Tile newTile = new Tile(tempPosition, Texture);
                    newTile.MapGridPos = new Vector2(j, i);

                    switch (randomTile)
                    {
                        case 0:
                            newTile.Type = TileType.Plains;
                            break;
                        case 1:
                            newTile.Type = TileType.Forest;
                            break;
                        case 2:
                            newTile.Type = TileType.Mountain;
                            break;
                    }

                    
                    map[j, i] = newTile;
                    map[j, i].CreateTile(randomTile);
                    x += 56;
                }
                y += 56;
            }
        }

        private void GenerateStructure(string StructureType)
        {
            int StructureColumnShift = 0;
            int StructureRowShift = 0;

            TileType Type = TileType.None;

            List<Vector2> points = new List<Vector2>();

            if (StructureType == "City")
            {
                Type = TileType.City;
                StructureColumnShift = -6;
            }
            else if (StructureType == "Factory")
            {
                Type = TileType.Factory;
                StructureSparsity *= 2;
                StructureColumnShift = -6;
                StructureRowShift= 1;
            }

            points = PoissonDiscSampling.GetPoints(StructureSparsity, MapSize);

            foreach (Vector2 point in points)
            {
                int newGridX = (int)(point.X) / TILE_DIMENSIONS;
                int newGridY = (int)(point.Y) / TILE_DIMENSIONS;

                Vector2 newGridPos = new Vector2(map[newGridX, newGridY].Position.X, map[newGridX, newGridY].Position.Y);

                Tile newStructure = new Tile(newGridPos, Texture);

                if (!(map[newGridX, newGridY].Type == TileType.City))
                {
                    newStructure.MapGridPos = new Vector2(newGridX, newGridY);
                    map[newGridX, newGridY] = newStructure;
                    map[newGridX, newGridY].Type = Type;
                    map[newGridX, newGridY].CreateTile(StructureColumnShift, StructureRowShift);

                    structureCoordinates.Add(new Vector2(newGridX, newGridY));
                }

            }
        }

        private void GenerateRoads()
        {
            Vector2 firstStructureGridPos = new Vector2(0, 0);
            Vector2 nextStructureGridPos = new Vector2(0, 0);

            for (int i = 0; i < structureCoordinates.Count - 1; i += 2)
            {
                firstStructureGridPos = structureCoordinates[i];
                nextStructureGridPos = structureCoordinates[i + 1];

                BuildRoad(firstStructureGridPos, nextStructureGridPos);
            }
        }

        private void BuildRoad(Vector2 firstStrcuturePos, Vector2 nextStructurePos)
        {
            int x0 = (int)firstStrcuturePos.X;
            int y0 = (int)firstStrcuturePos.Y;
            int x1 = (int)nextStructurePos.X;
            int y1 = (int)nextStructurePos.Y;

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

            while (y0 != y1)
            {
                if (y0 > y1)
                {
                    y0--;
                    CreateRoadTile(x0, y0, 4);
                }
                else if (y0 < y1)
                {
                    y0++;
                    CreateRoadTile(x0, y0, 4);
                }
            }

            
        }

        private void CreateRoadTile(int X, int Y, int direction)
        {
            if (map[X, Y].Type != TileType.City && map[X, Y].Type != TileType.Factory && map[X, Y].Type != TileType.Mountain)
            {
                Tile roadTile = new Tile(map[X, Y].Position, Texture);
                roadTile.Type = TileType.Road;
                roadTile.MapGridPos = new Vector2(X, Y);

                map[X, Y] = roadTile;
                map[X, Y].CreateTile(direction);
            }
        }

        public void GenerateHQs()
        {
            int mapWidth = MapWidth - 1;
            int mapHeight = MapHeight - 1;  

            switch (NumOfPlayers)
            {
                case 2:
                    CreateHQTile(0, 0);
                    CreateHQTile(mapWidth, mapHeight);
                    break;

                case 3:
                    CreateHQTile(0, 0);
                    CreateHQTile(mapWidth, mapHeight);
                    CreateHQTile(mapWidth, 0);
                    break;

                case 4:
                    CreateHQTile(0, 0);
                    CreateHQTile(mapWidth, mapHeight);
                    CreateHQTile(mapWidth, 0);
                    CreateHQTile(0, mapHeight);
                    break;
            }
        }

        private void CreateHQTile(int X, int Y)
        {
            Tile HQTile = new Tile(map[X, Y].Position, Texture);
            HQTile.Type = TileType.HQ;
            HQTile.MapGridPos = new Vector2(X, Y);

            map[X, Y] = HQTile;
            map[X, Y].CreateTile(-6, 2);
        }

        public Rectangle Collider
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, MapWidth * TILE_DIMENSIONS, MapHeight * TILE_DIMENSIONS);
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

        public int GetCost(Tile tile, Unit unit)
        {
            int terrainCost = 0;

            if (unit.Type == UnitType.Infantry || unit.Type == UnitType.Mech)
            {
                switch (tile.Type)
                {
                    case TileType.Plains:
                    case TileType.Forest:
                    case TileType.Road:
                        terrainCost = 1;
                        break;

                    case TileType.Mountain:
                    case TileType.City:
                    case TileType.Factory:
                    case TileType.HQ:
                        terrainCost = 2;
                        break;
                }
            }
            if (unit.Type == UnitType.Tank || unit.Type == UnitType.APC)
            {
                switch (tile.Type)
                {
                    case TileType.Road:
                        terrainCost = 1;
                        break;

                    case TileType.Plains:
                    case TileType.Forest:
                        terrainCost = 2;
                        break;

                    case TileType.City:
                    case TileType.Factory:
                    case TileType.HQ:
                        terrainCost = 3;
                        break;

                    case TileType.Mountain:
                        terrainCost = 6;
                        break;
                }
            }

            return terrainCost;

        }

        public List<Tile> GetNeighbours(Tile tile)
        {
            List<Tile> neighbors = new List<Tile>();

            int X = (int)tile.MapGridPos.X;
            int Y = (int)tile.MapGridPos.Y;

            if (X > 0)
            {
                neighbors.Add(map[X - 1, Y]);
            }
            if (X < map.GetLength(0) - 1)
            {
                neighbors.Add(map[X + 1, Y]);
            }

            if (Y > 0)
            {
                neighbors.Add(map[X, Y - 1]);
            }
            if (Y < map.GetLength(1) - 1)
            {
                neighbors.Add(map[X, Y + 1]);
            }

            return neighbors;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            foreach (Tile tile in map)
            {
                tile.Draw(spriteBatch, gameTime);
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach (Tile tile in map)
            {
                tile.Update(gameTime);
            }
        }
    }
}
