using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Sockets;
using ProjectPlatformer.Networking;
using ProjectPlatformer.Grid;
using ProjectPlatformer.Blocks;
using ProjectPlatformer.Character;
using ProjectPlatformer.Time;
using NetworkCommsDotNet;
using NetworkCommsDotNet.DPSBase;
using SharpZipLibCompressor;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet.Tools;
using NetworkCommsDotNet.Connections.TCP;
using ProtoBuf;
using System.Threading;

namespace ProjectPlatformer.Networking
{
    public class NetworkClient
    {
        public List<NetworkPlayer> connectedClients;
        public List<int> clients;

        public List<Cell> blockCells;

        /// <summary>
        /// Total connections to the server
        /// </summary>

        public int serverConnections { get; set; }

        public NetworkPlayer netPlayer = new NetworkPlayer();

        DataSerializer serializer { get; set; }
        List<DataProcessor> dataProcessors { get; set; } = new List<DataProcessor>();
        Dictionary<string, string> dataProcessorOptions;

        ConnectionInfo serverConnectionInfo;
        Connection serverConnection;

        public string serverIp { get; set; } = "25.88.165.244";
        public int serverPort { get; set; } = 9009;

        IPEndPoint endPoint { get; set; }

        SendReceiveOptions options { get; set; }

        Player player = new Player();
        private List<NetworkCell> netCells;

        private bool isConnected;

        public NetworkClient()
        {
            connectedClients = new List<NetworkPlayer>();
            clients = new List<int>();
            netCells = new List<NetworkCell>();
            blockCells = new List<Cell>();
            endPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
        }

        public void SetSendReceiveOptions()
        {
            serializer = DPSManager.GetDataSerializer<ProtobufSerializer>();
            dataProcessors.Add(DPSManager.GetDataProcessor<SharpZipLibGzipCompressor>());
            dataProcessorOptions = new Dictionary<string, string>();

            options = new SendReceiveOptions(serializer, dataProcessors, dataProcessorOptions);
            NetworkComms.DefaultSendReceiveOptions = options;
        }

        public void ConnectToServer()
        {
            serverConnectionInfo = new ConnectionInfo(endPoint, ApplicationLayerProtocolStatus.Enabled);
            int attempts = 0;
            Console.WriteLine("Connecting to server...");
            while (serverConnection == null)
            {
                try
                {
                    serverConnection = TCPConnection.GetConnection(serverConnectionInfo);
                    Console.WriteLine("Connected to server");
                    break;
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to connect, attempts: {0}", attempts);
                    attempts++;
                }
            }
        }

        private void AppendListeners()
        {
            NetworkComms.AppendGlobalConnectionCloseHandler(OnServerClose);
            NetworkComms.AppendGlobalIncomingPacketHandler<NetworkPlayer>("Player", OnReceivePlayer);
            NetworkComms.AppendGlobalIncomingPacketHandler<NetworkPlayer>("OnConnect", OnConnectedPlayer);
            NetworkComms.AppendGlobalIncomingPacketHandler<NetworkCell[]>("OnConnectBlocks", OnConnectedBlocks);
            NetworkComms.AppendGlobalIncomingPacketHandler<NetworkCell>("CellChanged", OnBlockCellChanged);
        }

        public void LoadMultiplayer(Player p)
        {
            SetSendReceiveOptions();
            AppendListeners();
            ConnectToServer();
            player = p;
        }

        public void UpdateMultiplayer()
        {
            if (!serverConnection.ConnectionAlive())
                ConnectToServer();
            if (!isConnected)
                return;
            try
            {
                SendPlayer();
            }
            catch (CommunicationException)
            {
                Console.WriteLine("Failed to send player");
            }
        }
        public void SendPlayer()
        {
            SetNetworkPlayer();
            serverConnection.SendObject("Player", netPlayer);
        }

        private void SetNetworkPlayer()
        {
            netPlayer.xPos = player.position.X;
            netPlayer.yPos = player.position.Y;
        }

        public void OnConnectedPlayer(PacketHeader header, Connection connection, NetworkPlayer newPlayer)
        {
            netPlayer = newPlayer;
            serverConnections++;
            Console.WriteLine("You are player: {0}", netPlayer.playerIndex);
            isConnected = true;
        }
        public void OnConnectedBlocks (PacketHeader header, Connection connection, NetworkCell[] newBlockCells)
        {
            foreach (NetworkCell cell in newBlockCells)
            {
                Cell newCell = cell.ToCell();
                blockCells.Add(newCell);
                Block.PlaceBlock(newCell, newCell.block);
            }
        }

        public void OnServerClose(Connection connection)
        {
            Console.WriteLine("Lost Connection");
            Exit();
            PlatformerGame.Instance.Exit();
        }

        public void OnReceivePlayer(PacketHeader header, Connection connection, NetworkPlayer p)
        {
            if (!clients.Contains(p.playerIndex))
            {
                clients.Add(p.playerIndex);
                serverConnections++;
                connectedClients.Add(p);
            }

            int i = clients.IndexOf(p.playerIndex);

            //Console.WriteLine("Just received new player {0}'s position X: {1} Y: {2}", p.playerIndex, p.xPos, p.yPos);

            connectedClients[i] = p;
        }

        public void OnBlockCellChanged(PacketHeader header, Connection connection, NetworkCell cell)
        {
            Console.WriteLine("JUST RECEIVED NEW CELL!!!");

            Cell newCell = cell.ToCell();

            Block.PlaceBlock(newCell, newCell.block);
        }

        public void ChangeCell(Cell blockCell, CellChangeType change)
        {
            NetworkCell newCell = blockCell.ToNetworkCell();
            
            switch (change)
            {
                case CellChangeType.RemoveBlock:
                    newCell.block = null;
                    break;
                case CellChangeType.PlaceBlock:
                    newCell.block.Width = blockCell.block.Width;
                    newCell.block.Height = blockCell.block.Height;
                    blockCells.Add(blockCell); // ?????
                    break;
            }
            serverConnection.SendObject("CellChanged", newCell);
        }
        public void Exit()
        {
            NetworkComms.Shutdown();
        }
    }
    public enum CellChangeType
    {
        RemoveBlock,
        PlaceBlock
    };
}