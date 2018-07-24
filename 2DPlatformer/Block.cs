using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectPlatformer.Grid;

namespace ProjectPlatformer.Blocks
{
    [Serializable]
    public class Block
    {
        [NonSerialized]public Cell cell;
        [NonSerialized]public Vector2 Origin;
        [NonSerialized]public Texture2D Sprite;
        public int Width, Height; // might wanna add [nonserialized] attribute

        public Block(Texture2D sprite, int width, int height)
        {
            Sprite = sprite;
            Width = width;
            Height = height;
            Origin = new Vector2(width/2, height/2);
        }

        public void DrawBlock(SpriteBatch batch)
        {

        }

        public static void PlaceBlock(Cell cell, Block block)
        {
            Cell.SetGridBlock(cell, block);
        }
    }
}
