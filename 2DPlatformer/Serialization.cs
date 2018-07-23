using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

namespace _2DPlatformer
{
    public static class Serialization
    {
        public static MemoryStream Serialize<T>(T objectToWrite)
        {
            MemoryStream stream = new MemoryStream();

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, objectToWrite);
            return stream;
        }

        public static T DeSerialize<T>(MemoryStream stream)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            byte[] bytes = stream.GetBuffer();
            Stream newStream = new MemoryStream(bytes, 0, bytes.Length, true);
            return (T)formatter.Deserialize(newStream);
        }
        public static T DeSerialize<T>(byte[] buffer)
        {
            // Construct a binary formatter
            BinaryFormatter formatter = new BinaryFormatter();
            Stream newStream = new MemoryStream(buffer, 0, buffer.Length, true);

            // Deserialize the stream into object
            return (T)formatter.Deserialize(newStream); // only called when two players are on
        }

        public static byte[] GetBytesFromStream(MemoryStream stream)
        {
            return stream.GetBuffer();
        }
    }
}