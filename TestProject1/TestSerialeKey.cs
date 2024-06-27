using F;

namespace TestProject1
{
    public struct Tsea : IFSerializableKey
    {
        public int a;
        public string b;
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
        public Dictionary<int, string> aT;
        public int[] aInt;

        public List<string> ListString = new List<string>();
        public List<int> ListInts = new List<int>();
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
                case nameof(aT):
                    serializable.Read(ref aT);
                    break;
                case nameof(tse2):
                    serializable.ReadSerializable(ref tse2);
                    break;
                case nameof(ListString):
                    serializable.Read(ref ListString);
                    break;
                case nameof(ListInts):
                    serializable.Read(ref ListInts);
                    break;
                default:
                    break;
                    ///
            }
        }

        public void Serialization(SerializableKey serializable)
        {
            //serializable.Push(nameof(a), a);
            //tse2 = new Tsea() { b = "2" ,a=3};
            serializable.PushSerializable(nameof(tse2), v: tse2);
            ListString = new List<string>() { "中国" };
            serializable.Push(nameof(ListString), ListString);
            //tse = new Tsea[2];
            //tse[0] = new Tsea() { a = 2 };
            //tse[1] = new Tsea() { a = 3 };
            aT = new Dictionary<int, string>() { { 1, "2" } };
            serializable.Push(nameof(aT), aT);
            ListInts.Add(1);
            serializable.Push(nameof(ListInts), ListInts);
            //serializable.PushSerializable(nameof(tse), tse);
        }
    }
}
