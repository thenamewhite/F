using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using F;

// author  (hf) Date：2023/5/8 17:26:08
namespace TestProject1
{
    public class TestSeriale : IFSerializable
    {

        public string[] Strings;
        public string StringA;
        public int Inta;
        //public List<string> Lista;
        public int[] Intsa;
        public bb Enum;
        public float[] floats;
        public List<string> ListString;
        public List<int> ListInt;
        public Sta Struct;
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
                serializable.Read(ref As);
                if (!serializable.IsEnd)
                {
                    var count = serializable.Read<int>();
                    keyValuePairs = new Dictionary<int, int[]>(count);
                    for (int i = 0; i < count; i++)
                    {
                        keyValuePairs.Add(serializable.Read<int>(), serializable.ReadArray<int>());
                    }
                }
            }

            public void Serialization(Serializable serializable)
            {
                serializable.Push(a);
                serializable.Push(As);
                if (keyValuePairs != null)
                {
                    var count = keyValuePairs.Count;
                    serializable.Push(count);
                    foreach (var item in keyValuePairs)
                    {
                        serializable.Push(item.Key);
                        serializable.Push(item.Value);
                    }
                }
            }
        }

        public void Deserialization(Serializable serialize)
        {
            serialize.Read(ref StringA);
            serialize.Read(ref floats);
            serialize.Read(ref Enum);
            serialize.Read(ref Strings);
            //serialize.Read(ref ListString);
            serialize.ReadSerializables(ref Struct);
        }

        public void Serialization(Serializable serialize)
        {
            serialize.Push(StringA);
            serialize.Push(floats);
            serialize.Push(Enum);
            serialize.Push(Strings);
            //serialize.Push(ListString);
            serialize.PushSerializables(Struct);
        }
    }
}
