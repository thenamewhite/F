
using F;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using static TestProject1.TestSeriale;

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
            testSere.keyValuePairs23 = new Dictionary<string, string[]>() { { "1", new string[] { "11阿达阿达" } } };
            testSere.keyValuePairs234 = new Dictionary<string, string[][]>() { { "1", new string[][] { new string[1] { "中文@#*3*" } } } };
            testSere.keyValuePairs2345 = new Dictionary<string, double[][]>() { { "1", new double[1][] { new double[1] { 1.2f } } } };
            testSere.keyValuePairs23456 = new Dictionary<int, string[][]>() { { 1, new string[1][] { new string[1] { "1231" } } } };
            testSere.keyValuePairsIFSerializable = new Dictionary<string, Sta>() { { "1", new Sta() { a = 11 } } };
            testSere.keyValuePairsIFSerializable12 = new Dictionary<string, Sta[]>() { { "2323", new Sta[] { new Sta() { a = 11232 } } } };
            testSere.keyValuePairsIFSerializable1 = new Dictionary<int, Sta[]>() { { 1, new Sta[] { new Sta() { a = 11232 } } } };
            var serializable = new Serializable();
            //serializable.Push(typeof(TestSeriale).FullName);
            testSere.Serialization(serializable);
            var path = "F:\\F2\\TestProject1\\11.txt";
            var dictString = new Dictionary<string, IFSerializable>();
            dictString.Add("11", testSere);
            var d = new Sta();
            dictString.Add("22", d);
            FileIO.WriteBytesDict(path, dictString, true);
            var aa = FileIO.ReadBytesDict<IFSerializable>(File.ReadAllBytes(path),true);
            //var dictInt = new Dictionary<int, byte[]>();
            //dictInt.Add(1, serializable.Bytes);
            //FileIO.WriteBytesDict(path, dictInt);
            //var bb = FileIO.ReadBytesDict<int, IFSerializable>(File.ReadAllBytes(path));

            //var by = new ByteStream();
            //by.Push("11");
            //var st = by.Read("11");
            TestGenerateConfig.Start();
        }


    }
}