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
    public class TestSeriale : ISerialize
    {

        public string[] strings;
        public string a;
        public int Inta;
        //public List<string> Lista;
        public int[] Intsa;
        public int[] Intsb;
        public bb aaaa;
        public float[] floats;
        public Sta cc;

        //public Sta Staa;
        public struct Sta
        {
            public int a;
        }
        public enum bb
        {
            a = 3,
        }
        public void Deserialization(Serialize serialize)
        {
            serialize.ReadObjs(this);
        }

        public void Serialization(Serialize serialize)
        {
            serialize.WriteObjs(this);
            //serialize.Push(aaaa);
        }
    }
}
