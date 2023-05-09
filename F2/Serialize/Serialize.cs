using System;
using System.Collections;
using System.Collections.Generic;

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
                if (v != null)
                {
                    Push(v);
                }
            }
        }
        public void Push(object obj)
        {
            switch (obj)
            {
                case int v:
                    Push(v); break;
                case uint v:
                    Push(v); break;
                case float v:
                    Push(v); break;
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
                    PushAarray(obj);
                    break;
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
                case sbyte[] v: break;
                case short[] v: break;
                case ushort[] v: break;
                case ulong[] v: break;
                default:
                    throw new Exception($"not find,{obj.GetType()}");
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

        public void ReadObjs(object v)
        {
            var fildes = v.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var readV = GetSapn();
            foreach (var item in fildes)
            {
                var type = item.FieldType;
                if (type == typeof(int))
                {
                    item.SetValue(v, readV.Read<int>());
                }
                else if (type == typeof(string))
                {
                    item.SetValue(v, readV.Read());
                }
                else if (type == typeof(int[]))
                {
                    item.SetValue(v, readV.ReadArray<int>());
                }
                else if (type == typeof(float[]))
                {
                    item.SetValue(v, readV.ReadArray<float>());
                }
            }
        }

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
