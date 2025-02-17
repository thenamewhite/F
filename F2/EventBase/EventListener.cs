﻿using System;
using System.Collections.Generic;

// author  (hf) time：2023/2/16 10:38:20
namespace F
{
    /// <summary>
    ///     实现泛型监听派发
    /// </summary>
    public class EventListener
    {
        private readonly Dictionary<Type, IDisposable> mKeyValuePairs = new Dictionary<Type, IDisposable>();

        /// <summary>
        ///     返回是否已添加监听函数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        /// <summary>
        /// </summary>
        public virtual bool IsHasAddListenerEvent<T>(Action<EventListenerData<T>> action) where T : struct
        {
            var type = typeof(T);
            if (mKeyValuePairs.TryGetValue(type, out var t))
            {
                var listAction = (ListActionT<T>)t;
                foreach (var item in listAction.ListActions)
                    if (item.Action == action)
                        return true;
            }

            return false;
        }

        /// <summary>
        ///     相同函数，相同类型只能注入一个
        /// </summary>
        /// <param name="action"></param>
        /// <param name="level"></param>
        /// <param name="isOnce"></param>
        public virtual void AddListenerEvent<T>(Action<EventListenerData<T>> action, int level = 0, bool isOnce = false)
            where T : struct
        {
            var type = typeof(T);
            mKeyValuePairs.TryGetValue(type, out var t);
            var listAction = (ListActionT<T>)t;

            if (t == null)
            {
                listAction = new ListActionT<T> { ListActions = new List<EventListenerData<T>>(4) };
                mKeyValuePairs.Add(type, listAction);
            }
            else
            {
                foreach (var item in listAction.ListActions)
                    if (item.Action == action)
                        return;
            }

            var eventT = new EventListenerData<T>(level, isOnce, action);
            var eventCount = listAction.ListActions.Count;
            //按照level 优先级排序
            for (var i = 0; i < eventCount; i++)
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

        public virtual void RemoveListenerEvents(Type type)
        {
            if (mKeyValuePairs.TryGetValue(type, out var t)) t.Dispose();
        }

        public virtual void RemoveListenerEvents<T>() where T : struct
        {
            if (mKeyValuePairs.TryGetValue(typeof(T), out var t)) t.Dispose();
        }

        public virtual void RemoveListenerEvent<T>(Action<EventListenerData<T>> action) where T : struct
        {
            var type = typeof(T);
            if (mKeyValuePairs.TryGetValue(type, out var t))
            {
                var data = ((ListActionT<T>)t).ListActions;
                for (var i = 0; i < data.Count; i++)
                    if (data[i].Action == action)
                    {
                        data.RemoveAt(i);
                        break;
                    }
            }
        }

        public virtual void DispatchEvent<T>(T param) where T : struct
        {
            var type = typeof(T);
            if (mKeyValuePairs.TryGetValue(type, out var t))
            {
                var data = ((ListActionT<T>)t).ListActions;
                var count = data.Count;
                for (var i = count - 1; i >= 0; i--)
                {
                    if (i >= data.Count) continue;

                    var d = data[i];
                    d.Value = param;
                    try
                    {
                        d.Action(d);
                    }
                    catch (Exception err)
                    {
                        throw new Exception($"{err.StackTrace},{err}");
                    }
                    finally
                    {
                        if (d.IsOnce) data.Remove(d);
                    }
                    //if (d.IsBreak)
                    //{
                    //    break;
                    //}
                }
            }
        }

        //public virtual void DispatchEvent<T>(ref T param) where T : struct
        //{
        //    var type = typeof(T);
        //    if (mKeyValuePairs.TryGetValue(type, out var t))
        //    {
        //        var data = ((ListActionT<T>)t).ListActions;
        //        var count = data.Count;
        //        for (int i = count - 1; i >= 0; i--)
        //        {
        //            if (i >= data.Count)
        //            {
        //                continue;
        //            }
        //            var d = data[i];
        //            d.Value = param;
        //            try
        //            {
        //                d.Action(d);
        //            }
        //            catch (System.Exception err)
        //            {
        //                throw new Exception($"{err.StackTrace},{err}");
        //            }
        //            finally
        //            {
        //                if (d.IsOnce)
        //                {
        //                    data.Remove(d);
        //                }
        //            }
        //            //if (d.IsBreak)
        //            //{
        //            //    break;
        //            //}
        //        }
        //    }
        //}
        /// <summary>
        ///     清理所有事件
        /// </summary>
        public void ClearEvent()
        {
            mKeyValuePairs.Clear();
        }

        private class ListActionT<T> : IDisposable where T : struct
        {
            public List<EventListenerData<T>> ListActions { get; set; }

            public void Dispose()
            {
                ListActions?.Clear();
            }
        }
    }
}