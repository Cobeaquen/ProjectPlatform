using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectPlatformer.Grid;
using ProtoBuf;

namespace ProjectPlatformer
{
    public class NetworkEntity
    {
        [ProtoMember(1)] public float xPos;
        [ProtoMember(2)] public float yPos;
        public Cell cell;
    }
}
