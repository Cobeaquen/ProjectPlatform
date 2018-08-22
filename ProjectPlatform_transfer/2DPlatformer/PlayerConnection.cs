using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectPlatformer.Blocks;
using ProjectPlatformer.Grid;
using ProjectPlatformer.Character;
using ProjectPlatformer.Time;
using NetworkCommsDotNet.Connections;
using ProtoBuf;

namespace ProjectPlatformer.Networking
{
    [ProtoContract]
    public class PlayerConnection
    {
        [ProtoMember(1)]
        public NetworkPlayer player;
        [ProtoMember(2)]
        public Connection connection;

        public PlayerConnection()
        {

        }


        /*public PlayerConnection(float x, float y, Connection c)
        {
            player.xPos = x;
            player.yPos = y;
            connection = c;
        }*/

        /*public Player player;
        public List<Cell> blockCells;
        public Timer timer;

        public PlayerConnection (Player _player, List<Cell> _blockCells)
        {
            player = _player;
            blockCells = _blockCells;
        }
        public PlayerConnection ()
        {
            player = new Player(false);
            blockCells = new List<Cell>();
        }

        public PlayerConnection ChangedVariables(PlayerConnection latestSent, List<Cell> blocksChanged)
        {
            PlayerConnection send = new PlayerConnection(player, blockCells);
            if (this != latestSent)
            {
                if (player != latestSent.player)
                {
                    if (player.position.X == latestSent.player.position.X)
                        send.player.position.X = 0;
                    if (player.position.Y == latestSent.player.position.Y)
                        send.player.position.Y = 0;
                }
                if (blocksChanged.Count > 0)
                {
                    send.blockCells = blocksChanged; //change later
                }
                else
                {
                    send.blockCells = null;
                }
            }
            else
            {
                send = null;
            }
            return send;
        }

        public void GetPlatformerNetworkVariables(PlayerConnection latestSave)
        {
            if (player.position.X == 0)
                player.position.X = latestSave.player.position.X;
            if (player.position.Y == 0)
                player.position.Y = latestSave.player.position.Y;

            if (blockCells == null)
            {
                blockCells = latestSave.blockCells;
            }
        }
        */
    }
}
