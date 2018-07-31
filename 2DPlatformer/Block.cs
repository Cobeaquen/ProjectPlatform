using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectPlatformer.Grid;
using ProjectPlatformer.Networking;

namespace ProjectPlatformer.Blocks
{
    public class Block
    {
        //public Cell cell;
        public Vector2 Origin { get; set; }
        public Texture2D Sprite { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public BlockType type { get; set; }

        public Block(BlockType blockType, int width, int height)
        {
            type = blockType;
            Sprite = PlatformerGame.Instance.Load<Texture2D>(type.ToString()); // will cause an error if this is called before calling LoadContent() in the main class
            Width = width;
            Height = height;
            Origin = new Vector2(width/2, height/2);
        }
        public NetworkBlock ToNetworkBlock()
        {
            NetworkBlock netBlock = new NetworkBlock();
            netBlock.type = type;
            netBlock.Width = Cell.cellWidth;
            netBlock.Height = Cell.cellHeight;
            return netBlock;
        }

        public void DrawBlock(SpriteBatch batch)
        {

        }

        public static void PlaceBlock(Cell cell, Block block)
        {
            Cell.SetGridBlock(cell, block);
        }

        public enum BlockType
        {
            Dirt,
            Grass
        };
    }
}
