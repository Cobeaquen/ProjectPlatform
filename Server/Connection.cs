using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.Net;
using System.Net.Sockets;
using ProjectPlatformer.Character;

namespace Server
{
    [Serializable]
    public class Connection
    {
        public Player player;
        public int index;
        public NetworkStream stream;

        public Connection(NetworkStream netStream, int connectionIndex)
        {
            stream = netStream;
            index = connectionIndex;
        }

        public void UpdateVariables()
        {

        }
    }
}
