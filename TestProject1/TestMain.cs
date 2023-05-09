
using F;
using System.IO;

namespace TestProject1
{
    public class TestMain
    {

        public static void Main()
        {
            var c = "1234123";
            var sp = c.Split("4");
            var s = F.StringExtension.Split(c, "0");

            var sere = new TestSeriale();
            sere.a = "1";
            sere.Inta = 2;
            sere.Intsa = new int[1] { 1 };
            sere.floats = new float[3] { 1.1f, 2f, 3f };
            var sbb = new Serialize();
            sere.Serialization(sbb);
            var d = new TestSeriale();
            d.Deserialization(new Serialize() { bytes = sbb.bytes });
            //var d = new ByteStream();
            //d.Push(-1);
            //var d2 = new ByteStream(d.Span);
            //var qqq = d2.Read<int>();
            //d.Push(2);
            //d.Push<string>("3");
            //d.Push(true);
            //d.Push(new int[] { 1, 3 });.
            //d.Push(new long[] { 4, 2 });
            //d.Read<int>();

            //var a = new ByteStream(d.Span);
            //var intS = a.Read();
            //var longS = a.ReadArray<long>();
            //byte[] buffer = d;

            //var intValue = a.Read<double>();
            //var intValue2 = a.Read<int>();
            //bool stirngValue = a.Read<bool>();
            //Console.WriteLine(intValue);
            //Console.WriteLine(stirngValue);
            //var dd = F.StringExtension.Span(new int[1] { 1 });
            //Console.WriteLine(sp);
            //BinaryWriter
            //C = sp[0];
        }
    }
}