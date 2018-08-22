using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using ProjectPlatformer.Grid;
using ProjectPlatformer.Blocks;
using Noise;
using System.IO;

namespace ProjectPlatformer
{
    public class Map
    {
        public static string worldSaveFolder;

        #region load
        public static World world;
        #endregion

        #region grid
        public Cell[] screenCells;
        #endregion

        // create a json file to store map generation properties
        #region Mapvars
        public static int mapWidth = 2000, mapHeight = 400;

        Block.BlockType clusterBlock;

        float treeDensity = .075f; // between 0 and 1

        float[,] blockNoise;
        float blockFrequency = 150f;
        float blockLacunarity = 1f; // between 0 and 1
        int blockOctaves = 1;
        float blockMinValue;
        float blockMaxValue;
        float blockDepthRarity = 1f;

        int offsetUnitsY = 2000, offsetUnitsX = 0;

        Vector2 offset = Vector2.Zero;
        #endregion

        #region 2DHeights
        List<float> heights = new List<float>(); // multiply x value with this list's length
        Vector2[] pointsBetweenHeights;
        List<Vector2> heightPoints = new List<Vector2>();
        #endregion

        public Map()
        {
            Cell.CreateGrid(mapWidth + 10, mapHeight + 10);
            #region SettingVars
            worldSaveFolder = PlatformerGame.cwd + "\\data\\worlds\\";

            heights.Clear();
            heightPoints.Clear();

            pointsBetweenHeights = new Vector2[15];
            screenCells = Cell.GetCellsOnScreen(PlatformerGame.Instance.player.camera.position, PlatformerGame.settings.resolutionWidth, PlatformerGame.settings.resolutionHeight);
            #endregion
        }

        #region Generation
        void GenerateHeights()
        {
            float[] _heights = new float[mapWidth];

            for (int i = 0; i < _heights.Length; i++)
            {
                _heights[i] = offsetUnitsY + (float)PlatformerGame.rand.NextDouble() * 10f * Cell.cellHeight;
            }

            for (int x = 0; x < _heights.Length - 3; x++)
            {
                for (int i = 0; i < pointsBetweenHeights.Length; i++)
                {
                    float height = CubicInterpolate(_heights[x], _heights[x + 1], _heights[x + 2], _heights[x + 3], ((float)i / pointsBetweenHeights.Length));
                    heights.Add(Cell.SnapToGrid(new Vector2(0, height)).Y);

                    pointsBetweenHeights[i] = new Vector2(x * Cell.cellWidth * pointsBetweenHeights.Length, 0f) + new Vector2(i * Cell.cellHeight, height);
                    pointsBetweenHeights[i] = Cell.SnapToGrid(pointsBetweenHeights[i]);
                    heightPoints.Add(pointsBetweenHeights[i]);
                }
            }
        }
        void GenerateMap()
        {
            blockNoise = new float[mapWidth, mapHeight];
            clusterBlock = Block.BlockType.Dirt;

            GenerateHeights();

            blockMinValue = float.MaxValue;
            blockMaxValue = float.MinValue;

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = offsetUnitsX; x < mapWidth; x++)
                {
                    float xCoord = (float)x / mapWidth;
                    float yCoord = (float)y / mapWidth;

                    blockNoise[x, y] = 0;

                    if (heights[x] < (y * Cell.cellHeight)) // these coordinates will contain a block
                    {
                        for (int i = 1; i < blockOctaves + 1; i++) // implement octaves pleeeeeeaaaaaase
                        {
                            //float noise = 0;
                            //noise = Math.Abs(blockNoiseMap.GetPerlin((xCoord * blockFrequency * i) + offset.X, (yCoord * blockFrequency * i) + offset.Y)) * (blockLacunarity / i) * (float)Math.Pow(y, blockDepthRarity) * .0005f;

                        }
                        float noise = NoiseHelper.GustavsonNoise((xCoord * blockFrequency) + offset.X, (yCoord * blockFrequency) + offset.Y) * blockLacunarity * (y / 450f); //(float)Math.Pow(y, blockDepthRarity) * .001f;
                        blockNoise[x, y] = Math.Abs(noise);

                        //set heights
                        Block cellBlock = new Block(GenerateBlockType(x, y));
                        Block.PlaceBlock(Cell.grid[x, y], cellBlock);
                        world.map.Add(Cell.grid[x, y]);

                        if (blockNoise[x, y] < blockMinValue)
                        {
                            blockMinValue = blockNoise[x, y];
                        }
                        else if (blockNoise[x, y] > blockMaxValue)
                        {
                            blockMaxValue = blockNoise[x, y];
                        }
                    }
                }
            }
        }
        Block.BlockType GenerateBlockType(int x, int y)
        {
            float value = blockNoise[x, y];
            Block.BlockType type = Block.BlockType.Dirt;

            Cell cell = Cell.grid[x, y];

            Cell cellAbove = Cell.GetCell(cell, 0, -1);

            int randOre = 0;

            Cell[] surCells;
            Block.BlockType[] surBlocks;
            Block.BlockType[] oreTypes = Block.GetOreBlockTypes();

            if (oreTypes.Contains(clusterBlock)) //only get the three cells in the top right to optimize
            {
                surCells = Cell.GetSurroundingCells(cell);
                surBlocks = new Block.BlockType[surCells.Length];
                for (int i = 0; i < surCells.Length; i++)
                {
                    if (surCells[i] != null && surCells[i].block != null)
                    {
                        surBlocks[i] = surCells[i].block.type;
                    }
                }
                if (!surBlocks.Contains(clusterBlock) && type != clusterBlock)
                {
                    clusterBlock = 0; //setting to Dirt
                }
            }

            if (cellAbove.block == null)
            {
                type = Block.BlockType.Grass;

                float spawnTree = (float)PlatformerGame.rand.NextDouble();

                bool flip = PlatformerGame.rand.Next(0, 2) == 1 ? true : false;

                if (treeDensity > spawnTree)
                {
                    Tree tree = new Tree(PlatformerGame.rand.Next(1, 3), cell, flip); // make first param a float and get all cells that contain part of the tree
                    world.trees.Add(tree);
                }
            }
            else if (value < 0)
            {
                return Block.BlockType.Dirt;
            }
            else if (value > 0.6f)
            {
                if (!oreTypes.Contains(clusterBlock))
                {
                    randOre = PlatformerGame.rand.Next(3, 6);
                    clusterBlock = (Block.BlockType)randOre;
                }
                type = clusterBlock;
            }
            else if (value > .5f)
            {
                type = Block.BlockType.Stone;
            }
            else if (value > .3f)
            {
                //type = Block.BlockType.gold;
            }
            else if (value > .2f)
            {
                type = Block.BlockType.Stone;
            }
            else if (value > .15f)
            {
                //type = Block.BlockType.iron;
            }
            else if (value > .1f)
            {
                type = Block.BlockType.Stone;
            }
            else if (value > .05f)
            {
                type = Block.BlockType.Dirt;
            }
            else if (value >= 0f)
            {
                type = Block.BlockType.Dirt;
            }

            return type;
        }

        #endregion

        float CubicInterpolate(float y0, float y1, float y2, float y3, float mu)
        {
            float a0, a1, a2, a3, mu2;

            mu2 = mu * mu;
            a0 = y3 - y2 - y0 + y1;
            a1 = y0 - y1 - a0;
            a2 = y2 - y0;
            a3 = y1;

            return (a0 * mu * mu2 + a1 * mu2 + a2 * mu + a3);
        }

        #region Loading Game
        public void CreateMap(int seed)
        {
            world = new World();
            NoiseHelper.Seed = seed;
            GenerateMap();
        }
        public void LoadMap(string worldName)
        {
            // deserialize the world file
            world = Serialization.DeserializeProtobuf<World>(worldSaveFolder + worldName);

            mapWidth = world.mapWidth;
            mapHeight = world.mapHeight;

            LoadBlocks();
        }
        private void LoadBlocks()
        {
            foreach (Cell cell in world.map)
            {
                Block.PlaceBlock(cell, cell.block);
            }
            foreach (var tree in world.trees)
            {
                tree.Place();
            }
        }
        public void SaveWorld(string worldName)
        {
            Serialization.SerializeProtobuf(worldSaveFolder + worldName, world);
        }
        #endregion
    }
}
