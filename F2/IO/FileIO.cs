using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

// author  (hf) Date：2023/5/22 10:05:25
namespace F
{
    public static class FileIO
    {
        public static void WriteBase64(string path, Dictionary<int, byte[]> v)
        {
            var streamWriter = new StreamWriter(path);
            foreach (var item in v)
            {
                streamWriter.WriteLine($"{item.Key}|{Convert.ToBase64String(item.Value)}");
            }
            streamWriter.Close();
        }

        public static byte[] ReadBase64(string path, int id, string values)
        {
            var s = File.ReadAllLines(path);
            foreach (var item in s)
            {
                var bytesValue = item.Split('|');
                if (bytesValue[0] == id.ToString())
                {
                    return Convert.FromBase64String(bytesValue[1]);
                }
            }
            throw new Exception($"ReadBase64 not found Id:{id}");
        }

        public static T ReadBase64<T>(string path, int id, string values) where T : IFSerializable
        {
            var s = File.ReadAllLines(path);
            var se = new Serializable();
            foreach (var item in s)
            {
                var bytesValue = item.Split('|');
                if (bytesValue[0] == id.ToString())
                {
                    var toClass = InstanceT.CreateInstance<T>();
                    toClass.Deserialization(se);
                    return toClass;
                }
            }
            throw new Exception($"ReadBase64 not found Id:{id}");
        }

        public static Dictionary<int, T> ReadBase64Dict<T>(string[] values) where T : IFSerializable
        {
            var dict = new Dictionary<int, T>(values.Length);
            var se = new Serializable();
            foreach (var item in values)
            {
                var bytesValue = item.Split('|');
                var toClass = InstanceT.CreateInstance<T>();
                se.Bytes = Convert.FromBase64String(bytesValue[1]);
                se.Position = 0;
                toClass.Deserialization(se);
                dict.Add(Convert.ToInt16(bytesValue[0]), toClass);
            }
            return dict;
        }

    }
}
