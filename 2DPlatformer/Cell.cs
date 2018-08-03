using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectPlatformer.Blocks;
using Microsoft.Xna.Framework;
using ProjectPlatformer.Networking;
using Microsoft.Xna.Framework.Graphics;
using ProtoBuf;

namespace ProjectPlatformer.Grid
{
    [ProtoContract]
    public class Cell
    {
        public static Cell[] CellsOnScreen { get; set; }

        [ProtoMember(1)] // ?
        public static List<Cell> blockCells = new List<Cell>();

        [ProtoMember(2)]
        public int x;
        [ProtoMember(3)]
        public int y;
        [ProtoMember(4)]
        public Block block;

        public static Cell[,] grid;
        public static readonly int cellWidth = 50, cellHeight = 50;

        public Cell(int _x, int _y) // need to create a new constructor without parameter to serialize
        {
            x = _x;
            y = _y;

            block = null;
        }
        public Cell()
        {

        }

        public void DrawBlock(SpriteBatch batch)
        {
            if (block.Sprite == null)
                block.SetSprite(block.type);
            batch.Draw(block.Sprite, ToVector2(), null, Color.White, 0f, new Vector2(cellWidth / 2, cellHeight / 2), 1f, block.orientation, 0f);
        }

        #region gridfunctions
        public static void CreateGrid(int rows, int columns)
        {
            grid = new Cell[rows, columns];
            for (int y = 0; y < columns; y++)
            {
                for (int x = 0; x < rows; x++)
                {
                    grid[x, y] = new Cell(x * cellWidth, y * cellHeight);
                }
            }
        }

        public static Vector2 SnapToGrid(Vector2 position)
        {
            float x = position.X % cellWidth;
            float y = position.Y % cellHeight;

            Vector2 pos = position;

            if (x > cellWidth/2)
            {
                pos.X += cellWidth - x;
            }
            else
            {
                pos.X -= x;
            }

            if (y > cellHeight/2)
            {
                pos.Y += cellHeight - y;
            }
            else
            {
                pos.Y -= y;
            }
            return pos;
        }
        public static Vector2 SnapToGridNoRound(Vector2 position)
        {
            float x = position.X % cellWidth;
            float y = position.Y % cellHeight;

            Vector2 pos = position;
            pos.X -= x;
            pos.Y -= y;
            return pos;
        }
        public static Cell GetCell(Vector2 position)
        {
            int posX = (int)Math.Round(position.X);
            int posY = (int)Math.Round(position.Y);
            try
            {
                return grid[posX / cellWidth, posY / cellHeight];
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }
        public static Cell GetCell(int x, int y)
        {
            return grid[x / cellWidth, y / cellHeight];
        }

        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }
        public NetworkCell ToNetworkCell()
        {
            NetworkCell netCell = new NetworkCell();
            netCell.block = block.ToNetworkBlock();
            netCell.x = x;
            netCell.y = y;
            return netCell;
        }

        #region Cells Nearby

        public static Cell GetCell(Cell cell, int offsetX, int offsetY)
        {
            try
            {
                return grid[(cell.x / cellWidth) + offsetX, (cell.y / cellHeight) + offsetY];
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }
        public static void SetGridBlock(Cell cell, Block block)
        {
            grid[cell.x / cellWidth, cell.y / cellHeight].block = block;
            blockCells.Add(grid[cell.x / cellWidth, cell.y / cellHeight]);
        }

        public static Cell[] GetSurroundingCells(Cell cell)
        {
            return new Cell[] { GetCell(cell, 1, 0), GetCell(cell, -1, 0), GetCell(cell, 0, 1), GetCell(cell, 0, -1), GetCell(cell, 1, 1), GetCell(cell, -1, 1), GetCell(cell, 1, -1), GetCell(cell, -1, -1) };
        }
        public static Cell[] GetSurroundingCells(Cell topLeftcell, int width, int height)
        {
            List<Cell> cells = new List<Cell>();

            for (int y = 0; y < height; y++)
            {
                Cell cell01 = GetCell(topLeftcell, width, y);
                cells.Add(cell01);
                Cell cell02 = GetCell(topLeftcell, -1, y);

                cells.Add(cell02);
            }
            for (int x = 0; x < width; x++)
            {
                Cell cell01 = GetCell(topLeftcell, x, height);
                cells.Add(cell01);
                Cell cell02 = GetCell(topLeftcell, x, -1);
                if (cell02 != null)
                {
                    cells.Add(cell02);
                }
            }
            Cell[] cel;
            cel = cells.Where(c => c != null).ToArray();
            return cel;
        }
        public static Cell[] GetAreaOfCells(Cell origin, int width, int height)
        {
            int halfWidth = width / 2;
            int halfHeight = height / 2;
            Cell startPoint = GetCell(origin, -halfWidth, -halfHeight);
            Cell[] cells = new Cell[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    cells[y * width + x] = GetCell(startPoint.x + (x * cellWidth), startPoint.y + (y * cellHeight));
                }
            }
            return cells;
        }
        #endregion
        public static Cell[] GetCellsOnScreen(Vector2 cameraPos, int screenWidth, int screenHeight)
        {
            int shellWidth = 2;
            int shellHeight = 2;
            Vector2 camCornerPos = new Vector2(cameraPos.X - (screenWidth / 2f) - cellWidth, cameraPos.Y - (screenHeight / 2f) - ((shellHeight / 2) * cellHeight));
            Vector2 camCellPos = SnapToGrid(camCornerPos);
            Cell camCell = GetCell(camCellPos);

            if (camCell == null)
            {
                return null;
            }

            float widthRest = screenWidth % cellWidth;

            int width = screenWidth + (cellWidth - (int)widthRest);
            int xCells = (width / cellWidth) + shellWidth;

            float heightRest = screenHeight % cellHeight;

            int height = screenHeight + (cellHeight - (int)heightRest);
            int yCells = (height / cellHeight) + shellHeight;

            Cell[] cells = new Cell[xCells * yCells];

            for (int y = 0; y < yCells; y++)
            {
                for (int x = 0; x < xCells; x++)
                {
                    int arrayIndex = xCells * y + x;

                    try
                    {
                        cells[arrayIndex] = grid[(camCell.x / cellWidth) + x, (camCell.y / cellWidth) + y];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        return cells;
                    }
                }
            }

            return cells;
        }
        public static void UpdateCellsOnScreen(Vector2 cameraPos, int screenWidth, int screenHeight)
        {
            CellsOnScreen = GetCellsOnScreen(cameraPos, screenWidth, screenHeight);
        }
        #endregion
    }
}
