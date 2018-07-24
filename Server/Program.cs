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

namespace Server
{
    class Program
    {
        static List<Connection> connections = new List<Connection>();

        static IPAddress ip;
        static Int32 port = 9009;
        static List<byte[]> read = new List<byte[]>();
        static byte[] databytes = new byte[1500]; // look for resizing later
        static TcpListener server;
        static List<TcpClient> clients = new List<TcpClient>();
        static PlatformerNetworking latestNetVars;

        static int timeToMoveText = 10;

        static int pos;

        static float updateSpeed = 10000000f;
        static Timer timer = new Timer(); // be careful with this one (since it might not be necessary and it is 64-bit)

        static bool timeToSendData = false;
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

            timeToSendData = true;

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
                    if (timer.GetTime() > 0)
                    {
                        for (int i = 0; i < connections.Count; i++)
                        {
                            if (connections[i].stream != null)
                            {
                                if (connections[i].stream.DataAvailable) // listening for data
                                {
                                    int r;
                                    if ((r = connections[i].stream.Read(read[i], 0, read[i].Length)) > 0)
                                    {
                                        byte[] bytes = new byte[read[i].Length - 1];
                                        Array.Copy(read[i], 1, bytes, 0, bytes.Length);

                                        switch (read[i][0])
                                        {
                                            case 0: // this is the data of the player
                                                if (latestNetVars == null)
                                                    latestNetVars = new PlatformerNetworking();

                                                PlatformerNetworking netVars = Serialization.DeSerialize<PlatformerNetworking>(bytes);
                                                //netVars.GetPlatformerNetworkVariables(latestNetVars);
                                                connections[i].player = netVars.player;
                                                Console.WriteLine("Player {0}: X: {1} Y: {2}", i, netVars.player.position.X, netVars.player.position.Y);

                                                // wait an amount of time and try that - also try and send a signal back to the original sender of the byte - i just realized thats facking stupid lol

                                                timer.Reset(); // remove and fix this is unnecessary pls

                                                SendDataToClients(i, read[i]);
                                                latestNetVars = netVars;
                                                timeToSendData = false;

                                                break;
                                            case 1: // our latest sent stream has been recieved
                                                timeToSendData = true;
                                                Console.WriteLine("the clients {0} data was recieved!!", i);
                                                break;
                                            default:
                                                break;
                                        }
                                        read[i] = new byte[read[i].Length];
                                    }
                                }
                            }
                            connections[i].stream.Flush(); // probably wanna remove this later yoo
                        }
                    }
                    else
                    {
                        timer.Run();
                    }
                }
            }
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
                    }
                    catch
                    {
                        RunServer();
                    }
                }
                else
                {
                    SendByte(connections[i].stream, 1);
                     // offset might cause errors here if there is data stacked (not sure excactly how it works)
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
