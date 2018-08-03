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
using ProtoBuf;

namespace ProjectPlatformer.Blocks
{
    [ProtoContract]
    public class Block
    {
        public Vector2 Origin { get; set; }
        public Texture2D Sprite { get; set; }
        [ProtoMember(1)]
        public BlockType type { get; set; }
        public SpriteEffects orientation { get; set; }
        private SpriteEffects[] allowedOrientations { get; set; }

        public Block(BlockType blockType)
        {
            SetSprite(blockType); // will cause an error if this is called before calling LoadContent() in the main class
            Origin = new Vector2(Cell.cellWidth/2f, Cell.cellHeight/2f);

            SetOrientation();
        }
        public Block()
        {
            Origin = new Vector2(Cell.cellWidth / 2f, Cell.cellHeight / 2f);
        }

        public void SetSprite(BlockType type) // can i do this??!??
        {
            this.type = type;
            Sprite = PlatformerGame.Instance.Load<Texture2D>(type.ToString());

            SetOrientation();
        }
        public void SetOrientation()
        {
            int num01 = 0;
            if (type == BlockType.Grass)
            {
                allowedOrientations = new SpriteEffects[] { SpriteEffects.None, SpriteEffects.FlipHorizontally };
            }
            else
            {
                allowedOrientations = new SpriteEffects[] { SpriteEffects.None, SpriteEffects.FlipHorizontally, SpriteEffects.FlipVertically };
                num01 = PlatformerGame.rand.Next(1, allowedOrientations.Length);
            }

            int num = PlatformerGame.rand.Next(allowedOrientations.Length);

            if (num01 != 0) // if the blocktype allows more than 2 orientations
            {
                orientation = (SpriteEffects)num | (SpriteEffects)num01;
            }
            else
            {
                orientation = (SpriteEffects)num;
            }
        }

        public NetworkBlock ToNetworkBlock()
        {
            NetworkBlock netBlock = new NetworkBlock();
            netBlock.type = type;
            return netBlock;
        }

        public static BlockType[] GetOreBlockTypes()
        {
            BlockType[] ores = { BlockType.Diamond, BlockType.Gold, BlockType.Iron };
            return ores;
        }

        public static void PlaceBlock(Cell cell, Block block)
        {
            Cell.SetGridBlock(cell, block);
        }

        public enum BlockType
        {
            Dirt,
            Stone,
            Grass,
            Diamond,
            Gold,
            Iron
        };
    }
}
