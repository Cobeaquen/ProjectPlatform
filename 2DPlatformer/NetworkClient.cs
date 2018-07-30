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

namespace ProjectPlatformer
{
    public class NetworkClient
    {
        public List<NetworkPlayer> connectedClients;

        public NetworkPlayer netPlayer = new NetworkPlayer();

        DataSerializer serializer;
        List<DataProcessor> dataProcessors = new List<DataProcessor>();
        Dictionary<string, string> dataProcessorOptions;

        ConnectionInfo serverConnectionInfo;
        Connection serverConnection;

        public string serverIp = "25.88.165.244";
        public int serverPort = 9009;

        IPEndPoint endPoint;

        SendReceiveOptions options;

        Player player = new Player(false);

        public NetworkClient()
        {
            connectedClients = new List<NetworkPlayer>();
            endPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
        }

        public void SetDataProcessors()
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
            NetworkComms.AppendGlobalIncomingPacketHandler<NetworkPlayer>("CustomObject", OnReceivePlayer);
            //NetworkComms.AppendGlobalIncomingPacketHandler<PlayerConnect>("CustomObject", OnNewClientConnect);
            //NetworkComms.AppendGlobalIncomingPacketHandler<PlayerConnection>("CustomObject", OnNewClientConnect);
        }

        public void LoadMultiplayer(Player p)
        {
            SetDataProcessors();
            ConnectToServer();
            AppendListeners();
            player = p;
        }

        public void UpdateMultiplayer()
        {
            if (!serverConnection.ConnectionAlive())
                ConnectToServer();
            SendPlayer();
        }
        public void SendPlayer()
        {
            SetNetworkPlayer();
            serverConnection.SendObject("CustomObject", netPlayer);
        }

        private void SetNetworkPlayer()
        {
            netPlayer.xPos = player.position.X;
            netPlayer.yPos = player.position.Y;
        }

        public void OnNewClientConnect(PacketHeader header, Connection connection, PlayerConnect newPlayer)
        {
            netPlayer.playerIndex = newPlayer.playerIndex;
            Console.WriteLine("JUST RECEIVED THE NEW PACKAGE!!! {0}", netPlayer.playerIndex);
        }

        public void OnReceivePlayer(PacketHeader header, Connection connection, NetworkPlayer p)
        {
            //for (int i = 0; i < connectedClients.Count; i++)
            //{
            //    if (connectedClients[i] == p[i])
            //}
            Console.WriteLine("Just received new player.. X: {0} Y: {1}", p.xPos, p.yPos);
            if (connectedClients.Count < 1)
            {
                connectedClients.Add(p);
            }
            else
            {
                connectedClients[0] = p;
            }
        }
    }
}