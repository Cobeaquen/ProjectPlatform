using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using _2DPlatformer.Networking;

namespace _2DPlatformer
{
    public static class NetworkManager
    {
        public static List<PlatformerNetworking> connectedClient;

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
    }
}
