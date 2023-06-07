using System;
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
        public bb Enum;
        public float[] floats;
        public Sta Struct;
        public int[][] Int2Array;

        public string[][] String2Array;

        public Dictionary<string, double> keyValuePairs =
            new Dictionary<string, double>();
        public Dictionary<string, string> keyValuePairs2 =
            new Dictionary<string, string>();
        public Dictionary<string, string[]> keyValuePairs23 =
            new Dictionary<string, string[]>() { { "1", new string[] { "11阿达阿达" } } };
        public Dictionary<string, string[][]> keyValuePairs234 =
            new Dictionary<string, string[][]>() { { "1", new string[][] { new string[1] { "中文@#*3*" } } } };
        public Dictionary<string, double[][]> keyValuePairs2345 =
            new Dictionary<string, double[][]>() { { "1", new double[1][] { new double[1] { 1.2f } } } };
        public Dictionary<int, string[][]> keyValuePairs23456 =
            new Dictionary<int, string[][]>() { { 1, new string[1][] { new string[1] { "1231" } } } };
        public Dictionary<int, Sta> keyValuePairsIFSerializable =
            new Dictionary<int, Sta>() { { 1, new Sta() { a = 11 } } };
        public Dictionary<int, Sta[]> keyValuePairsIFSerializable1 =
            new Dictionary<int, Sta[]>() { { 1, new Sta[] { new Sta() { a = 11232 } } } };
        public Dictionary<string, Sta[]> keyValuePairsIFSerializable12 =
            new Dictionary<string, Sta[]>() { { "2323", new Sta[] { new Sta() { a = 11232 } } } };
        public Sta[] Sata = new Sta[] { new Sta { As = new int[] { 1 } } };

        public TestF TestF2;
        //public void Deserialization(Serializable serializable)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Serialization(Serializable serializable)
        //{
        //    throw new NotImplementedException();
        //}

        public enum bb
        {
            a = 3,
            b = 4,
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
        }

        public void Serialization(Serializable serialize)
        {
            //serialize.Push(new TestSeriale());
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
        }
    }

}
