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
            var a = args[0].Split("|");
            //var p = "D:/HanZhi/Client/GamePlay/Library/ScriptAssemblies/Assembly-CSharp.dll";
            //var a = "D:/HanZhi/策划/Config|D:/HanZhi/Client/GamePlay/Assets/Scripts/Config/|D:/HanZhi/Client/GamePlay/Config/|D:/HanZhi/Client/GamePlay/Config//AllConfig.cfg".Split("|");

            ///第一个是 策划配置路径，二是代码生成路径，3是每个配置生成的byte 路径，4是总配置合并路径 ，4后面都是   typeof(GameLaunch).Assembly.Location dll 路基
            InstanceT.Assemblies.Clear();

            //第3个开始都是Assembly
            for (int i = 4; i < a.Length; i++)
            {
                InstanceT.AddAssembly(Assembly.LoadFile(a[i]));
            }
            InstanceT.AddAssembly(typeof(IFSerializable).Assembly);
            //InstanceT.AddAssembly(Assembly.LoadFile(p));
            //Console.WriteLine(InstanceT.Assemblies.Count);
            F.ExcelConfig.Start(a[0], a[1], a[2], a.Length >= 3 ? a[3] : null);
        }
    }
}