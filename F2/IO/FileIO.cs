﻿using System;
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
        public static void WriteBytes(string path, Dictionary<int, byte[]> v)
        {
            var by = new ByteStream();
            by.Push(v.Count);
            foreach (var item in v)
            {
                by.Push(item.Key);
                by.CopyFrom(item.Value, 0, item.Value.Length);
            }
            File.WriteAllBytes(path, by.Bytes);
        }
        public static void WriteBytes(string path, Dictionary<int, Serializable> v)
        {
            var by = new ByteStream();
            by.Push(v.Count);
            foreach (var item in v)
            {
                by.Push(item.Key);
                by.CopyFrom(item.Value.Bytes, 0, item.Value.Bytes.Length);
            }
            File.WriteAllBytes(path, by.Bytes);
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

        public static Dictionary<int, T> ReadBytesDict<T>(byte[] values) where T : IFSerializable
        {
            var se = new Serializable(values);
            var length = se.Read<int>();
            var dict = new Dictionary<int, T>(length);
            while (length > 0)
            {
                var toClass = InstanceT.CreateInstance<T>();
                var key = se.Read<int>();
                toClass.Deserialization(se);
                dict.Add(key, toClass);
                length--;
            }
            return dict;
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
