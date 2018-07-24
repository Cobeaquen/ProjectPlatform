using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectPlatformer.Blocks;
using ProjectPlatformer.Grid;
using ProjectPlatformer.Character;

namespace ProjectPlatformer.Networking
{
    [Serializable]
    public class PlatformerNetworking
    {
        public Player player;
        public List<Cell> blockCells;

        public PlatformerNetworking (Player _player, List<Cell> _blockCells)
        {
            player = _player;
            blockCells = _blockCells;
        }
        public PlatformerNetworking ()
        {
            player = new Player();
            blockCells = new List<Cell>();
        }

        public PlatformerNetworking ChangedVariables(PlatformerNetworking latestSent, List<Cell> blocksChanged)
        {
            PlatformerNetworking send = new PlatformerNetworking(player, blockCells);
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

        public void GetPlatformerNetworkVariables(PlatformerNetworking latestSave)
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
    }
}
