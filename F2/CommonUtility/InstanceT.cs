using System;
using System.Collections.Generic;
using System.Text;

// author  (hf) Date：2023/5/22 10:22:48
namespace F
{
    public static class InstanceT
    {

        public static T CreateInstance<T>()
        {
            return Activator.CreateInstance<T>();
        }
    }
}
