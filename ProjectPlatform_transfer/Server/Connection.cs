using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.Net;
using System.Net.Sockets;
using ProjectPlatformer.Character;
using ProjectPlatformer.Networking;

namespace Server
{
    [Serializable]
    public class Connection
    {
        public int index;
        public NetworkStream stream;
        public bool lastStreamRecieved = true;
        public PlayerConnection net = new PlayerConnection();
        public PlayerConnection latestNet = new PlayerConnection();
        public byte[] toSend;

        public Connection(NetworkStream netStream, int connectionIndex)
        {
            stream = netStream;
            index = connectionIndex;
        }

        public void UpdateVariables(Player player)
        {
            net.player = player;
        }
    }
}
