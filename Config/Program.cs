using ConfigExe.Config;
using System.Diagnostics;
using F;
using System.Reflection;
using System;
namespace ConfigExe
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Console.WriteLine("Hello, World!");
            //ExcelConfig.Start(args[0], args[1]);
            //foreach (var item in args)
            //{
            //    Console.WriteLine($"Path{item.Split(';')[0]}");
            //}
            //Console.WriteLine(args.Length);
            ////Console.WriteLine(args.Length);
            var bb = "D:/HanZhi/策划/Config|D:/HanZhi/Client/GamePlay/Assets/Scripts/Config/|D:/HanZhi/Client/GamePlay/Config/|D:\\HanZhi\\Client\\GamePlay\\Library\\ScriptAssemblies\\Assembly-CSharp.dll|D:\\HanZhi\\Client\\GamePlay\\Library\\ScriptAssemblies\\Assembly-CSharp.dll";
            //var a = args[0].Split("|");
            var a = bb.Split("|");

            //foreach (var b in a)
            //{
            //    Console.WriteLine(b);
            //}
            InstanceT.Assemblies.Clear();
            Console.WriteLine(a[0]);
            Console.WriteLine(a[1]);
            Console.WriteLine(a[2]);
            Console.WriteLine(Assembly.LoadFile(a[3]));
            for (int i = 3; i < a.Length; i++)
            {
                InstanceT.AddAssembly(Assembly.LoadFile(a[i]));
            }
            InstanceT.AddAssembly(typeof(IFSerializable).Assembly);
            Console.WriteLine(InstanceT.Assemblies.Count);
            F.ExcelConfig.Start(a[0], a[1], a[2]);

        }
    }
}