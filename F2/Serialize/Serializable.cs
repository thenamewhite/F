using System;
using System.Collections.Generic;
using System.Drawing;

// author  (hf) Date：2023/5/6 14:36:45
namespace F
{
    public class Serializable : IDisposable
    {
        //public byte[] Bytes = Array.Empty<byte>();

        public ByteStream Bytes = new ByteStream();
        public int Position;
        //public ByteStream GetSpan()
        //{
        //    return new ByteStream(Bytes) { Position = Position };
        //}
        public Serializable(byte[] bytes)
        {
            Bytes = new ByteStream(bytes);
        }
        public Serializable(Span<byte> bytes)
        {
            Bytes = new ByteStream(bytes);
            //bytes.ToArray();
        }
        public Serializable()
        {
        }
        public bool IsEnd
        {
            get => Bytes.IsEnd;
        }
        #region read push obj
        public void PushObj(object obj)
        {
            switch (obj)
            {
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
                        var fildes = obj.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        foreach (var item in fildes)
                        {
                            var v = item.GetValue(obj);
                            //null 值直接不写入
                            if (v != null)
                            {
                                PushObj(v);
                            }
                        }
                    }
                    break;
            }
        }
        private void Push(Enum v)
        {
            Type underlyingType = Enum.GetUnderlyingType(v.GetType());
            switch (Type.GetTypeCode(underlyingType))
            {
                case TypeCode.SByte:
                case TypeCode.Byte: Push<byte>(Convert.ToByte(v)); break;
                case TypeCode.Int16:
                case TypeCode.UInt16: Push<int>(Convert.ToInt32(v)); break;
                case TypeCode.Int32:
                case TypeCode.UInt32: Push<UInt32>(Convert.ToUInt32(v)); break;
                case TypeCode.Int64:
                case TypeCode.UInt64: Push<ulong>(Convert.ToUInt64(v)); break;
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
                    Push(v);
                    break;
                case sbyte[] v:
                    Push(v); break;
                case short[] v:
                    Push(v); break;
                case ushort[] v:
                    Push(v); break;
                case ulong[] v:
                    Push(v); break;
                case IFSerializable[] v:
                    PushSerializable(v); break;
                default:
                    //2维数组
                    if (obj is Array[] v2)
                    {
                        Bytes.PushLength((v2?.Length).GetValueOrDefault());
                        foreach (var item in v2)
                        {
                            PushAarray(item);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// 通过类（反射）读取
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
                default:
                    //ReadObjs()
                    //Read(obj);
                    break;
            }
        }
        private void SetFiledValue(ByteStream stream, object v)
        {
            var fildes = v.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var item in fildes)
            {
                var type = item.FieldType;
                if (type == typeof(int))
                {
                    item.SetValue(v, stream.Read<int>());
                }
                else if (type.IsEnum)
                {
                    Type t = Enum.GetUnderlyingType(type);
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
        public void Push<T>(T v) where T : unmanaged
        {
            //var span = GetSpan();
            //span.Push(v);
            //Position += span.Position - Position;
            //Bytes = span;
            Bytes.Push(v);
        }

        public void Push(string v)
        {
            Bytes.Push(v);
        }

        public void Push<T>(T[] v) where T : unmanaged
        {
            Bytes.Push(v);
        }

        public void Push(string[] v)
        {
            Bytes.Push(v);
        }
        public void Push<T>(T[][] v) where T : unmanaged
        {
            Bytes.Push(v);
        }
        public void Push(string[][] v)
        {
            Bytes.Push(v);
        }
        #endregion
        #region  Dictionary
        public void Push<T, T1>(Dictionary<T, T1> v) where T : unmanaged where T1 : unmanaged
        {
            var count = (v?.Count).GetValueOrDefault();
            Push(count);
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
            Push(count);
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
            Push(count);
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
            Push(count);
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
            Push(count);
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
            Push(count);
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
            Push(count);
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
            Push(count);
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
            Push(count);
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
            Push(count);
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
            Push(count);
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
            Push(count);
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
            Push(length);
            if (length > 0)
                foreach (var item in v)
                {
                    PushSerializable(item);
                }
        }
        public void PushSerializable<T1>(T1[][] v) where T1 : IFSerializable
        {
            var length = (v?.Length).GetValueOrDefault();
            Push(length);
            if (length > 0)
                PushSerializable(v);
        }

        public void PushSerializable<T, T1>(Dictionary<T, T1> v) where T : unmanaged where T1 : IFSerializable
        {
            var count = (v?.Count).GetValueOrDefault();
            Push(count);
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
            Push(count);
            if (count > 0)
                foreach (var item in v)
                {
                    Push(item.Key);
                    PushSerializable(item.Value);
                }
        }

        public void PushSerializable<T, T1>(Dictionary<T, IFSerializable[][]> v) where T : unmanaged where T1 : IFSerializable
        {
            var count = (v?.Count).GetValueOrDefault();
            Push(count);
            if (count > 0)
            {
                foreach (var item in v)
                {
                    Push(item.Key);
                    PushSerializable(item.Value);
                }
            }
        }

        public void PushSerializable<T1>(Dictionary<string, T1[]> v) where T1 : IFSerializable
        {
            var count = (v?.Count).GetValueOrDefault();
            Push(count);
            if (count > 0)
            {
                foreach (var item in v)
                {
                    Push(item.Key);
                    PushSerializable(item.Value);
                }
            }
        }

        public void PushSerializable<T>(T v) where T : IFSerializable
        {
            if (v != null)
            {
                v.Serialization(this);
            }
        }


        public void ReadSerializable<T>(ref T v) where T : IFSerializable
        {
            if (v != null)
            {
                v.Deserialization(this);
            }
        }


        public void ReadSerializable<T>(ref T[] v) where T : IFSerializable
        {
            var length = Read<int>();
            if (length > 0) v = new T[length];
            for (int i = 0; i < length; i++)
            {
                var toClass = InstanceT.CreateInstance<T>();
                toClass.Deserialization(this);
                v[i] = toClass;
            }
        }

        public void ReadSerializable<T, T1>(ref Dictionary<T, T1> v) where T : unmanaged where T1 : IFSerializable
        {
            var count = Read<int>();
            v = new Dictionary<T, T1>();
            while (count > 0)
            {
                var se = InstanceT.CreateInstance<T1>();
                var key = Read<T>();
                se.Deserialization(this);
                v.Add(key, se);
                count--;
            }
        }
        public void ReadSerializable<T>(ref Dictionary<string, T> v) where T : IFSerializable
        {
            var count = Read<int>();
            v = new Dictionary<string, T>();
            while (count > 0)
            {
                var se = InstanceT.CreateInstance<T>();
                var key = Read();
                se.Deserialization(this);
                v.Add(key, se);
                count--;
            }
        }
        public void ReadSerializable<T, T1>(ref Dictionary<T, T1[]> v) where T : unmanaged where T1 : IFSerializable
        {
            var count = Read<int>();
            v = new Dictionary<T, T1[]>();
            while (count > 0)
            {
                var key = Read<T>();
                var valuesCount = Read<int>();
                var c = new T1[valuesCount];
                for (int i = 0; i < valuesCount; i++)
                {
                    var se = InstanceT.CreateInstance<T1>();
                    se.Deserialization(this);
                    c[i] = se;
                }
                v.Add(key, c);
                count--;
            }
        }
        public void ReadSerializable<T1>(ref Dictionary<string, T1[]> v) where T1 : IFSerializable
        {
            var count = Read<int>();
            v = new Dictionary<string, T1[]>();
            while (count > 0)
            {
                var key = Read();
                var valuesCount = Read<int>();
                var c = new T1[valuesCount];
                for (int i = 0; i < valuesCount; i++)
                {
                    var se = InstanceT.CreateInstance<T1>();
                    se.Deserialization(this);
                    c[i] = se;
                }
                v.Add(key, c);
                count--;
            }
        }

        public T Read<T>() where T : unmanaged
        {
            return Bytes.Read<T>();
        }

        public string Read()
        {
            return Bytes.Read();
        }

        public T[] ReadArray<T>() where T : unmanaged
        {

            return Bytes.ReadArray<T>();
        }
        public string[] ReadArray()
        {

            return Bytes.ReadArray();
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
            return Bytes.ReadArray2<T>();
        }
        public string[][] ReadArray2()
        {
            return Bytes.ReadArray2();
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
            //Bytes = null;
        }

        public void Read<T, T1>(ref Dictionary<T, T1> v) where T : unmanaged where T1 : unmanaged
        {
            var count = Read<int>();
            v = new Dictionary<T, T1>(count);
            for (int i = 0; i < count; i++)
            {
                v.Add(Read<T>(), Read<T1>());
            }
        }
        public void Read<T, T1>(ref Dictionary<T, T1[]> v) where T : unmanaged where T1 : unmanaged
        {
            var count = Read<int>();
            v = new Dictionary<T, T1[]>(count);
            for (int i = 0; i < count; i++)
            {
                v.Add(Read<T>(), ReadArray<T1>());
            }
        }
        public void Read<T, T1>(ref Dictionary<T, T1[][]> v) where T : unmanaged where T1 : unmanaged
        {

        }
        public void Read<T, T1>(ref Dictionary<T, string> v) where T : unmanaged
        {
            var count = Read<int>();
            v = new Dictionary<T, string>(count);
            for (int i = 0; i < count; i++)
            {
                v.Add(Read<T>(), Read());
            }
        }
        public void Read<T>(ref Dictionary<T, string[]> v) where T : unmanaged
        {
            var count = Read<int>();
            v = new Dictionary<T, string[]>(count);
            for (int i = 0; i < count; i++)
            {
                v.Add(Read<T>(), ReadArray());
            }
        }
        public void Read<T>(ref Dictionary<T, string[][]> v) where T : unmanaged
        {
            var count = Read<int>();
            v = new Dictionary<T, string[][]>(count);
            for (int i = 0; i < count; i++)
            {
                v.Add(Read<T>(), ReadArray2());
            }
        }
        public void Read(ref Dictionary<string, string> v)
        {
            var count = Read<int>();
            v = new Dictionary<string, string>(count);
            for (int i = 0; i < count; i++)
            {
                v.Add(Read(), Read());
            }
        }
        public void Read(ref Dictionary<string, string[]> v)
        {
            var count = Read<int>();
            v = new Dictionary<string, string[]>(count);
            for (int i = 0; i < count; i++)
            {
                v.Add(Read(), ReadArray());
            }
        }
        public void Read(ref Dictionary<string, string[][]> v)
        {
            var count = Read<int>();
            v = new Dictionary<string, string[][]>(count);
            for (int i = 0; i < count; i++)
            {
                v.Add(Read(), ReadArray2());
            }
        }
        public void Read<T>(ref Dictionary<string, T[][]> v) where T : unmanaged
        {
            var count = Read<int>();
            v = new Dictionary<string, T[][]>(count);
            for (int i = 0; i < count; i++)
            {
                v.Add(Read(), ReadArray2<T>());
            }
        }
        public void Read<T1>(ref Dictionary<string, T1> v) where T1 : unmanaged
        {
            var count = Read<int>();
            v = new Dictionary<string, T1>(count);
            for (int i = 0; i < count; i++)
            {
                v.Add(Read(), Read<T1>());
            }
        }
        #endregion
    }
}
