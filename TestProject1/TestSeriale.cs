﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using F;
using static TestProject1.TestSeriale;

// author  (hf) Date：2023/5/8 17:26:08
namespace TestProject1
{
    public partial class TestSeriale : EventListener, IFSerializable
    {

        public string[] Strings;
        public string StringA;
        public int Inta;
        public int[] Intsa;
        public uint UintA=uint.MaxValue;
        public bb Enum;
        public float[] floats;
        public Sta Struct;
        public int[][] Int2Array;
        public int[][] Int3Array;

        public string[][] String2Array;

        public Dictionary<string, double> keyValuePairs =
            new Dictionary<string, double>();
        public Dictionary<string, string> keyValuePairs2 =
            new Dictionary<string, string>();
        public Dictionary<string, string[]> keyValuePairs23;
        public Dictionary<string, string[][]> keyValuePairs234;
        public Dictionary<string, double[][]> keyValuePairs2345;
        public Dictionary<int, string[][]> keyValuePairs23456;
        public Dictionary<string, Sta> keyValuePairsIFSerializable;
        public Dictionary<int, Sta[]> keyValuePairsIFSerializable1;
        public Dictionary<string, Sta[]> keyValuePairsIFSerializable12;
        public Dictionary<int, string> keyValuePairsIFSerializable13;

        public Sta[] Sata = new Sta[] { new Sta { As = new int[] { 1 } } };

        public TestF TestF2;

        public void Deserialization(Serializable serializable, string key = null)
        {
            throw new NotImplementedException();
        }

        public enum bb
        {
            a = 3,
            b = 4,
        }
        public struct BB<T> : IFSerializable where T : unmanaged
        {
            public T Value;
            public void Deserialization(Serializable serializable)
            {
                serializable.Read(ref Value);
            }

            public void Deserialization(Serializable serializable, string key = null)
            {
                throw new NotImplementedException();
            }

            public void Serialization(Serializable serializable)
            {
                serializable.Push(Value);
            }
        }
        //public Sta Staa;
        public struct Sta : IFSerializable
        {
            public int a;

            public int[] As;

            public Dictionary<int, int[]> keyValuePairs;

            public void Deserialization(Serializable serializable)
            {
                serializable.Read(ref a);
                serializable.Read(ref keyValuePairs);
                serializable.Read(ref As);
            }

            public void Deserialization(Serializable serializable, string key = null)
            {
                throw new NotImplementedException();
            }

            public void Serialization(Serializable serializable)
            {
                serializable.Push(a);
                serializable.Push(keyValuePairs);
                serializable.Push(As);
            }
        }
        public class TestF : IFSerializable
        {
            public void Deserialization(Serializable serializable)
            {
            }

            public void Deserialization(Serializable serializable, string key = null)
            {
                throw new NotImplementedException();
            }

            public void Serialization(Serializable serializable)
            {
            }
        }
    }

    /// <summary>
    /// 合并类，这个类，后面改成扫描生成
    /// </summary>
    public partial class TestSeriale
    {
        public void Deserialization(Serializable serialize)
        {
            serialize.ReadUint(ref UintA);
            serialize.Read(ref Inta);
            serialize.Read(ref Int3Array);
            serialize.Read(ref keyValuePairs);
            serialize.Read(ref keyValuePairs2);
            serialize.Read(ref keyValuePairs23);
            serialize.Read(ref keyValuePairs234);
            serialize.Read(ref keyValuePairs2345);
            serialize.Read(ref keyValuePairs23456);
            serialize.Read(ref StringA);
            serialize.Read(ref floats);
            serialize.Read(ref Enum);
            serialize.Read(ref Strings);
            serialize.Read(ref Int2Array);
            serialize.Read(ref String2Array);
            serialize.ReadSerializable(ref TestF2);
            serialize.ReadSerializable(ref Struct);
            serialize.ReadSerializable(ref Sata);
            serialize.ReadSerializable(ref keyValuePairsIFSerializable12);
            serialize.ReadSerializable(ref keyValuePairsIFSerializable1);
            serialize.ReadSerializable(ref keyValuePairsIFSerializable);
            serialize.Read(ref keyValuePairsIFSerializable13);

        }

        public void Serialization(Serializable serialize)
        {
            serialize.PushUInt(UintA);
            serialize.Push(Inta);
            serialize.Push(Int3Array);
            serialize.Push(keyValuePairs);
            serialize.Push(keyValuePairs2);
            serialize.Push(keyValuePairs23);
            serialize.Push(keyValuePairs234);
            serialize.Push(keyValuePairs2345);
            serialize.Push(keyValuePairs23456);
            serialize.Push(StringA);
            serialize.Push(floats);
            serialize.Push(Enum);
            serialize.Push(Strings);
            serialize.Push(Int2Array);
            serialize.Push(String2Array);
            serialize.PushSerializable(TestF2);
            serialize.PushSerializable(Struct);
            serialize.PushSerializable(Sata);
            serialize.PushSerializable(keyValuePairsIFSerializable12);
            serialize.PushSerializable(keyValuePairsIFSerializable1);
            serialize.PushSerializable(keyValuePairsIFSerializable);
            serialize.Push(keyValuePairsIFSerializable13);
        }
    }

}
