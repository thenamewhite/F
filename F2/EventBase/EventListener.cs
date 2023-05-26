using System.Collections.Generic;
using System;
using static F.EventListener;
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
        private struct ListActionT<T> : IList
        {
            public List<Event<T>> ListActions;
        }

        public delegate void ActionRef<T1>(ref T1 arg1);

        public struct Event<T>
        {
            public T Value;
            /// <summary>
            /// 是否退出后续触发
            /// </summary>
            public bool IsBreak;
            public readonly int Level;
            public readonly bool IsOnce;
            public readonly ActionRef<Event<T>> Action;

            public Event(bool isBreak, int level, bool isOnce, ActionRef<Event<T>> action)
            {
                this.IsBreak = isBreak;
                this.Level = level;
                this.IsOnce = isOnce;
                this.Action = action;
                this.Value = default;
            }
        }

        private Dictionary<Type, IList> mKeyValuePairs = new Dictionary<Type, IList>();


        /// <summary>
        /// 相同函数，相同类型只能注入一个
        /// </summary>
        /// <param name="type"></param>
        /// <param name="level"></param>
        /// <param name="isOnce"></param>
        public void AddEvent<T>(ActionRef<Event<T>> action, int level = 0, bool isOnce = false)
        {
            var type = typeof(T);
            mKeyValuePairs.TryGetValue(type, out var t);
            if (t == null)
            {
                t = new ListActionT<T>() { ListActions = new List<Event<T>>() { } };
                mKeyValuePairs.Add(type, t);
            }
            var listAction = (ListActionT<T>)t;
            foreach (var item in listAction.ListActions)
            {
                if (item.Action == action)
                {
                    return;
                }
            }
            var eventT = new Event<T>(false, level, isOnce, action);
            var eventCount = listAction.ListActions.Count;
            //按照level 优先级排序
            for (int i = 0; i < eventCount; i++)
            {
                var eventLevel = listAction.ListActions[i].Level;
                if (level >= eventLevel)
                {
                    listAction.ListActions.Insert(i, eventT);
                    return;
                }
            }
            listAction.ListActions.Add(eventT);
        }

        public void RemoveEvent<T>(ActionRef<Event<T>> action)
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
                    var d = data[i];
                    data[i].Action(ref d);
                    if (i >= data.Count)
                    {
                        continue;
                    }
                    if (data[i].IsOnce)
                    {
                        data.RemoveAt(i);
                    }
                    if (d.IsBreak)
                    {
                        break;
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
