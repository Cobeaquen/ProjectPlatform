using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace ProjectPlatformer
{   [ProtoContract]
    public class NetworkPlayer
    {
        [ProtoMember(1)]public float xPos;
        [ProtoMember(2)]public float yPos;
        public int playerIndex; // loop through and check if it matches the current one
    }
    [ProtoContract]
    public class PlayerConnect
    {
        [ProtoMember(1)]public int playerIndex;
    }
}
