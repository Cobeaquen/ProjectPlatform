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

namespace ProjectPlatformer
{
    public class NetworkManager
    {
        public List<PlatformerNetworking> connectedClient;

        public static int port = 9009;
        public static string ip = "195.67.217.18";
        public static TcpClient client;
        public static int playerIndex;
        public PlatformerNetworking platformerNetworking;
        public PlatformerNetworking latestPlatformerNetworking;

        public Player player;

        Byte[] dataReciever = new byte[1500]; // look for resizing later
        Byte[] dataSender;

        NetworkStream stream;

        public static bool connectionDataRecieved = false;
        public bool lastStreamRecieved = true;

        public List<Cell> blockCellsChanged = new List<Cell>();

        private Timer timer;
        private int updateTime = 1;

        public NetworkManager()
        {
            timer = new Timer();
        }

        #region Multiplayer
        public void LoadMultiplayer(Player _player)
        {
            player = _player;
            connectedClient = new List<PlatformerNetworking>(); // change
            client = ConnectToServer(ip, port);
            if (client.Connected)
            {
                stream = client.GetStream();
            }
            platformerNetworking = new PlatformerNetworking(player, Cell.blockCells);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        private void SendDataToServer()
        {
            if (latestPlatformerNetworking == null)
                latestPlatformerNetworking = new PlatformerNetworking();

            PlatformerNetworking dataChanges = platformerNetworking.ChangedVariables(latestPlatformerNetworking, blockCellsChanged);
            if (dataChanges == null) // fix bitte
                return;

            MemoryStream dataStream = Serialization.Serialize(dataChanges);
            byte[] bytes = Serialization.GetBytesFromStream(dataStream);
            byte[] send = new byte[bytes.Length + 1];
            Array.Copy(bytes, 0, send, 1, bytes.Length);
            send[0] = 0; // this is the metadata: sending type PlatformerNetworking

            try
            {
                stream.Flush(); // might be around here .------------------------------------------------------------------------------------------------------------------------------------------------------------> not checking if the latest sent stream was recieved before sending a new one.... ones again.
                stream.Write(send, 0, send.Length);
                lastStreamRecieved = false;

                blockCellsChanged.Clear();
            }
            catch
            {
                lastStreamRecieved = true;
            }

            bytes = new byte[bytes.Length];

            //latestPlatformerNetworking = platformerNetworking;
            //Player newPlayer = Serialization.DeSerialize<Player>(playerStream);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        private void SetNetworkingVariables(byte[] bytes)
        {
            PlatformerNetworking newPlatformerNetwork = Serialization.DeSerialize<PlatformerNetworking>(bytes);
            if (connectedClient.Count < 1)
            {
                connectedClient.Add(newPlatformerNetwork);
            }
            connectedClient[0] = newPlatformerNetwork; // need to change this for when there are more than two people on the server

            if (newPlatformerNetwork.blockCells != null)
            {
                //Cell.blockCells.AddRange(newPlatformerNetwork.blockCells);
                for (int i = 0; i < newPlatformerNetwork.blockCells.Count; i++)
                {
                    newPlatformerNetwork.blockCells[i].block.Sprite = PlatformerGame.instance.Pixel(Cell.cellWidth, Cell.cellHeight);
                    Block.PlaceBlock(newPlatformerNetwork.blockCells[i], new Block(newPlatformerNetwork.blockCells[i].block.Sprite, Cell.cellWidth, Cell.cellHeight));
                    //newPlatformerNetwork.blockCells[i].block.Sprite = Pixel(Cell.cellWidth, Cell.cellHeight); // make a static funtion that converts an enum of a blocktype to a block
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        private void SendByte(byte send)
        {
            byte[] _send = new byte[] { 1 };
            stream.Write(_send, 0, 1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        private void RecieveData(byte[] data)
        {
            byte[] bytes = new byte[data.Length - 1];
            Array.Copy(data, 1, bytes, 0, bytes.Length);

            if (!data.All(b => b == 0))
            {
                switch (data[0])
                {
                    case 0: // this is the data of the player
                        SetNetworkingVariables(bytes);
                        SendByte(1);
                        break;
                    case 1: // this tells us that the last data stream was recieved (properly)
                        lastStreamRecieved = true;
                        Console.WriteLine("the servers data was recieved!!");
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Console.WriteLine("all sent bytes are null");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        public void UpdateMultiplayer()
        {
            platformerNetworking.player = player;
            platformerNetworking.blockCells = Cell.blockCells; // can possibly be optimized by adding to this while adding to the regular list.
            if (timer.GetTime() > updateTime) //|| lastStreamRecieved) // try and add some kind of timer as a backup for if the data was lost. ------------------------------------------------------------------- TAKE A LOOK PLEEEES.
            {
                timer.Reset();
                SendDataToServer();
            }
            else
            {
                timer.Run();
            }
            if (stream.DataAvailable) // get the data that has been sent by the server
            {
                int r = ReadIncomingData(dataReciever);
                if (r > 0)
                {
                    RecieveData(dataReciever);
                }
                dataReciever = new byte[dataReciever.Length];
            }

            /*if (!client.Connected || !connectionDataRecieved)
            {
                if (!client.Connected)
                {
                    try
                    {
                        client.Connect(ip, port);
                    }
                    catch (SocketException)
                    {

                    }
                }
                else if (!connectionDataRecieved)
                {
                    int i = stream.Read(dataReciever, 0, dataReciever.Length);
                    if (i != 0)
                    {
                        playerIndex = BitConverter.ToInt32(dataReciever, 0);
                        Console.WriteLine("playerindex: " + playerIndex);
                        connectionDataRecieved = true;
                    }
                }
                return;
            }*/

            //playerStream.CopyTo(stream);

            /*
            byte[] send01 = BitConverter.GetBytes(sendFloats[0]);
            byte[] send02 = BitConverter.GetBytes(sendFloats[1]);

            byte[] senderData = NetworkManager.CombineData(send01, send02);
            
            dataSender = new byte[senderData.Length + 1];
            Array.Copy(senderData, 0, dataSender, 1, senderData.Length);
            dataSender[0] = 0;

            NetworkManager.SendData(stream, dataSender, true);
            */
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        private int ReadIncomingData(byte[] reader)
        {
            if (stream.DataAvailable)
            {
                int i = stream.Read(reader, 0, reader.Length);
                return i;
            }
            return 0;
        }

        public void PlaceBlock(Cell blockCell)
        {
            blockCellsChanged.Add(blockCell);
        }

        #endregion

        #region StaticFunctions

        public static TcpClient ConnectToServer(string hostname, int port)
        {
            TcpClient client;
            try
            {
                client = new TcpClient(hostname, port);
                Console.WriteLine("Connected Successfully");
            }
            catch (SocketException)
            {
                Console.WriteLine("Failed to connect to the server");
                return new TcpClient();
            }
            return client;
        }

        public static void SendData(NetworkStream stream, Byte[] data, bool append)
        {
            stream.Write(data, 0, data.Length);
        }
        public static int ReadData(NetworkStream stream, ref Byte[] data, bool append)
        {
            return stream.Read(data, 0, data.Length);
        }

        public static byte[] CombineData(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length) + 1];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
        #endregion
    }
}
