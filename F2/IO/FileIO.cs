using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

// author  (hf) Date：2023/5/22 10:05:25
namespace F
{
    public static class FileIO
    {
        public static string GetFileSize(double length)
        {
            var sizeConverters = new string[] { "B", "KB", "MB", "GB" };
            var index = 0;
            while (length >= 1024f && index < sizeConverters.Length - 1)
            {
                index++;
                length /= 1024f;
            }
            return length.ToString() + sizeConverters[index];
        }
        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static string GetFileSize(string path)
        {
            return GetFileSize(File.OpenRead(path).Length);
        }

        /// <summary>
        /// 指定文件写入流，文件不存在，创建文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="byteStream"></param>
        public static void WriteBytes(string path, ByteStream byteStream)
        {
            CreateFile(path);
            File.WriteAllBytes(path, byteStream.Bytes);
        }
        public static void CreateFile(string path)
        {
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }
        }
        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Create(path).Dispose();
            }
        }
        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        public static void DeleteDirectory(string path, bool recursive = true)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive);
            }
        }


        public static void WriteBase64(string path, Dictionary<int, byte[]> v)
        {
            var streamWriter = new StreamWriter(path);
            foreach (var item in v)
            {
                streamWriter.WriteLine($"{item.Key}|{Convert.ToBase64String(item.Value)}");
            }
            streamWriter.Close();
        }
        public static void WriteBytesDict<Tkey>(string path, Dictionary<Tkey, byte[]> v) where Tkey : unmanaged
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
        public static void WriteBytesDict(string path, Dictionary<string, byte[]> v)
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

        public static void WriteBytesDict<Tkey>(string path, Dictionary<Tkey, IFSerializable> v, bool isWriteClassName = false) where Tkey : unmanaged
        {
            var s = new Serializable();
            s.Push(v.Count);
            foreach (var item in v)
            {
                s.Push(item.Key);
                if (isWriteClassName)
                {
                    s.Push(item.Value.GetType().FullName);
                }
                item.Value.Serialization(s);
            }
            File.WriteAllBytes(path, s.Bytes);
        }
        /// <summary>
        /// 写入序列化，支持多个不同类写入
        /// </summary>
        /// <param name="path"></param>
        /// <param name="v"></param>
        /// <param name="isWriteClassName">是否写入类名</param>
        public static void WriteBytesDict(string path, Dictionary<string, IFSerializable> v, bool isWriteClassName = false)
        {
            var s = new Serializable();
            s.Push(v.Count);
            foreach (var item in v)
            {
                s.Push(item.Key);
                if (isWriteClassName)
                {
                    s.Push(item.Value.GetType().FullName);
                }
                item.Value.Serialization(s);
            }
            File.WriteAllBytes(path, s.Bytes);
        }

        public static byte[] ReadBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        /// <summary>
        /// 读取序列化，支持多个不同类
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="values"></param>
        /// <param name="isReadClassName">是否读取类名</param>
        /// <returns></returns>
        public static Dictionary<string, TValue> ReadBytesDict<TValue>(byte[] values, bool isReadClassName = false) where TValue : IFSerializable
        {
            var se = new Serializable(values);
            var length = se.Read<int>();
            var dict = new Dictionary<string, TValue>(length);
            while (length > 0)
            {
                var key = se.Read();
                var toClass = isReadClassName ? InstanceT.CreateInstance<IFSerializable>(se.Read()) : InstanceT.CreateInstance<TValue>();
                toClass.Deserialization(se);
                dict.Add(key, (TValue)toClass);
                length--;
            }
            return dict;
        }
        public static Dictionary<Tkey, TValue> ReadBytesDict<Tkey, TValue>(byte[] values, bool isReadClassName = false) where Tkey : unmanaged where TValue : IFSerializable
        {
            var se = new Serializable(values);
            var length = se.Read<int>();
            var dict = new Dictionary<Tkey, TValue>(length);
            while (length > 0)
            {
                var key = se.Read<Tkey>();
                var toClass = isReadClassName ? InstanceT.CreateInstance<IFSerializable>(se.Read()) : InstanceT.CreateInstance<TValue>();
                toClass.Deserialization(se);
                dict.Add(key, (TValue)toClass);
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

    }
}
