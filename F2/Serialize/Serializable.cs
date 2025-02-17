﻿using System;
using System.Collections.Generic;
using System.Reflection;

// author  (hf) Date：2023/5/6 14:36:45
namespace F
{
    public class Serializable : IDisposable
    {
        public ByteStream ByteBuff;

        public Serializable(byte[] bytes)
        {
            ByteBuff = new ByteStream(bytes);
        }

        public Serializable(Span<byte> bytes)
        {
            ByteBuff = new ByteStream(bytes);
            //bytes.ToArray();
        }

        public Serializable()
        {
        }

        public bool IsEnd => ByteBuff.IsEnd;

        public static void Deserialization<T>(ref T v) where T : IFSerializable
        {
            using (var serializable = new Serializable())
            {
                serializable.ReadSerializable(ref v);
            }
        }

        #region read push obj

        public void PushObj(object obj)
        {
            switch (obj)
            {
                case bool v:
                    Push(v); break;
                case int v:
                    Push(v); break;
                case uint v:
                    Push(v); break;
                case float v:
                    Push(v); break;
                case Enum v:
                    Push(v);
                    break;
                case string v:
                    Push(v); break;
                case ulong v:
                    Push(v); break;
                case double v:
                    Push(v); break;
                case byte v:
                    Push(v); break;
                case short v:
                    Push(v); break;
                case ushort v:
                    Push(v); break;
                case long v:
                    Push(v); break;
                case char v:
                    Push(v); break;
                case IFSerializable v:
                    PushSerializable(v);
                    break;
                default:
                    if (obj.GetType().IsArray)
                    {
                        PushAarray(obj);
                    }
                    else
                    {
                        var fildes = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                        foreach (var item in fildes)
                        {
                            var v = item.GetValue(obj);
                            //null 值直接不写入
                            if (v != null) PushObj(v);
                        }
                    }

                    break;
            }
        }

        private void Push(Enum v)
        {
            var underlyingType = Enum.GetUnderlyingType(v.GetType());
            switch (Type.GetTypeCode(underlyingType))
            {
                case TypeCode.SByte:
                case TypeCode.Byte: Push(Convert.ToByte(v)); break;
                case TypeCode.Int16:
                case TypeCode.UInt16: Push(Convert.ToInt32(v)); break;
                case TypeCode.Int32:
                case TypeCode.UInt32: Push(Convert.ToUInt32(v)); break;
                case TypeCode.Int64:
                case TypeCode.UInt64: Push(Convert.ToUInt64(v)); break;
            }
        }

        public void PushAarray(object obj)
        {
            switch (obj)
            {
                case int[] v:
                    Push(v); break;
                case uint[] v:
                    Push(v); break;
                case double[] v:
                    Push(v); break;
                case float[] v:
                    Push(v); break;
                case string[] v:
                    Push(v); break;
                case byte[] v:
                    Push(v); break;
                case sbyte[] v:
                    Push(v); break;
                case short[] v:
                    Push(v); break;
                case ushort[] v:
                    Push(v); break;
                case ulong[] v:
                    Push(v); break;
                case long[] v:
                    Push(v); break;
                case IFSerializable[] v:
                    PushSerializable(v); break;
                default:
                    //2维数组
                    if (obj is Array[] v2)
                    {
                        ByteBuff.PushLength((v2?.Length).GetValueOrDefault());
                        foreach (var item in v2) PushAarray(item);
                    }

                    break;
            }
        }

        /// <summary>
        ///     通过类（反射）读取
        /// </summary>
        /// <param name="v"></param>
        public void ReadObjs(object v)
        {
        }

        private void Read(object obj)
        {
            switch (obj)
            {
                case int v:
                    Read(ref v); break;
                case uint v:
                    Read(ref v); break;
                case float v:
                    Read(ref v); break;
                case string v:
                    Read(ref v); break;
                case ulong v:
                    Read(ref v); break;
                case double v:
                    Read(ref v); break;
                case byte v:
                    Read(ref v); break;
                case short v:
                    Read(ref v); break;
                case ushort v:
                    Read(ref v); break;
                case char v:
                    Read(ref v); break;
                case int[] v:
                    Read(ref v); break;
                //case Dictionary v:
                case int[][] v:
                    break;
            }
        }

        private void SetFiledValue(ByteStream stream, object v)
        {
            var fildes = v.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var item in fildes)
            {
                var type = item.FieldType;
                if (type == typeof(int))
                {
                    item.SetValue(v, stream.Read<int>());
                }
                else if (type.IsEnum)
                {
                    var t = Enum.GetUnderlyingType(type);
                    //var d = readV.Read<uint>();
                    object eV = default;
                    switch (Type.GetTypeCode(t))
                    {
                        case TypeCode.SByte:
                        case TypeCode.Byte: eV = Enum.ToObject(type.GetElementType(), stream.Read<byte>()); break;
                        case TypeCode.Int16:
                        case TypeCode.UInt16: eV = Enum.ToObject(type, stream.Read<int>()); break;
                        case TypeCode.Int32:
                        case TypeCode.UInt32: eV = Enum.ToObject(type, stream.Read<uint>()); break;
                        case TypeCode.Int64:
                        case TypeCode.UInt64: eV = Enum.ToObject(type, stream.Read<ulong>()); break;
                    }

                    item.SetValue(v, eV);
                }
                else if (type == typeof(string))
                {
                    item.SetValue(v, stream.Read());
                }
                else if (type == typeof(int[]))
                {
                    item.SetValue(v, stream.ReadArray<int>());
                }
                else if (type == typeof(float[]))
                {
                    item.SetValue(v, stream.ReadArray<float>());
                }
                else if (type == typeof(string[]))
                {
                    item.SetValue(v, stream.ReadArray());
                }
                else if (type.IsValueType)
                {
                    //创建结构体
                    var d = Activator.CreateInstance(type);
                    SetFiledValue(stream, d);
                    item.SetValue(v, d);
                }
            }
        }

        #endregion


        #region push

        public void PushLength(long v)
        {
            ByteBuff.PushLength(v);
        }

        public int ReadLength()
        {
            return ByteBuff.ReadLength();
        }

        public void Push<T>(T v) where T : unmanaged
        {
            //var span = GetSpan();
            //span.Push(v);
            //Position += span.Position - Position;
            //Bytes = span;
            ByteBuff.Push(v);
        }

        public int BeginPush(string k)
        {
            ByteBuff.Push(k);
            //占位，用来写入数据长度
            ByteBuff.Push(0);
            return ByteBuff.Position;
        }

        public int BeginPush(int k)
        {
            //占位，用来写入数据长度
            ByteBuff.Push(0);
            return ByteBuff.Position;
        }

        public void EndPush(int pos)
        {
            var newpos = ByteBuff.Position;
            //获取占位符数据所在位置
            ByteBuff.SetPosition((uint)pos - 4);
            //写入正确的数据长度
            ByteBuff.Push(newpos - pos);
            ByteBuff.SetPosition((uint)newpos);
        }

        public void Push<T>(string k, T v) where T : unmanaged
        {
            var pos = BeginPush(k);
            ByteBuff.Push(v);
            EndPush(pos);
        }

        public void Push(string v)
        {
            ByteBuff.Push(v);
        }

        public void Push<T>(T[] v) where T : unmanaged
        {
            ByteBuff.Push(v);
        }

        public void Push(string[] v)
        {
            ByteBuff.Push(v);
        }

        public void Push<T>(T[][] v) where T : unmanaged
        {
            ByteBuff.Push(v);
        }

        public void Push(string[][] v)
        {
            ByteBuff.Push(v);
        }

        //压缩字节
        public void PushInt(int[][] v)
        {
            ByteBuff.PushInt(v);
        }

        public void PushInt(int[] v)
        {
            ByteBuff.PushInt(v);
        }

        public void PushInt(int v)
        {
            ByteBuff.PushInt(v);
        }

        public void PushUInt(uint v)
        {
            ByteBuff.PushUInt(v);
        }

        public void PushUInt(uint[][] v)
        {
            ByteBuff.PushUInt(v);
        }

        public void PushUInt(uint[] v)
        {
            ByteBuff.PushUInt(v);
        }

        #endregion

        #region Dictionary

        public void Push<T, T1>(Dictionary<T, T1> v) where T : unmanaged where T1 : unmanaged
        {
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    Push(item.Key);
                    Push(item.Value);
                }
        }

        public void Push<T, T1>(Dictionary<T, T1[]> v) where T : unmanaged where T1 : unmanaged
        {
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    Push(item.Key);
                    Push(item.Value);
                }
        }

        public void Push<T, T1>(Dictionary<T, T1[][]> v) where T : unmanaged where T1 : unmanaged
        {
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    Push(item.Key);
                    Push(item.Value);
                }
        }

        public void Push<T>(Dictionary<T, string> v) where T : unmanaged
        {
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    Push(item.Key);
                    Push(item.Value);
                }
        }

        public void Push<T>(Dictionary<T, string[]> v) where T : unmanaged
        {
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    Push(item.Key);
                    Push(item.Value);
                }
        }

        public void Push<T>(Dictionary<T, string[][]> v) where T : unmanaged
        {
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    Push(item.Key);
                    Push(item.Value);
                }
        }

        public void Push<T1>(Dictionary<string, T1> v) where T1 : unmanaged
        {
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    Push(item.Key);
                    Push(item.Value);
                }
        }

        public void Push(Dictionary<string, string> v)
        {
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    Push(item.Key);
                    Push(item.Value);
                }
        }

        public void Push(Dictionary<string, string[]> v)
        {
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    Push(item.Key);
                    Push(item.Value);
                }
        }

        public void Push<T>(Dictionary<string, T[]> v) where T : unmanaged
        {
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    Push(item.Key);
                    Push(item.Value);
                }
        }

        public void Push<T>(Dictionary<string, T[][]> v) where T : unmanaged
        {
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    Push(item.Key);
                    Push(item.Value);
                }
        }

        public void Push(Dictionary<string, string[][]> v)
        {
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    Push(item.Key);
                    Push(item.Value);
                }
        }

        public void PushSerializable<T>(T[] v) where T : IFSerializable
        {
            var length = (v?.Length).GetValueOrDefault();
            ByteBuff.PushLength(length);
            if (length > 0)
                foreach (var item in v)
                    PushSerializable(item);
        }

        public void PushSerializable<T1>(T1[][] v) where T1 : IFSerializable
        {
            var length = (v?.Length).GetValueOrDefault();
            ByteBuff.PushLength(length);
            if (length > 0)
                PushSerializable(v);
        }

        public void PushSerializable<T, T1>(Dictionary<T, T1> v) where T : unmanaged where T1 : IFSerializable
        {
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    Push(item.Key);
                    PushSerializable(item.Value);
                }
        }

        public void PushSerializable<T1>(Dictionary<string, T1> v) where T1 : IFSerializable
        {
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    Push(item.Key);
                    PushSerializable(item.Value);
                }
        }

        public void PushSerializable<T, T1>(Dictionary<T, T1[]> v) where T : unmanaged where T1 : IFSerializable
        {
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    Push(item.Key);
                    PushSerializable(item.Value);
                }
        }

        public void PushSerializable<T, T1>(Dictionary<T, IFSerializable[][]> v)
            where T : unmanaged where T1 : IFSerializable
        {
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    Push(item.Key);
                    PushSerializable(item.Value);
                }
        }

        public void PushSerializable<T1>(Dictionary<string, T1[]> v) where T1 : IFSerializable
        {
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    Push(item.Key);
                    PushSerializable(item.Value);
                }
        }

        public void PushSerializable<T>(string key, T v) where T : IFSerializable
        {
            if (v != null)
            {
                var keyP = BeginPush(key);
                var pos = BeginPush(0);
                v.Serialization(this);
                EndPush(keyP);
                EndPush(pos);
            }
            else
            {
                Push(string.Empty);
                Push(0);
            }
        }


        public void PushSerializable<T>(T v) where T : IFSerializable
        {
            if (v != null) v.Serialization(this);
        }


        public void ReadSerializable<T>(ref T v) where T : IFSerializable
        {
            if (v != null) v.Deserialization(this);
        }


        public void ReadSerializable<T>(ref T[] v) where T : IFSerializable
        {
            var length = ByteBuff.ReadLength();
            if (length > 0) v = new T[length];
            for (var i = 0; i < length; i++)
            {
                var se = InstanceT.CreateInstance<T>();
                ReadSerializable(ref se);
                v[i] = se;
            }
        }

        public void ReadSerializable<T, T1>(ref Dictionary<T, T1> v) where T : unmanaged where T1 : IFSerializable
        {
            var count = ByteBuff.ReadLength();
            v = new Dictionary<T, T1>();
            while (count > 0)
            {
                var se = InstanceT.CreateInstance<T1>();
                var key = Read<T>();
                //se.Deserialization(this);
                ReadSerializable(ref se);
                v.Add(key, se);
                count--;
            }
        }

        public void ReadSerializable<T>(ref Dictionary<string, T> v) where T : IFSerializable
        {
            var count = ByteBuff.ReadLength();
            v = new Dictionary<string, T>();
            while (count > 0)
            {
                var se = InstanceT.CreateInstance<T>();
                var key = Read();
                ReadSerializable(ref se);
                v.Add(key, se);
                count--;
            }
        }

        public void ReadSerializable<T, T1>(ref Dictionary<T, T1[]> v) where T : unmanaged where T1 : IFSerializable
        {
            var count = ByteBuff.ReadLength();
            v = new Dictionary<T, T1[]>();
            while (count > 0)
            {
                var key = Read<T>();
                var valuesCount = ByteBuff.ReadLength();
                var c = new T1[valuesCount];
                for (var i = 0; i < valuesCount; i++)
                {
                    var se = InstanceT.CreateInstance<T1>();
                    ReadSerializable(ref se);
                    c[i] = se;
                }

                v.Add(key, c);
                count--;
            }
        }

        public void ReadSerializable<T1>(ref Dictionary<string, T1[]> v) where T1 : IFSerializable
        {
            var count = ByteBuff.ReadLength();
            v = new Dictionary<string, T1[]>();
            while (count > 0)
            {
                var key = Read();
                var valuesCount = ByteBuff.ReadLength();
                var c = new T1[valuesCount];
                for (var i = 0; i < valuesCount; i++)
                {
                    var se = InstanceT.CreateInstance<T1>();
                    ReadSerializable(ref se);
                    c[i] = se;
                }

                v.Add(key, c);
                count--;
            }
        }

        public T Read<T>() where T : unmanaged
        {
            return ByteBuff.Read<T>();
        }

        public string Read()
        {
            return ByteBuff.Read();
        }

        /// <summary>
        ///     读取压缩字节
        /// </summary>
        /// <returns></returns>
        public int ReadInt(ref int v)
        {
            return v = ByteBuff.ReadInt();
        }

        public uint ReadUint(ref uint v)
        {
            return v = ByteBuff.ReadUint();
        }

        public uint[] ReadUint(ref uint[] v)
        {
            return v = ByteBuff.ReadUintArray();
        }

        public uint[][] ReadUint(ref uint[][] v)
        {
            return v = ByteBuff.ReadUintArray2();
        }

        public int[] ReadInt(ref int[] v)
        {
            return v = ByteBuff.ReadIntArray();
        }

        public int[][] ReadInt(ref int[][] v)
        {
            return v = ByteBuff.ReadIntArray2();
        }

        public T[] ReadArray<T>() where T : unmanaged
        {
            return ByteBuff.ReadArray<T>();
        }

        public string[] ReadArray()
        {
            return ByteBuff.ReadArray();
        }

        public void Read<T>(ref T v) where T : unmanaged
        {
            v = Read<T>();
        }

        public void Read(ref string v)
        {
            v = Read();
        }

        public void Read<T>(ref T[] v) where T : unmanaged
        {
            v = ReadArray<T>();
        }

        public void Read(ref string[] v)
        {
            v = ReadArray();
        }

        public T[][] ReadArray2<T>() where T : unmanaged
        {
            return ByteBuff.ReadArray2<T>();
        }

        public string[][] ReadArray2()
        {
            return ByteBuff.ReadArray2();
        }

        public void Read<T>(ref T[][] v) where T : unmanaged
        {
            v = ReadArray2<T>();
        }

        public void Read(ref string[][] v)
        {
            v = ReadArray2();
        }

        public void Dispose()
        {
            ByteBuff.Dispose();
        }

        public void Read<T, T1>(ref Dictionary<T, T1> v) where T : unmanaged where T1 : unmanaged
        {
            var count = ByteBuff.ReadLength();
            v = new Dictionary<T, T1>(count);
            for (var i = 0; i < count; i++) v.Add(Read<T>(), Read<T1>());
        }

        public void Read<T, T1>(ref Dictionary<T, T1[]> v) where T : unmanaged where T1 : unmanaged
        {
            var count = ByteBuff.ReadLength();
            v = new Dictionary<T, T1[]>(count);
            for (var i = 0; i < count; i++) v.Add(Read<T>(), ReadArray<T1>());
        }

        public void Read<T, T1>(ref Dictionary<T, T1[][]> v) where T : unmanaged where T1 : unmanaged
        {
        }

        public void Read<T>(ref Dictionary<T, string> v) where T : unmanaged
        {
            var count = ByteBuff.ReadLength();
            v = new Dictionary<T, string>(count);
            for (var i = 0; i < count; i++) v.Add(Read<T>(), Read());
        }

        public void Read<T>(ref Dictionary<T, string[]> v) where T : unmanaged
        {
            var count = ByteBuff.ReadLength();
            v = new Dictionary<T, string[]>(count);
            for (var i = 0; i < count; i++) v.Add(Read<T>(), ReadArray());
        }

        public void Read<T>(ref Dictionary<T, string[][]> v) where T : unmanaged
        {
            var count = ByteBuff.ReadLength();
            v = new Dictionary<T, string[][]>(count);
            for (var i = 0; i < count; i++) v.Add(Read<T>(), ReadArray2());
        }

        public void Read(ref Dictionary<string, string> v)
        {
            var count = ByteBuff.ReadLength();
            v = new Dictionary<string, string>(count);
            for (var i = 0; i < count; i++) v.Add(Read(), Read());
        }

        public void Read(ref Dictionary<string, string[]> v)
        {
            var count = ByteBuff.ReadLength();
            v = new Dictionary<string, string[]>(count);
            for (var i = 0; i < count; i++) v.Add(Read(), ReadArray());
        }

        public void Read(ref Dictionary<string, string[][]> v)
        {
            var count = ByteBuff.ReadLength();
            v = new Dictionary<string, string[][]>(count);
            for (var i = 0; i < count; i++) v.Add(Read(), ReadArray2());
        }

        public void Read<T>(ref Dictionary<string, T[][]> v) where T : unmanaged
        {
            var count = ByteBuff.ReadLength();
            v = new Dictionary<string, T[][]>(count);
            for (var i = 0; i < count; i++) v.Add(Read(), ReadArray2<T>());
        }

        public void Read<T1>(ref Dictionary<string, T1> v) where T1 : unmanaged
        {
            var count = ByteBuff.ReadLength();
            v = new Dictionary<string, T1>(count);
            for (var i = 0; i < count; i++) v.Add(Read(), Read<T1>());
        }

        #endregion

        #region list

        public void PushSerializable<T>(List<T> v) where T : IFSerializable
        {
            var valueOrDefault = (v?.Count).GetValueOrDefault();
            Push(valueOrDefault);
            if (valueOrDefault > 0)
                foreach (var v2 in v)
                    PushSerializable(v2);
        }

        public List<T> ReadSerializable<T>(ref List<T> v) where T : IFSerializable, new()
        {
            var num = Read<int>();
            if (num > 0) v = new List<T>(num);
            for (var i = 0; i < num; i++)
            {
                var val = new T();
                ReadSerializable(ref val);
                v.Add(val);
            }

            return v;
        }

        #endregion
    }
}