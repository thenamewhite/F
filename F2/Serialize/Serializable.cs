using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

// author  (hf) Date：2023/5/6 14:36:45
namespace F
{
    public interface IFSerializable
    {
        /// <summary>
        /// 自定义序列化
        /// </summary>
        /// <param name="serializable"></param>

        public void Serialization(Serializable serializable);

        /// <summary>
        /// 
        /// </summary>
        public void Deserialization(Serializable serializable);
    }

    public class Serializable
    {
        public byte[] bytes = Array.Empty<byte>();
        private int mPosition;
        public ByteStream GetSapn()
        {
            return new ByteStream(bytes) { Position = mPosition };
        }
        public Serializable(byte[] bytes)
        {
            this.bytes = bytes;
        }
        public Serializable(Span<byte> bytes)
        {
            this.bytes = bytes.ToArray();
        }
        public Serializable()
        {
        }
        public bool IsEnd
        {
            get => GetSapn().IsEnd;
        }

        public void WriteObjs(object obj)
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
                    PushEnum(v);
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
                default:
                    if (obj.GetType().IsArray)
                    {
                        PushAarray(obj);
                    }
                    else
                    {
                        WriteObjs(obj);
                    }
                    break;
            }
        }
        private void PushEnum(Enum v)
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
                    break;
                case sbyte[] v:
                    Push(v); break;
                case short[] v:
                    Push(v); break;
                case ushort[] v:
                    Push(v); break;
                case ulong[] v:
                    Push(v); break;
                default:
                    throw new Exception($"not find,{obj.GetType()}");
            }
        }


        /// <summary>
        /// 通过类（反射）读取
        /// </summary>
        /// <param name="v"></param>
        public void ReadObjs(object v)
        {

            var sapn = GetSapn();

            SetFiledValue(sapn, v);

            void SetFiledValue(ByteStream stream, object v)
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
        }

        public void Push<T>(T v) where T : unmanaged
        {
            var span = GetSapn();
            span.Push(v);
            mPosition += span.Position - mPosition;
            bytes = span;
        }
        public void Push(string v)
        {
            var span = GetSapn();
            span.Push(v);
            mPosition += span.Position - mPosition;
            bytes = span;
        }

        public void Push<T>(T[] v) where T : unmanaged
        {
            var span = GetSapn();
            span.Push(v);
            mPosition += span.Position - mPosition;
            bytes = span;
        }

        public void Push(string[] v)
        {
            var span = GetSapn();
            span.Push(v);
            mPosition += span.Position - mPosition;
            bytes = span;
        }

        //public void Push<T>(List<T> v) where T : unmanaged
        //{
        //    var span = GetSapn();
        //    span.Push(v);
        //    mPosition += span.Position - mPosition;
        //    bytes = span;
        //}
        //public void Push(List<string> v)
        //{
        //    var span = GetSapn();
        //    span.Push(v);
        //    mPosition += span.Position - mPosition;
        //    bytes = span;
        //}

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
                default:
                    //ReadObjs()
                    //Read(obj);
                    break;
            }
        }



        public void ReadSerializables<T>(ref T v) where T : IFSerializable
        {
            v.Deserialization(this);
        }
        public void PushSerializables<T>(T v) where T : IFSerializable
        {
            v.Serialization(this);
        }


        public T Read<T>() where T : unmanaged
        {
            var span = GetSapn();
            var v = span.Read<T>();
            mPosition += span.Position - mPosition;
            return v;
        }

        public string Read()
        {
            var span = GetSapn();
            var v = span.Read();
            mPosition += span.Position - mPosition;
            return v;
        }

        public T[] ReadArray<T>() where T : unmanaged
        {
            var span = GetSapn();
            var v = span.ReadArray<T>();
            mPosition += span.Position - mPosition;
            return v;
        }
        public string[] ReadArray()
        {
            var span = GetSapn();
            var v = span.ReadArray();
            mPosition += span.Position - mPosition;
            return v;
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


        //public void Read<T>(ref List<T> v) where T : unmanaged
        //{
        //}
        //public void Read(ref List<string> v)
        //{
        //    var span = GetSapn();
        //    v = span.ReadArray();
        //    mPosition += span.Position - mPosition;
        //}
    }
}
