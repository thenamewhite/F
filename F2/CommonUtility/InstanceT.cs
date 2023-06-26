using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

// author  (hf) Date：2023/5/22 10:22:48
namespace F
{
    public static class InstanceT
    {

        private static List<Assembly> mAssemblies = new List<Assembly>();

        public static void AddAssembly(Assembly assembly)
        {
            if (!mAssemblies.Contains(assembly))
            {
                mAssemblies.Add(assembly);
            }
        }
        public static T CreateInstance<T>()
        {
            return Activator.CreateInstance<T>();
        }
        public static object CreateInstance(string v)
        {
            Type type = default;
            type = Assembly.GetExecutingAssembly().GetType(v);
            if (type == null)
            {
                foreach (var item in mAssemblies)
                {
                    type = item.GetType(v);
                    if (type != null)
                    {
                        break;
                    }
                }
            }
            var obj = Activator.CreateInstance(type) ?? type;
            if (obj == null)
            {
                throw new Exception($" CreateInstance not fount className:{v}");
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
