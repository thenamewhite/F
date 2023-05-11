﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

// author  (hf) Date：2023/5/6 14:36:45
namespace F
{
    public interface ISerialize
    {
        /// <summary>
        /// 自定义序列化
        /// </summary>
        /// <param name="bytes"></param>

        public void Serialization(Serialize bytes);

        /// <summary>
        /// 
        /// </summary>
        public void Deserialization(Serialize bytes);
    }

    public class Serialize
    {
        public byte[] bytes = Array.Empty<byte>();
        private int mPosition;
        public ByteStream GetSapn()
        {
            return new ByteStream(bytes) { Position = mPosition };
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
                    Push<string>(v); break;
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
                    Push<string>(v); break;
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
        public void Push<T>(string v)
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

        public void Push<T>(string[] v)
        {
            var span = GetSapn();
            span.Push(v);
            mPosition += span.Position - mPosition;
            bytes = span;
        }

        public void Push<T>(List<T> v) { }


        public void Read(object obj)
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
                    Read<string>(ref v); break;
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
                    //Read(obj);
                    break;
            }
        }

        public T Read<T>() where T : unmanaged
        {
            var span = GetSapn();
            mPosition += span.Position;
            return span.Read<T>();
        }

        public void Read<T>(ref T v) where T : unmanaged
        {
            var span = GetSapn();
            v = span.Read<T>();
            mPosition += span.Position;
        }

        public void Read<T>(ref string v)
        {
            var span = GetSapn();
            v = span.Read();
            mPosition += span.Position;
        }
        public void Read<T>(ref T[] v) where T : unmanaged
        {
            var span = GetSapn();
            v = span.ReadArray<T>();
            mPosition += span.Position;
        }

        public void Read<T>(ref List<T> v)
        {
        }

    }
}
