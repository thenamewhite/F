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
    }
}
