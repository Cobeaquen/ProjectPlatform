using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectPlatformer.Grid;
using ProjectPlatformer.Blocks;
using ProtoBuf;

namespace ProjectPlatformer.Networking
{
    [ProtoContract]
    public class NetworkCell
    {
        [ProtoMember(1)]
        public int x;
        [ProtoMember(2)]
        public int y;
        [ProtoMember(3)]
        public NetworkBlock block;

        public NetworkCell()
        {
            x = 0;
            y = 0;
            block = new NetworkBlock();
            block.Width = Cell.cellWidth;
            block.Height = Cell.cellHeight;
        }
        public Cell ToCell()
        {
            Cell cell = new Cell(x, y);
            cell.block = block.ToBlock();
            cell.x = x;
            cell.y = y;
            return cell;
        }
    }
}
