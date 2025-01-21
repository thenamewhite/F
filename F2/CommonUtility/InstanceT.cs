using System;
using System.Collections.Generic;
using System.Reflection;

// author  (hf) Date：2023/5/22 10:22:48
namespace F
{
    public static class InstanceT
    {
        public static readonly HashSet<Assembly> Assemblies = new HashSet<Assembly>();

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

            object obj = default;

            foreach (var item in Assemblies)
            {
                type = item.GetType(v);
                if (type != null) break;
            }

            if (type == null)
            {
                Console.WriteLine($" CreateInstance not fount className:{v}");
                throw new Exception($" CreateInstance not fount className:{v}");
            }

            obj = Activator.CreateInstance(type, args);
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