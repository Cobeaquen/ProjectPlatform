using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectPlatformer.Grid;
using ProjectPlatformer.Blocks;
using ProtoBuf;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectPlatformer.Networking
{
    [ProtoContract]
    public class NetworkBlock
    {
        [ProtoMember(1)]
        public int Width { get; set; } // will remove
        [ProtoMember(2)]
        public int Height { get; set; }
        [ProtoMember(3)]
        public Block.BlockType type { get; set; }

        public NetworkBlock()
        {
            Width = Cell.cellWidth;
            Height = Cell.cellHeight;
        }
        public Block ToBlock()
        {
            Block block = new Block(type, Width, Height);
            return block;
        }
    }
}
