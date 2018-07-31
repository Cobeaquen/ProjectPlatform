using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace ProjectPlatformer.Networking
{   [ProtoContract]
    public class NetworkPlayer
    {
        [ProtoMember(1)]public float xPos;
        [ProtoMember(2)]public float yPos;
        [ProtoMember(3)]public int playerIndex; // loop through and check if it matches the current one

        public NetworkPlayer()
        {
            xPos = 0;
            yPos = 0;
            playerIndex = 0;
        }
    }
}
