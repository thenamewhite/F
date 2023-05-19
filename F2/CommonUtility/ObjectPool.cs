using System;
using System.Collections.Generic;
// author  (hf) time：2023/2/16 11:20:19
namespace F
{
    /// <summary>
    /// 简单的对象池管理,
    /// T 如果实现IInitialization 接口，重新初始化会调用接口
    /// 如果不是可以自定义点监听InItFun
    /// </summary>
    public class ObjectPool<T> where T : class
    {
        /// <summary>
        /// 获取对象重置函数
        /// </summary>
        public Action InItFun;
        /// <summary>
        /// 自定义对象实例化对象
        /// </summary>
        public Func<T> DefinitionObjFun;

        private Stack<T> mStack;

        public ObjectPool(int count = 8)
        {
            mStack = new Stack<T>(count);
        }
        ~ObjectPool()
        {
            mStack.Clear();
        }

        public T New()
        {
            T obj;
            if (mStack.Count == 0)
            {
                obj = DefinitionObjFun != null ? DefinitionObjFun() : Activator.CreateInstance<T>();
            }
            else
            {
                obj = mStack.Pop();
            }
            var I = obj as IInitialization;
            if (I != null)
            {
                //调用初始化接口
                I.Initialization();
            }
            else
            {
                InItFun?.Invoke();
            }
            return obj;
        }

        public void Release(T obj)
        {
            var I = obj as IInitialization;
            if (I != null)
            {
                I.Release();
            }
            mStack.Push(obj);
        }

        public void Clear()
        {
            mStack.Clear();
        }

        public T[] ToArry()
        {
            return mStack.ToArray();
        }
    }
}