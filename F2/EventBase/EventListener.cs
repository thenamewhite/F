using System.Collections.Generic;
using System;
// author  (hf) time：2023/2/16 10:38:20
namespace F
{
    /// <summary>
    /// 实现泛型监听派发 
    /// </summary>
    public class EventListener
    {
        private interface IList
        {
        }
        private struct EventT<T>
        {
            public int Level;
            public bool IsOnce;
            public ActionRef<T> Action;
        }
        private struct ListActionT<T> : IList
        {
            public List<EventT<T>> ListActions;
        }

        public delegate void ActionRef<T1>(ref T1 arg1);

        //public unsafe delegate* managed  ActionRef<T1>(ref T1 arg1);

        private Dictionary<Type, IList> mKeyValuePairs = new Dictionary<Type, IList>();

        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="level"></param>
        /// <param name="isOnce"></param>
        public void AddEvent<T>(ActionRef<T> action, int level = 0, bool isOnce = false)
        {
            var type = typeof(T);
            mKeyValuePairs.TryGetValue(type, out var t);
            if (t == null)
            {
                t = new ListActionT<T>() { ListActions = new List<EventT<T>>() { } };
                mKeyValuePairs.Add(type, t);
            }
            var listAction = (ListActionT<T>)t;
            listAction.ListActions.Add(new EventT<T>() { Action = action, Level = level, IsOnce = isOnce });
        }

        public void RemoveEvent<T>(ActionRef<T> action)
        {
            var type = typeof(T);
            if (mKeyValuePairs.TryGetValue(type, out var t))
            {
                var data = ((ListActionT<T>)t).ListActions;
                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i].Action == action)
                    {
                        data.Remove(data[i]);
                        break;
                    }
                }
            }
        }
        public virtual void DispatchEvent<T>(ref T param)
        {
            var type = typeof(T);
            if (mKeyValuePairs.TryGetValue(type, out var t))
            {
                var data = ((ListActionT<T>)t).ListActions;
                var count = data.Count;
                for (int i = count - 1; i >= 0; i--)
                {
                    data[i].Action(ref param);
                    if (i >= data.Count)
                    {
                        continue;
                    }
                    if (data[i].IsOnce)
                    {
                        data.RemoveAt(i);
                    }
                }
            }
        }
        /// <summary>
        /// 清理所有事件
        /// </summary>
        public void ClearEvent()
        {
            mKeyValuePairs.Clear();
        }
    }
}
