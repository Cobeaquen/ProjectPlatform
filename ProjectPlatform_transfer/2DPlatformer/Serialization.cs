using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using ProtoBuf;
using System.IO;

namespace ProjectPlatformer
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

        public static void SerializeJson<T>(string fileName, T objectToSerialize)
        {
            StringBuilder newLine = new StringBuilder();
            newLine = newLine.AppendLine();
            StringBuilder fileText = new StringBuilder();
            string json = JsonConvert.SerializeObject(objectToSerialize);

            string[] elements = json.Split(',');

            string text = string.Empty;

            for (int i = 0; i < elements.Length; i++)
            {
                fileText = fileText.Append(elements[i]);
                if (i != elements.Length - 1)
                {
                    fileText = fileText.AppendLine(",");
                }
            }

            fileText = fileText.Insert(1, newLine);
            fileText = fileText.Insert(fileText.Length - 1, newLine);
            text = fileText.ToString();
            File.WriteAllText(fileName, text);
        }
        public static T DeserializeJson<T>(string fileName)
        {
            using (var file = File.OpenText(fileName))
            {
                JsonSerializer serializer = new JsonSerializer();
                return (T)serializer.Deserialize(file, typeof(T));
            }
        }

        public static void SerializeProtobuf<T>(string filePath, T objectToSerialize)
        {
            using (var file = File.Create(filePath))
            {
                Serializer.Serialize(file, objectToSerialize);
            }

        }
        public static T DeserializeProtobuf<T>(string filePath)
        {
            using (var file = File.OpenRead(filePath))
            {
                return Serializer.Deserialize<T>(file);
            }
        }
    }
}