using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

// author  (hf) Date：2023/5/6 14:36:45
namespace F
{
    /// <summary>
    ///     使用 SerializableKey 需要手动调用Dispose释放内存
    /// </summary>
    public class SerializableKey : IDisposable
    {
        //public ByteStream ByteBuff = new ByteStream();
        public ByteStreamFixed ByteBuff;

        public SerializableKey(byte[] bytes)
        {
            ByteBuff = new ByteStreamFixed(bytes);
        }

        public SerializableKey(Span<byte> bytes)
        {
            ByteBuff = new ByteStreamFixed(bytes);
        }

        public SerializableKey()
        {
        }

        public bool IsEnd => ByteBuff.IsEnd;


        public static void Deserialization<T>(ref T v) where T : IFSerializableKey
        {
            using (var serializable = new SerializableKey())
            {
                serializable.ReadSerializable(ref v);
            }
        }

        #region read push obj

        public void PushSerializableObj(object obj)
        {
            var pos = BeginPush(0);
            var fildes = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var item in fildes)
            {
                var v = item.GetValue(obj);
                //null 值直接不写入
                if (v != null)
                    //Test();
                    PushObj(v, true, item);
            }

            EndPush(pos);
        }


        private void PushUnmanagedObj<T>(T v, bool isWriteName = true, FieldInfo fieldInfo = null) where T : unmanaged
        {
            if (isWriteName)
                Push(fieldInfo.Name, v);
            else
                Push(v);
        }

        private void PushUnmanagedObj<T>(T[] v, bool isWriteName = true, FieldInfo fieldInfo = null) where T : unmanaged
        {
            if (isWriteName)
                Push(fieldInfo.Name, v);
            else
                Push(v);
        }

        private void PushObj(object obj, bool isWriteName = true, FieldInfo fieldInfo = null)
        {
            switch (obj)
            {
                case bool v:
                    PushUnmanagedObj(v, isWriteName, fieldInfo);
                    break;
                case int v:
                    PushUnmanagedObj(v, isWriteName, fieldInfo);
                    break;
                case uint v:
                    PushUnmanagedObj(v, isWriteName, fieldInfo);
                    break;
                case float v:
                    PushUnmanagedObj(v, isWriteName, fieldInfo);
                    break;
                case ulong v:
                    PushUnmanagedObj(v, isWriteName, fieldInfo);
                    break;
                case double v:
                    PushUnmanagedObj(v, isWriteName, fieldInfo);
                    break;
                case byte v:
                    PushUnmanagedObj(v, isWriteName, fieldInfo);
                    break;
                case short v:
                    PushUnmanagedObj(v, isWriteName, fieldInfo);
                    break;
                case ushort v:
                    PushUnmanagedObj(v, isWriteName, fieldInfo);
                    break;
                case long v:
                    PushUnmanagedObj(v, isWriteName, fieldInfo);
                    break;
                case char v:
                    PushUnmanagedObj(v, isWriteName, fieldInfo);
                    break;
                case string v:
                    if (isWriteName)
                        Push(fieldInfo.Name, v);
                    else
                        Push(v);
                    break;
                case Enum v:
                    var underlyingType = Enum.GetUnderlyingType(v.GetType());
                    switch (Type.GetTypeCode(underlyingType))
                    {
                        case TypeCode.SByte:
                        case TypeCode.Byte: Push(fieldInfo.Name, Convert.ToByte(v)); break;
                        case TypeCode.Int16:
                        case TypeCode.UInt16: Push(fieldInfo.Name, Convert.ToInt32(v)); break;
                        case TypeCode.Int32:
                        case TypeCode.UInt32: Push(fieldInfo.Name, Convert.ToUInt32(v)); break;
                        case TypeCode.Int64:
                        case TypeCode.UInt64: Push(fieldInfo.Name, Convert.ToUInt64(v)); break;
                    }

                    break;
                case IFSerializableKey v:
                    if (isWriteName)
                        PushSerializable(fieldInfo.Name, v);
                    else
                        PushSerializable(v);
                    break;
                case IDictionary v:
                    var pos = BeginPush(fieldInfo.Name);
                    var count = (v?.Count).GetValueOrDefault();
                    ByteBuff.PushLength(count);
                    if (count > 0)
                    {
                        foreach (var key in v.Keys) PushObj(key, false);
                        foreach (var value in v.Values) PushObj(value, false);
                    }

                    EndPush(pos);
                    break;
                default:
                    if (obj.GetType().IsArray)
                    {
                        PushAarray(obj, isWriteName, fieldInfo);
                    }
                    else
                    {
                        var type = obj.GetType();
                        if (type.IsClass || (type.IsValueType && !type.IsEnum && type.IsPrimitive))
                        {
                            var fildes = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                            foreach (var item in fildes)
                            {
                                var v = item.GetValue(obj);
                                //null 值直接不写入
                                if (v != null) PushObj(v);
                            }
                        }
                    }

                    break;
            }
        }

        private void PushAarray(object obj, bool isWriteName, FieldInfo fieldInfo)
        {
            switch (obj)
            {
                case int[] v:
                    PushUnmanagedObj(v, isWriteName, fieldInfo); break;
                case uint[] v:
                    PushUnmanagedObj(v, isWriteName, fieldInfo); break;
                case double[] v:
                    PushUnmanagedObj(v, isWriteName, fieldInfo); break;
                case float[] v:
                    PushUnmanagedObj(v, isWriteName, fieldInfo); break;
                case byte[] v:
                    PushUnmanagedObj(v, isWriteName, fieldInfo); break;
                case sbyte[] v:
                    PushUnmanagedObj(v, isWriteName, fieldInfo); break;
                case short[] v:
                    PushUnmanagedObj(v, isWriteName, fieldInfo); break;
                case ushort[] v:
                    PushUnmanagedObj(v, isWriteName, fieldInfo); break;
                case ulong[] v:
                    PushUnmanagedObj(v, isWriteName, fieldInfo); break;
                case long[] v:
                    PushUnmanagedObj(v, isWriteName, fieldInfo); break;
                case char[] v:
                    PushUnmanagedObj(v, isWriteName, fieldInfo); break;
                case IFSerializableKey[] v:
                    PushSerializable(fieldInfo.Name, v); break;
                case string[] v:
                    if (isWriteName)
                        Push(fieldInfo.Name, v);
                    else
                        Push(v);
                    break;
                default:
                    //2维数组
                    if (obj is Array[] v2)
                    {
                        var length = (v2?.Length).GetValueOrDefault();
                        var po = BeginPush(fieldInfo.Name);
                        ByteBuff.PushLength(length);
                        if (length > 0)
                            foreach (var item in v2)
                                PushAarray(item, false, null);

                        EndPush(po);
                    }

                    break;
            }
        }

        // /// <summary>
        // /// 通过类（反射）读取
        // /// </summary>
        // /// <param name="v"></param>
        // public void ReadObjs(object v)
        // {
        // }

        // private void Read(object obj)
        // {
        //     switch (obj)
        //     {
        //         case int v:
        //             Read(ref v); break;
        //         case uint v:
        //             Read(ref v); break;
        //         case float v:
        //             Read(ref v); break;
        //         case string v:
        //             Read(ref v); break;
        //         case ulong v:
        //             Read(ref v); break;
        //         case double v:
        //             Read(ref v); break;
        //         case byte v:
        //             Read(ref v); break;
        //         case short v:
        //             Read(ref v); break;
        //         case ushort v:
        //             Read(ref v); break;
        //         case char v:
        //             Read(ref v); break;
        //         case int[] v:
        //             Read(ref v); break;
        //         //case Dictionary v:
        //         case int[][] v:
        //             break;
        //         default:
        //             //ReadObjs()
        //             //Read(obj);
        //             break;
        //     }
        // }
        // private void SetFiledValue(ByteStream stream, object v)
        // {
        //     var fildes = v.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        //     foreach (var item in fildes)
        //     {
        //         var type = item.FieldType;
        //         if (type == typeof(int))
        //         {
        //             item.SetValue(v, stream.Read<int>());
        //         }
        //         else if (type.IsEnum)
        //         {
        //             Type t = Enum.GetUnderlyingType(type);
        //             //var d = readV.Read<uint>();
        //             object eV = default;
        //             switch (Type.GetTypeCode(t))
        //             {
        //                 case TypeCode.SByte:
        //                 case TypeCode.Byte: eV = Enum.ToObject(type.GetElementType(), stream.Read<byte>()); break;
        //                 case TypeCode.Int16:
        //                 case TypeCode.UInt16: eV = Enum.ToObject(type, stream.Read<int>()); break;
        //                 case TypeCode.Int32:
        //                 case TypeCode.UInt32: eV = Enum.ToObject(type, stream.Read<uint>()); break;
        //                 case TypeCode.Int64:
        //                 case TypeCode.UInt64: eV = Enum.ToObject(type, stream.Read<ulong>()); break;
        //             }
        //             item.SetValue(v, eV);
        //         }
        //         else if (type == typeof(string))
        //         {
        //             item.SetValue(v, stream.Read());
        //         }
        //         else if (type == typeof(int[]))
        //         {
        //             item.SetValue(v, stream.ReadArray<int>());
        //         }
        //         else if (type == typeof(float[]))
        //         {
        //             item.SetValue(v, stream.ReadArray<float>());
        //         }
        //         else if (type == typeof(string[]))
        //         {
        //             item.SetValue(v, stream.ReadArray());
        //         }
        //         else if (type.IsValueType)
        //         {
        //             //创建结构体
        //             var d = Activator.CreateInstance(type);
        //             SetFiledValue(stream, d);
        //             item.SetValue(v, d);
        //         }
        //     }
        // }

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

        private void Push<T>(T v) where T : unmanaged
        {
            ByteBuff.Push(v);
        }

        private void Push(string v)
        {
            ByteBuff.Push(v);
        }

        public void Push(string k, string v)
        {
            var pos = BeginPush(k);
            ByteBuff.Push(v);
            EndPush(pos);
        }

        private void Push<T>(T[] v) where T : unmanaged
        {
            ByteBuff.Push(v);
        }

        private void Push(string[] v)
        {
            ByteBuff.Push(v);
        }

        public void Push<T>(string k, T[] v) where T : unmanaged
        {
            var pos = BeginPush(k);
            ByteBuff.Push(v);
            EndPush(pos);
        }

        public void Push(string k, string[] v)
        {
            var pos = BeginPush(k);
            ByteBuff.Push(v);
            EndPush(pos);
        }

        public void Push<T>(string k, T[][] v) where T : unmanaged
        {
            var pos = BeginPush(k);
            ByteBuff.Push(v);
            EndPush(pos);
        }

        public void Push(string k, string[][] v)
        {
            var pos = BeginPush(k);
            ByteBuff.Push(v);
            EndPush(pos);
        }

        //压缩字节
        public void PushInt(string k, int[][] v)
        {
            var pos = BeginPush(k);
            ByteBuff.PushInt(v);
            EndPush(pos);
        }

        public void PushInt(string k, int[] v)
        {
            var pos = BeginPush(k);
            ByteBuff.PushInt(v);
            EndPush(pos);
        }

        public void PushInt(string k, int v)
        {
            var pos = BeginPush(k);
            ByteBuff.PushInt(v);
            EndPush(pos);
        }

        public void PushUInt(string k, uint v)
        {
            var pos = BeginPush(k);
            ByteBuff.PushUInt(v);
            EndPush(pos);
        }

        public void PushUInt(string k, uint[][] v)
        {
            var pos = BeginPush(k);
            ByteBuff.PushUInt(v);
            EndPush(pos);
        }

        public void PushUInt(string k, uint[] v)
        {
            var pos = BeginPush(k);
            ByteBuff.PushUInt(v);
            EndPush(pos);
        }

        #region 数组

        public void PushSerializable<T>(string key, T[] v) where T : IFSerializableKey
        {
            var length = (v?.Length).GetValueOrDefault();
            var po = BeginPush(key);
            ByteBuff.PushLength(length);
            if (length > 0)
                foreach (var item in v)
                    PushSerializable(item);

            EndPush(po);
        }


        public void PushSerializable<T1>(string key, T1[][] v) where T1 : IFSerializableKey
        {
            var length = (v?.Length).GetValueOrDefault();
            var po = BeginPush(key);
            ByteBuff.PushLength(length);
            if (length > 0)
                foreach (var item in v)
                    PushSerializable(item);

            EndPush(po);
        }

        private void PushSerializable<T>(T[] v) where T : IFSerializableKey
        {
            var length = (v?.Length).GetValueOrDefault();
            ByteBuff.PushLength(length);
            if (length > 0)
                foreach (var item in v)
                    PushSerializable(item);
        }

        private void PushSerializable<T1>(T1[][] v) where T1 : IFSerializableKey
        {
            var length = (v?.Length).GetValueOrDefault();
            ByteBuff.PushLength(length);
            if (length > 0)
                foreach (var item in v)
                    PushSerializable(item);
        }

        #endregion

        #endregion

        #region Dictionary

        public void Push<T, T1>(string key, Dictionary<T, T1> v) where T : unmanaged where T1 : unmanaged
        {
            var pos = BeginPush(key);
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    ByteBuff.Push(item.Key);
                    ByteBuff.Push(item.Value);
                }

            EndPush(pos);
        }

        public void Push<T, T1>(string key, Dictionary<T, T1[]> v) where T : unmanaged where T1 : unmanaged
        {
            var pos = BeginPush(key);
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    ByteBuff.Push(item.Key);
                    ByteBuff.Push(item.Value);
                }

            EndPush(pos);
        }

        public void Push<T, T1>(string key, Dictionary<T, T1[][]> v) where T : unmanaged where T1 : unmanaged
        {
            var pos = BeginPush(key);
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    ByteBuff.Push(item.Key);
                    ByteBuff.Push(item.Value);
                }

            EndPush(pos);
        }

        public void Push<T>(string key, Dictionary<T, string> v) where T : unmanaged
        {
            var pos = BeginPush(key);

            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    ByteBuff.Push(item.Key);
                    ByteBuff.Push(item.Value);
                }

            EndPush(pos);
        }

        public void Push<T>(string key, Dictionary<T, string[]> v) where T : unmanaged
        {
            var pos = BeginPush(key);
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    ByteBuff.Push(item.Key);
                    ByteBuff.Push(item.Value);
                }

            EndPush(pos);
        }

        public void Push<T>(string key, Dictionary<T, string[][]> v) where T : unmanaged
        {
            var pos = BeginPush(key);
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    ByteBuff.Push(item.Key);
                    ByteBuff.Push(item.Value);
                }
        }

        public void Push<T1>(string key, Dictionary<string, T1> v) where T1 : unmanaged
        {
            var pos = BeginPush(key);
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    ByteBuff.Push(item.Key);
                    ByteBuff.Push(item.Value);
                }

            EndPush(pos);
        }

        public void Push(string key, Dictionary<string, string> v)
        {
            var pos = BeginPush(key);
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    ByteBuff.Push(item.Key);
                    ByteBuff.Push(item.Value);
                }

            EndPush(pos);
        }

        public void Push(string key, Dictionary<string, string[]> v)
        {
            var pos = BeginPush(key);
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    ByteBuff.Push(item.Key);
                    ByteBuff.Push(item.Value);
                }

            EndPush(pos);
        }

        public void Push<T>(string key, Dictionary<string, T[]> v) where T : unmanaged
        {
            var pos = BeginPush(key);
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    ByteBuff.Push(item.Key);
                    ByteBuff.Push(item.Value);
                }

            EndPush(pos);
        }

        public void Push<T>(string key, Dictionary<string, T[][]> v) where T : unmanaged
        {
            var pos = BeginPush(key);
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    ByteBuff.Push(item.Key);
                    ByteBuff.Push(item.Value);
                }

            EndPush(pos);
        }

        public void Push(string key, Dictionary<string, string[][]> v)
        {
            var pos = BeginPush(key);
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    ByteBuff.Push(item.Key);
                    ByteBuff.Push(item.Value);
                }

            EndPush(pos);
        }


        public void PushSerializable<T, T1>(string key, Dictionary<T, T1> v)
            where T : unmanaged where T1 : IFSerializableKey
        {
            var pos = BeginPush(key);
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    ByteBuff.Push(item.Key);
                    PushSerializable(item.Value);
                }

            EndPush(pos);
        }

        public void PushSerializable<T1>(string key, Dictionary<string, T1> v) where T1 : IFSerializableKey
        {
            var pos = BeginPush(key);
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    ByteBuff.Push(item.Key);
                    PushSerializable(item.Value);
                }

            EndPush(pos);
        }

        public void PushSerializable<T, T1>(string key, Dictionary<T, T1[]> v)
            where T : unmanaged where T1 : IFSerializableKey
        {
            var pos = BeginPush(key);
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    ByteBuff.Push(item.Key);
                    PushSerializable(item.Value);
                }

            EndPush(pos);
        }

        public void PushSerializable<T, T1>(string key, Dictionary<T, IFSerializableKey[][]> v)
            where T : unmanaged where T1 : IFSerializableKey
        {
            var pos = BeginPush(key);
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    ByteBuff.Push(item.Key);
                    PushSerializable(item.Value);
                }

            EndPush(pos);
        }

        public void PushSerializable<T1>(string key, Dictionary<string, T1[]> v) where T1 : IFSerializableKey
        {
            var pos = BeginPush(key);
            var count = (v?.Count).GetValueOrDefault();
            ByteBuff.PushLength(count);
            if (count > 0)
                foreach (var item in v)
                {
                    ByteBuff.Push(item.Key);
                    PushSerializable(item.Value);
                }

            EndPush(pos);
        }

        public void PushSerializable<T>(string key, T v) where T : IFSerializableKey
        {
            if (v != null)
            {
                var keyP = BeginPush(key);
                //占位
                var pos = BeginPush(0);
                v.Serialization(this);
                //占位当前数据总长度,用做判断是否解析了数据
                EndPush(keyP);
                //占位当前数据总长度,用做后续解析
                EndPush(pos);
                //public void ReadSerializable<T>(ref T v) where T : IFSerializableKey 解意
            }
            else
            {
                ByteBuff.Push(string.Empty);
                ByteBuff.Push(0);
            }
        }

        public void PushSerializable<T>(T v) where T : IFSerializableKey
        {
            if (v != null)
            {
                var pos = BeginPush(0);
                v.Serialization(this);
                EndPush(pos);
            }
            else
            {
                ByteBuff.Push(0);
            }
        }

        public void ReadSerializable<T>(ref T v) where T : IFSerializableKey
        {
            if (v != null)
            {
                var serLength = Read<int>();
                while (serLength > 0)
                {
                    var beginPos = ByteBuff.Position;
                    var key = Read();
                    var dataLength = Read<int>();
                    var pos = ByteBuff.Position;
                    v.Deserialization(this, key);
                    if (dataLength > 0 && ByteBuff.Position - pos != dataLength)
                        ByteBuff.SetPosition((uint)(pos + dataLength));
                    serLength -= ByteBuff.Position - beginPos;
                }
            }
        }

        //public T Deserialization<T>(ref T v) where T : IFSerializableKey
        //{
        //    ReadSerializable(ref v);
        //    return v;
        //}


        public void ReadSerializable<T>(ref T[] v) where T : IFSerializableKey, new()
        {
            var length = ByteBuff.ReadLength();
            if (length > 0) v = new T[length];
            for (var i = 0; i < length; i++)
            {
                var se = new T();
                ReadSerializable(ref se);
                v[i] = se;
            }
        }

        public void ReadSerializable<T, T1>(ref Dictionary<T, T1> v)
            where T : unmanaged where T1 : IFSerializableKey, new()
        {
            var count = ByteBuff.ReadLength();
            v = new Dictionary<T, T1>();
            while (count > 0)
            {
                var se = new T1();
                var key = Read<T>();
                //se.Deserialization(this);
                ReadSerializable(ref se);
                v.Add(key, se);
                count--;
            }
        }

        public void ReadSerializable<T>(ref Dictionary<string, T> v) where T : IFSerializableKey, new()
        {
            var count = ByteBuff.ReadLength();
            v = new Dictionary<string, T>();
            while (count > 0)
            {
                var se = new T();
                var key = Read();
                ReadSerializable(ref se);
                v.Add(key, se);
                count--;
            }
        }

        public void ReadSerializable<T, T1>(ref Dictionary<T, T1[]> v)
            where T : unmanaged where T1 : IFSerializableKey, new()
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
                    var se = new T1();
                    ReadSerializable(ref se);
                    c[i] = se;
                }

                v.Add(key, c);
                count--;
            }
        }

        public void ReadSerializable<T1>(ref Dictionary<string, T1[]> v) where T1 : IFSerializableKey, new()
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
                    var se = new T1();
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

        public void Push<T>(string key, List<T> v) where T : unmanaged
        {
            var pos = BeginPush(key);
            var valueOrDefault = (v?.Count).GetValueOrDefault();
            ByteBuff.Push(valueOrDefault);
            if (valueOrDefault > 0)
                foreach (var v2 in v)
                    Push(v2);

            EndPush(pos);
        }

        public void Push(string key, List<string> v)
        {
            var pos = BeginPush(key);
            var valueOrDefault = (v?.Count).GetValueOrDefault();
            ByteBuff.Push(valueOrDefault);
            if (valueOrDefault > 0)
                foreach (var v2 in v)
                    Push(v2);

            EndPush(pos);
        }

        public void PushSerializable<T>(string key, List<T> v) where T : IFSerializableKey
        {
            var pos = BeginPush(key);
            var valueOrDefault = (v?.Count).GetValueOrDefault();
            ByteBuff.Push(valueOrDefault);
            if (valueOrDefault > 0)
                foreach (var v2 in v)
                    PushSerializable(v2);

            EndPush(pos);
        }

        public List<T> ReadSerializable<T>(ref List<T> v) where T : IFSerializableKey, new()
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

        public List<T> Read<T>(ref List<T> v) where T : unmanaged
        {
            var num = Read<int>();
            if (num > 0) v = new List<T>(num);
            for (var i = 0; i < num; i++) v.Add(Read<T>());
            return v;
        }

        public List<string> Read(ref List<string> v)
        {
            var num = Read<int>();
            if (num > 0) v = new List<string>(num);
            for (var i = 0; i < num; i++) v.Add(Read());
            return v;
        }

        #endregion
    }
}