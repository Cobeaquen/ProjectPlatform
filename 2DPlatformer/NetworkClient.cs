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
using NetworkCommsDotNet;
using NetworkCommsDotNet.DPSBase;
using SharpZipLibCompressor;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet.Tools;
using NetworkCommsDotNet.Connections.TCP;
using ProtoBuf;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ProjectPlatformer.Networking
{
    public class NetworkClient
    {
        public List<NetworkPlayer> connectedClients { get; set; }
        public List<NetworkPlayer> previousClientStates { get; set; } // look into cleaning and putting some of these in the NetworkPlayer class
        private List<float> lerpProgress;
        public List<int> clients { get; set; }

        /// <summary>
        /// The entire map, will later be stored in a file rather than memory
        /// </summary>
        public List<Cell> blockCells { get; set; }

        /// <summary>
        /// Total connections to the server
        /// </summary>
        public int serverConnections { get; set; }

        public NetworkPlayer netPlayer = new NetworkPlayer();
        public NetworkPlayer previousNetPlayer = new NetworkPlayer();
        public static MultiplayerSettings settings;

        DataSerializer serializer { get; set; }
        List<DataProcessor> dataProcessors { get; set; } = new List<DataProcessor>();
        Dictionary<string, string> dataProcessorOptions;

        ConnectionInfo serverConnectionInfo;
        Connection serverConnection;

        public string ServerIp { get; set; }
        public int ServerPort { get; set; }
        IPEndPoint endPoint { get; set; }
        SendReceiveOptions options { get; set; }

        Player player = new Player();

        private Timer sendTimer { get; set; }

        /// <summary>
        /// The time in millieseconds between each time we send data to the server.
        /// </summary>
        private int UpdateFrequency { get; set; } = 50;

        private bool isConnected { get; set; }

        public NetworkClient()
        {
            SetMultiplayerSettings();
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
            NetworkComms.AppendGlobalIncomingPacketHandler<NetworkPlayer>("Player", OnReceivePlayer); // low priority
            NetworkComms.AppendGlobalIncomingPacketHandler<NetworkPlayer>("OnConnect", OnConnectedPlayer); // high priority
            NetworkComms.AppendGlobalIncomingPacketHandler<NetworkCell[]>("OnConnectBlocks", OnConnectedBlocks); // high priority
            NetworkComms.AppendGlobalIncomingPacketHandler<NetworkCell>("CellChanged", OnBlockCellChanged); // add a high priority on this one
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
        }
        Vector2 playerPos = Vector2.Zero; // move

        public void Draw(SpriteBatch batch)
        {
            if (connectedClients.Count > 0)
            {
                try
                {
                    int i = 0;
                    foreach (NetworkPlayer p in connectedClients)
                    {
                        //playerPos = new Vector2(previousClientStates[i].xPos, previousClientStates[i].yPos);
                        // Vector2.Lerp(begin, target, (UpdateFrequency * 2f) / 1000f);

                        if (settings.SmoothMovement)
                        {
                            playerPos = SmoothPlayerPosition(new Vector2(previousClientStates[i].xPos, previousClientStates[i].yPos), new Vector2(p.xPos, p.yPos), lerpProgress[i]);

                            lerpProgress[i] += 1f / 4f; //(1f/UpdateFrequency) * 12f;

                            if (lerpProgress[i] > 1f)
                            {
                                lerpProgress[i] = 1f;
                            }
                        }
                        else
                        {
                            playerPos = new Vector2(p.xPos, p.yPos);
                        }

                        //playerPos += new Vector2(p.xPos, p.yPos) * (1f - 0.1f);
                        batch.Draw(player.Sprite, new Vector2(playerPos.X, playerPos.Y), null, Color.Purple, 0f, player.Origin, Vector2.One, SpriteEffects.None, 0f); //change the sprite

                        Console.WriteLine(lerpProgress[i]);

                        i++;
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        private Vector2 SmoothPlayerPosition(Vector2 begin, Vector2 target, float progress)
        {
            return Vector2.Lerp(begin, target, progress);// (UpdateFrequency*2f)/1000f);
        }

        public void SendDataCallBack(object state)
        {
            if (isConnected)
            {
                Console.WriteLine("Sending data!!!!!!!!");

                try
                {
                    SendPlayer();
                }
                catch (CommunicationException)
                {
                    Console.WriteLine("Failed to send player");
                }
            }
        }

        public void SendPlayer()
        {
            SetNetworkPlayer();
            float deltaMove = Vector2.Distance(new Vector2(previousNetPlayer.xPos, previousNetPlayer.yPos), new Vector2(netPlayer.xPos, netPlayer.yPos)); // probably want to change, expensive and inefficient calculation
            if (deltaMove > settings.MinDeltaMove)
            {
                serverConnection.SendObject("Player", netPlayer);
                previousNetPlayer = netPlayer.Clone();
            }
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
                previousClientStates.Add(p);
                lerpProgress.Add(0f);
            }

            int i = clients.IndexOf(p.playerIndex);

            //Console.WriteLine("Just received new player {0}'s position X: {1} Y: {2}", p.playerIndex, p.xPos, p.yPos);

            playerPos = new Vector2(connectedClients[i].xPos, connectedClients[i].yPos);
            lerpProgress[i] = 0f;

            previousClientStates[i] = connectedClients[i];
            connectedClients[i] = p;

            //Console.WriteLine("Previous position: {0}\n" +
            //    "Latest position: {1}", previousClientStates[i].yPos, connectedClients[i].yPos);
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
                    blockCells.Add(blockCell); // ?????
                    break;
            }
            serverConnection.SendObject("CellChanged", newCell);
        }
        public void Exit()
        {
            NetworkComms.Shutdown();
        }

        public void SetMultiplayerSettings()
        {
            string filePath = PlatformerGame.OptionsPath + "Multiplayer Settings.platform";
            if (!File.Exists(filePath))
            {
                settings = new MultiplayerSettings()
                {
                    ServerIp = IPAddress.Any.ToString(),
                    ServerPort = 9009,
                    UpdateIntervals = 50,
                    MinDeltaMove = 1f,
                    SmoothMovement = true
                };

                Serialization.SerializeJson(filePath, settings);
            }
            else
            {
                settings = Serialization.DeserializeJson<MultiplayerSettings>(filePath);
            }

            sendTimer = new Timer(SendDataCallBack, null, 0, settings.UpdateIntervals);
            ServerIp = settings.ServerIp;
            ServerPort = settings.ServerPort;

            connectedClients = new List<NetworkPlayer>();
            previousClientStates = new List<NetworkPlayer>();
            lerpProgress = new List<float>();

            clients = new List<int>();
            blockCells = new List<Cell>();
            endPoint = new IPEndPoint(IPAddress.Parse(ServerIp), ServerPort);
        }
    }
    public enum CellChangeType
    {
        RemoveBlock,
        PlaceBlock
    };
}