
using F;
using System.Runtime.Serialization.Formatters.Binary;
namespace TestProject1
{


    public class TestMain : IInitialization
    {


        public ObjectPool<TestMain> objectPool;

        public void T()
        {
            var c = objectPool.New();
        }
        public struct Sta : IFSerializableKey
        {
            public int a;
            public int b;
            public int[] As;

            public Dictionary<int, int[]> keyValuePairs;

            public void Deserialization(Serializable serializable, string key)
            {
                switch (key)
                {
                    case nameof(a):
                        serializable.Read(ref a);
                        break;
                    case nameof(b):
                        Console.WriteLine("");
                        break;
                    case "c":
                        var ccc = serializable.ByteBuff.Read<int>();
                        break;
                    default:
                        break;
                }
            }

            public void Serialization(Serializable serializable)
            {
                serializable.Push(nameof(a), a);
                serializable.Push(nameof(b), b);
                serializable.Push("c", 1);
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



        public class BB : EventListener
        {

            public void bb()
            {

            }
            //public void aOnEventHandler(EventListenerData<abb> eventListener)
            //{

            //}
        }

        public struct abb
        {
            public int x;
        }
        public struct b
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v2">啊啊</param>
        public static void Tes()
        {


            var mBuffer = new byte[] { };
            mBuffer.Add<byte>(1);
            //for (int i = 0; i < 10; i++)
            //{
            //    byte v = 0;s
            int[] numbers = { 1, 2, 3, 4, 5 };
            //var cccccc = new Dictionary<int>(numbers);
            var abb = numbers[..1];
            int[] subArray = numbers[new Range()]; // 使用索引范围对象获取子数组

            Console.WriteLine(string.Join(", ", subArray)); // 输出子数组的元素
            //}
            var a = new F.ByteStream();

            var inde = 100000;
            while (inde-- > 0)
            {
                a.Push(inde);
            }
            double aa = 1;
            var sp = new Span<byte>();
            //a.Push(-1, ref sp);
            //a.Push(-2, ref sp);
            //a.Push(1);
            //a.Push(new int[] { 1 });
            //a.Push("11");
            //a.Push("22");
            //a.SetPosition(0);
            //var d = a.Read<int>();
            //var dd = a.ReadArray<int>();
            //var b = a.Read();
            //var c = a.R                                         ead();
            //var st = new Stream();
            var f = new BinaryFormatter();
        }




        static List<int> GenerateRandomArray(int targetSum, int minValue, int length)
        {
            List<int> result = new List<int>();
            Random random = new Random();

            // 首先生成一个满足条件的随机数组，长度为length，最小值为minValue，元素总和为targetSum
            for (int i = 0; i < length; i++)
            {
                int randomNum = random.Next(minValue, targetSum - i * minValue) + minValue;
                result.Add(randomNum);
            }

            // 然后对数组进行调整，使得数组的元素总和为targetSum
            int sum = result.Sum();
            int diff = targetSum - sum;
            if (diff > 0)
            {
                int index = random.Next(0, length);
                result[index] += diff;
            }

            return result;
        }



        public static void Main()
        {
            sbyte b = -127;

            var cccc = new ByteStream();
            cccc.Push(b);
        }

        public void Initialization()
        {
        }

        public void Release()
        {
        }

        struct PointSctruct
        {
            public int X { get; set; }
            public int Y { get; set; }
            public PointSctruct()//C#10以后允许struct声明无参构造函数
            {
                X = 1;
                Y = 2;
            }
            public PointSctruct(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        [Flags]
        public enum Ea
        {

            V = 1,
            V2 = 2,
            v3 = 3,
        }

    }
}