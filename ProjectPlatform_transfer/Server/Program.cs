using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.Net;
using System.Net.Sockets;
using System.IO;
using ProjectPlatformer;
using ProjectPlatformer.Networking;
using ProjectPlatformer.Time;

namespace Server
{
    class Program
    {
        static List<Connection> connections = new List<Connection>();

        static IPAddress ip;
        static Int32 port = 9009;
        static List<byte[]> read = new List<byte[]>();
        static byte[] databytes = new byte[2000]; // look for resizing later
        static TcpListener server;
        static List<TcpClient> clients = new List<TcpClient>();
        static PlayerConnection latestNetVars;
        static PlayerConnection playerToSend;

        static float timeTpUpdate = 1000;

        static int timeToMoveText = 10;

        static int pos;

        static float updateSpeed = 100f;
        static Timer timer = new Timer();

        static bool lastStreamRecieved = false;
        static int timeBetweenSends = 10;

        static void Main(string[] args)
        {
            ip = GetIpAddress();
            server = new TcpListener(ip, port);
            connections.Clear();

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;

            Console.SetBufferSize(Console.BufferWidth, Console.BufferHeight);
            pos = 500;

            Launch();
        }

        static void CheckForIncomingConnection()
        {
            try
            {
                server.BeginAcceptTcpClient(new AsyncCallback(AcceptConnection), server);
            }
            catch
            {
                throw;
            }
        }

        static void Launch()
        {
            server.Start();

            CheckForIncomingConnection();
            Console.WriteLine("Waiting for a connection...");

            lastStreamRecieved = true;

            RunServer();
        }

        /*static void MoveConsole ()
        {
            //Console.WriteLine(timer.GetTime());
            if (timer.GetTime() > timeToMoveText)
            {
                timer.Reset();
                pos++;
                if (pos > 0)
                {
                    pos = Console.WindowWidth;
                }
                Console.SetWindowPosition(pos, 0);
            }
            else
            {
                timer.Run();
            }
        }*/

        static void RunServer()
        {
            while (true)
            {
                //MoveConsole();

                if (clients.Count > 0)
                {
                    timer.Run();
                    for (int i = 0; i < connections.Count; i++)
                    {
                        if (connections[i].lastStreamRecieved || timer.GetTime() > timeTpUpdate)
                        {
                            timeTpUpdate = timeTpUpdate + timer.GetTime();
                            if (connections[i].stream != null)
                            {
                                if (connections[i].stream.DataAvailable) // listening for data
                                {
                                    Console.WriteLine("data is available for launch");
                                    int r;
                                    if ((r = connections[i].stream.Read(read[i], 0, read[i].Length)) > 0)
                                    {
                                        byte[] bytes = GetBytesWithoutMetadata(read[i]);

                                        if (!(bytes.Any(b => b != 0)))
                                        {
                                            Console.WriteLine("Recieved bytes are of NULL value");
                                            RunServer();
                                        }

                                        connections[i].lastStreamRecieved = false;

                                        switch (read[i][0])
                                        {
                                            case 0: // this is the data of the player
                                                if (latestNetVars == null)
                                                    latestNetVars = new PlayerConnection();

                                                connections[i].toSend = read[i];

                                                PlayerConnection netVars = Serialization.DeSerialize<PlayerConnection>(bytes);
                                                 connections[i].net = netVars;

                                                if (netVars.blockCells != null)
                                                {
                                                    connections[i].net.blockCells.AddRange(netVars.blockCells);

                                                    Console.WriteLine("blocells: {0}", connections[i].net.blockCells.Count);
                                                }

                                                if (netVars.blockCells != null)
                                                {
                                                    Console.WriteLine("hell ye??");
                                                }

                                                //netVars.GetPlatformerNetworkVariables(latestNetVars);

                                                Console.WriteLine("Player {0}: X: {1} Y: {2}", i, netVars.player.position.X, netVars.player.position.Y);

                                                // wait an amount of time and try that - also try and send a signal back to the original sender of the byte - i just realized thats facking stupid lol

                                                latestNetVars = netVars;

                                                SendByte(connections[i].stream, 1);

                                                break;
                                            case 1: // our latest sent stream has been recieved
                                                connections[i].lastStreamRecieved = true;
                                                Console.WriteLine("the clients {0} data was recieved!!", i);
                                                break;
                                            default:
                                                break;
                                        }
                                        read[i] = new byte[read[i].Length];
                                    }
                                }
                                /*if (connections.Count > 1 && timer.GetTime() > updateSpeed && connections[i].lastStreamRecieved) // send vars when time comes
                                {
                                    timer.Reset();
                                }
                                else
                                {
                                    SendDataToClients(i, read[i]);
                                    timer.Run();
                                }*/

                                if (connections[i].net != connections[i].latestNet)
                                {
                                    SendDataToClients(i, connections[i].toSend);
                                    // offset might cause errors here if there is data stacked (not sure excactly how it works)
                                }
                            }
                        }
                    }
                }
            }
        }
        static byte[] GetTimerBytes()
        {
            MemoryStream stream = Serialization.Serialize(timer);
            byte[] data = Serialization.GetBytesFromStream(stream);
            byte[] send = SetMetaData(data, 2);
            return data;
        }
        static byte[] SetMetaData(byte[] array, byte metadata)
        {
            byte[] newArray = new byte[array.Length + 1];
            Array.Copy(array, 0, newArray, 1, array.Length);
            newArray[0] = metadata;
            return newArray;
        }
        static byte[] GetBytesWithoutMetadata(byte[] array)
        {
            byte[] newArray = new byte[array.Length - 1];
            Array.Copy(array, 1, newArray, 0, newArray.Length);
            return newArray;
        }

        static void SendByte(NetworkStream stream, byte send)
        {
            byte[] _send = new byte[] { 1 };
            stream.Write(_send, 0, 1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        static void SendDataToClients(int sender, byte[] data)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                if (i != sender)
                {
                    try
                    {
                        connections[i].stream.Write(data, 0, data.Length); // sending a stream before it has been successfully recieved
                        connections[i].latestNet = connections[i].net;
                    }
                    catch
                    {
                        RunServer();
                    }
                }
                else // send the global timer
                {
                    byte[] send = GetTimerBytes();
                    connections[i].stream.Write(send, 0, send.Length);
                }
            }
        }
        public static void AcceptConnection(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;

            TcpClient client = listener.EndAcceptTcpClient(ar);
            clients.Add(client);
            connections.Add(new Connection(client.GetStream(), clients.Count));
            read.Add(databytes);
            SendConnectionData();
            Console.WriteLine("Player {0} has connected", clients.Count);
            Console.WriteLine("connection {0}: {1}", clients.Count, connections[clients.Count-1].stream);
            Console.WriteLine("Player {0} has connected", clients.Count);
            CheckForIncomingConnection();
        }

        static void SendConnectionData()
        {
            byte[] data = BitConverter.GetBytes(connections.Count);
        }

        static byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
        static IPAddress GetIpAddress()
        {
            //string externalip = new WebClient().DownloadString("http://icanhazip.com");
            return IPAddress.Any;
        }
    }
}
