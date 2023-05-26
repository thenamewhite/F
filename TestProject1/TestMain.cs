
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
            var testSere = new TestSeriale();
            testSere.StringA = "°¡°¡";
            testSere.Strings = new string[] { };
            testSere.Inta = 9999999;
            testSere.Enum = TestSeriale.bb.b;
            testSere.Intsa = new int[0] { };
            testSere.Struct = new TestSeriale.Sta() { a = 3, As = new int[2] { 2, 3 }, keyValuePairs = new Dictionary<int, int[]>() { { 1, new int[] { 1, 3, 2 } } } };
            testSere.floats = new float[3] { -1.1f, 2f, 3f };
            testSere.Int2Array = new int[2][] { new int[] { 1, 2, 3 }, new int[] { 3, 4, 5 } };
            testSere.String2Array = new string[2][] { new string[] { "ÖÐ¹úºº×Ö°¡1231,af", "12312", "113" }, new string[] { "²âÊÔÖÐ" } };
            var serializable = new Serializable();
            testSere.Serialization(serializable);
            var doSerializable = new TestSeriale();
            doSerializable.Deserialization(new Serializable(serializable.Bytes));

            testSere.AddEvent<float>(fun, 0, true);
            testSere.AddEvent<float>((ref Event<float> a) => { }, 8, true);
            testSere.AddEvent<float>((ref Event<float> a) =>
            {

            }, 9, true);
            testSere.AddEvent<float>((ref Event<float> a) =>
            {

            }, 7, true);
            testSere.AddEvent<float>((ref Event<float> a) =>
            {

            }, 7100, true);
            testSere.AddEvent<float>((ref Event<float> a) =>
            {

            }, -9000, true);
            testSere.AddEvent<float>((ref Event<float> a) =>
            {

            }, -800, true);

            float c = 1;
            testSere.DispatchEvent<float>(ref c);
            void fun(ref Event<float> t)
            {
                var ddd = t.Value;
                t.IsBreak = true;
                //t.Action = null;
                //t = new Event<float>();
                //testSere.RemoveEvent<float>(fun);
            }
            var path = "F:\\F2\\TestProject1\\11.txt";
            var dict = new Dictionary<int, byte[]>();
            dict.Add(1, serializable.Bytes);
            dict.Add(2, serializable.Bytes);
            //FileIO.WriteBytes(path, dict);
            //var c = JsonSerializer.Serialize<TestSeriale>(testSere);
            //var d = FileIO.ReadBytesDict<TestSeriale>(File.ReadAllBytes(path));
            //FileIO.WriteBase64(path, dict);
            //var base64d = FileIO.ReadBase64Dict<TestSeriale>(File.ReadAllLines(path));
        }

    }
}