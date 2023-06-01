
using F;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json;
using static F.EventListener;

namespace TestProject1
{
    public class TestMain
    {

        public static void Main()
        {
            InstanceT.AddAssembly(typeof(TestSeriale).Assembly);


            var testSere = new TestSeriale();
            testSere.StringA = "啊啊";
            testSere.Strings = new string[] { };
            testSere.Inta = 9999999;
            testSere.Enum = TestSeriale.bb.b;
            testSere.Intsa = new int[0] { };
            testSere.Struct = new TestSeriale.Sta() { a = 3, As = new int[2] { 2, 3 }, keyValuePairs = new Dictionary<int, int[]>() { { 1, new int[] { 1, 3, 2 } } } };
            testSere.floats = new float[3] { -1.1f, 2f, 3f };
            testSere.Int2Array = new int[2][] { new int[] { 1, 2, 3 }, new int[] { 3, 4, 5 } };
            testSere.String2Array = new string[2][] { new string[] { "中国汉字啊1231,af", "12312", "113" }, new string[] { "测试中" } };
            var serializable = new Serializable();
            serializable.Push(typeof(TestSeriale).FullName);
            testSere.Serialization(serializable);
            var path = "F:\\F2\\TestProject1\\11.txt";
            var dictString = new Dictionary<string, byte[]>();
            dictString.Add(typeof(TestSeriale).FullName, serializable.Bytes);
            FileIO.WriteBytesDict(path, dictString);
            var aa = FileIO.ReadBytesDict<TestSeriale>(File.ReadAllBytes(path));
            var dictInt = new Dictionary<int, byte[]>();
            dictInt.Add(1, serializable.Bytes);
            FileIO.WriteBytesDict(path, dictInt);
            var bb = FileIO.ReadBytesDict<int, IFSerializable>(File.ReadAllBytes(path));

        }

    }
}