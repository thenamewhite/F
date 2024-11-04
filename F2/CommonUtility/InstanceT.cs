using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

// author  (hf) Date：2023/5/22 10:22:48
namespace F
{
    public static class InstanceT
    {

        public readonly static HashSet<Assembly> Assemblies = new HashSet<Assembly>();

        public static void AddAssembly(Assembly assembly)
        {
            Assemblies.Add(assembly);
        }
        public static T CreateInstance<T>()
        {
            return Activator.CreateInstance<T>();
        }
        public static object CreateInstance(string v, object[] args = null)
        {
            Type type = default;

            Object obj = default;

            foreach (var item in Assemblies)
            {
                type = item.GetType(v);
                if (type != null)
                {
                    break;
                }
            }
            if (type == null)
            {
                Console.WriteLine($" CreateInstance not fount className:{v}");
                throw new Exception($" CreateInstance not fount className:{v}");
            }
            else
            {
                obj = Activator.CreateInstance(type, args);
            }
            return obj;
        }

        public static T CreateInstance<T>(string v) where T : class
        {
            return CreateInstance(v) as T;
        }

        public static object CreateInstance(Func<Type> callBack)
        {
            return Activator.CreateInstance(callBack.Invoke());
        }
    }
}
