
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

            var d = new ByteStream();
            //d.Push(1);
            //d.Push(2);
            d.Push("3");
            //d.Push(true);
            //d.Push(new int[] { 1, 3 });.
            //d.Push(new long[] { 4, 2 });
            //d.Read<int>();

            var a = new ByteStream(d.Span);
            var intS = a.Read();
            var longS = a.ReadArray<long>();
            byte[] buffer = d;

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