using Basic_Wars_V2.Enums;
using Basic_Wars_V2.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Basic_Wars_V2.Entities
{
    public class MapManager : IGameEntity, ICollideable
    {
        private const int WINDOW_WIDTH = 1920;
        private const int WINDOW_HEIGHT = 1080;

        public Tile[,] Map { get; set; }
        public List<Tile> Structures { get; private set; } = new List<Tile>();
        public List<Tile> HQs { get; private set; } = new List<Tile>();

        public int MapWidth { get; set; }
        public int MapHeight { get; set; }
        private int NumOfPlayers { get; set; }

        public bool DrawMap { get; set; }

        private const int TILE_DIMENSIONS = 56;
        public Vector2 MapSize { get; set; }
        public Vector2 Position { get; set; }
        private Texture2D Texture { get; set; }
        public Rectangle MapCollider { get; set; }

        public int DrawOrder => 0;

        private int StructureSparsity { get; set; }

        public MapManager(Texture2D texture, int mapWidth, int mapHeight, int numOfPlayers = 2)
        {
            MapWidth = mapWidth;
            MapHeight = mapHeight;
            Texture = texture;
            NumOfPlayers = numOfPlayers;

            Position = new Vector2(WINDOW_WIDTH / 2 - (MapWidth * TILE_DIMENSIONS / 2), WINDOW_HEIGHT / 2 - (MapHeight * TILE_DIMENSIONS / 2));
            MapSize = new Vector2(mapWidth * TILE_DIMENSIONS, mapHeight * TILE_DIMENSIONS);

            ResetMapSize();
            StructureSparsity = (MapWidth / 6) * TILE_DIMENSIONS;

            DrawMap = false;

            GenerateMap();
        }

        private void GenerateMap()
        {
            GenerateBaseMap();
            GenerateStructure("City");
            GenerateStructure("Factory");
            GenerateRoads();
            GenerateHQs();
            CreateTileSprites();
        }

        public void RegenerateMap()
        {
            GenerateRoads();
            CreateTileSprites();
        }

        private void CreateTileSprites()
        {
            foreach (Tile tile in Map)
            {
                tile.CreateTileSpriteOnType();
            }
        }

        public void ClearMap()
        {
            Array.Clear(Map);
        }

        public void ResetMapSize()
        {
            Map = new Tile[MapWidth, MapHeight];
        }

        private void GenerateBaseMap()
        {
            float x;
            float y = Position.Y;
            int randomTile;
            Vector2 tempPosition;

            for (int i = 0; i < MapHeight; i++)
            {
                x = Position.X;
                for (int j = 0; j < MapWidth; j++)
                {
                    randomTile = RandomTile();

                    tempPosition = new Vector2(x, y);
                    Tile newTile = new(tempPosition, Texture)
                    {
                        MapGridPos = new Vector2(j, i)
                    };

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

                    Map[j, i] = newTile;
                    x += 56;
                }
                y += 56;
            }
        }

        private void GenerateStructure(string StructureType)
        {
            TileType Type = TileType.None;

            List<Vector2> points;

            if (StructureType == "City")
            {
                Type = TileType.City;
            }
            else if (StructureType == "Factory")
            {
                Type = TileType.Factory;
                StructureSparsity *= 3;
            }

            PoissonDiscSampling sampler = new();
            points = sampler.GetPoints(StructureSparsity, MapSize);

            foreach (Vector2 point in points)
            {
                int newGridX = (int)(point.X) / TILE_DIMENSIONS;
                int newGridY = (int)(point.Y) / TILE_DIMENSIONS;

                Vector2 newGridPos = new(Map[newGridX, newGridY].Position.X, Map[newGridX, newGridY].Position.Y);

                Tile newStructure = new(newGridPos, Texture);

                if (!(Map[newGridX, newGridY].Type == TileType.City))
                {
                    newStructure.MapGridPos = new Vector2(newGridX, newGridY);
                    Map[newGridX, newGridY] = newStructure;
                    Map[newGridX, newGridY].Type = Type;

                    Structures.Add(newStructure);
                }

            }
        }

        private void GenerateRoads()
        {
            Vector2 firstStructureGridPos;
            Vector2 nextStructureGridPos;

            for (int i = 0; i < Structures.Count - 1; i += 2)
            {
                firstStructureGridPos = Structures[i].MapGridPos;
                nextStructureGridPos = Structures[i + 1].MapGridPos;

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
            if (Map[X, Y].Type != TileType.City
                && Map[X, Y].Type != TileType.Factory
                && Map[X, Y].Type != TileType.Mountain
                && Map[X, Y].Type != TileType.HQ
               )
            {
                Tile roadTile = new(Map[X, Y].Position, Texture)
                {
                    Type = TileType.Road,
                    MapGridPos = new Vector2(X, Y)
                };

                Map[X, Y] = roadTile;
                Map[X, Y].CreateTileSprite(direction);
            }
        }

        public void GenerateHQs()
        {
            int mapWidth = MapWidth - 1;
            int mapHeight = MapHeight - 1;

            switch (NumOfPlayers)
            {
                case 2:
                    CreateHQTile(0, 0, 0);
                    CreateHQTile(mapWidth, mapHeight, 1);
                    break;

                case 3:
                    CreateHQTile(0, 0, 0);
                    CreateHQTile(mapWidth, mapHeight, 1);
                    CreateHQTile(mapWidth, 0, 2);
                    break;

                case 4:
                    CreateHQTile(0, 0, 0);
                    CreateHQTile(mapWidth, mapHeight, 1);
                    CreateHQTile(mapWidth, 0, 2);
                    CreateHQTile(0, mapHeight, 3);
                    break;
            }
        }

        private void CreateHQTile(int X, int Y, int team)
        {
            Tile HQTile = new(Map[X, Y].Position, Texture)
            {
                Type = TileType.HQ,
                MapGridPos = new Vector2(X, Y),
                Team = team
            };

            Map[X, Y] = HQTile;

            Structures.Add(HQTile);
            HQs.Add(HQTile);
        }

        public Rectangle Collider
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, MapWidth * TILE_DIMENSIONS, MapHeight * TILE_DIMENSIONS);
            }
        }

        private static int RandomTile()
        {
            Random random = new();

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
            List<Tile> neighbors = new();

            int X = (int)tile.MapGridPos.X;
            int Y = (int)tile.MapGridPos.Y;

            if (X > 0)
            {
                neighbors.Add(Map[X - 1, Y]);
            }
            if (X < Map.GetLength(0) - 1)
            {
                neighbors.Add(Map[X + 1, Y]);
            }

            if (Y > 0)
            {
                neighbors.Add(Map[X, Y - 1]);
            }
            if (Y < Map.GetLength(1) - 1)
            {
                neighbors.Add(Map[X, Y + 1]);
            }

            return neighbors;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (DrawMap)
            {
                foreach (Tile tile in Map)
                {
                    tile.Draw(spriteBatch, gameTime);
                }
            }
        }

        public void Update(GameTime gameTime)
        {

        }
    }
}
