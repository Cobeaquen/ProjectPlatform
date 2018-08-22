using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using ProjectPlatformer.Grid;

namespace ProjectPlatformer
{
    [ProtoContract]
    public class World
    {
        [ProtoMember(1)]
        public List<Cell> map { get; set; }
        [ProtoMember(2)]
        public List<Tree> trees { get; set; }
        [ProtoMember(3)]
        public int mapWidth { get; set; }
        [ProtoMember(4)]
        public int mapHeight { get; set; }

        public World()
        {
            map = new List<Cell>();
            trees = new List<Tree>();
        }

        public void AddBlock(Cell cell) // may neeed to add block parameter and fix that shet
        {
            map.Add(cell);
        }
    }
}
