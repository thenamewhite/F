using F;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{


    public struct Tsea : IFSerializableKey
    {
        public int a;
        public void Deserialization(SerializableKey serializable, string key)
        {
            switch (key)
            {
                case nameof(a):
                    serializable.Read(ref a);
                    break;
                default:
                    break;
            }
        }

        public void Serialization(SerializableKey serializable)
        {
            serializable.Push(nameof(a), 1);
        }
    }
    public class TestSerialeKey : F.IFSerializableKey
    {
        public int a;
        public int b;
        public Tsea[] tse;
        public Tsea tse2;

        public void Deserialization(SerializableKey serializable, string key)
        {
            switch (key)
            {
                case nameof(tse):
                    serializable.ReadSerializable(ref tse);
                    break;
                case nameof(a):
                    serializable.Read(ref a);
                    break;
                default:
                    break;
            }
        }

        public void Serialization(SerializableKey serializable)
        {
            serializable.Push(nameof(a), 1);
            tse = new Tsea[2];
            tse[0] = new Tsea() { a = 2 };
            tse[1] = new Tsea() { a = 3 };
            serializable.PushSerializable(nameof(tse), tse);
        }
    }
}
