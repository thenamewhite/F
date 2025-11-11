using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

// author  (hf) time：2023/2/16 10:38:20
namespace F
{
    /// <summary>
    /// 实现泛型监听派发
    /// </summary>
    public class EventListener
    {
        private readonly Dictionary<Type, object> _mKeyValuePairs = new Dictionary<Type, object>();

        private readonly Dictionary<Type, List<Listener>> _addCache = new Dictionary<Type, List<Listener>>();
        private readonly Dictionary<Type, List<Listener>> _removeCache = new Dictionary<Type, List<Listener>>();
        private bool _isDispatching;

        /// <summary>
        ///返回是否已添加监听函数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        /// <summary>
        /// </summary>
        public virtual bool IsHasAddListenerEvent<T>(Action<EventData<T>> action)
        {
            var type = typeof(T);
            if (_mKeyValuePairs.TryGetValue(type, out var t))
            {
                var listAction = (ListActionT<T>)t;
                foreach (var item in listAction.ListActions)
                    if (item.Action == action)
                        return true;
            }

            return false;
        }
        /// <summary>
        ///相同函数，相同类型只能注入一个
        /// </summary>
        /// <param name="action"></param>
        /// <param name="level"></param>
        /// <param name="isOnce"></param>
        public virtual void AddListenerEvent<T>(Action<EventData<T>> action, int level = 0, bool isOnce = false)
        {
            var type = typeof(T);
            _mKeyValuePairs.TryGetValue(type, out var t);
            var listAction = Unsafe.As<ListActionT<T>>(t);
            if (t == null)
            {
                listAction = new ListActionT<T> { };
                _mKeyValuePairs.Add(type, listAction);
            }
            else
            {
                foreach (var item in listAction.ListActions)
                    if (item.Action == action)
                        return;
            }

            if (_isDispatching)
            {
                if (!_addCache.TryGetValue(type, out var list))
                {
                    list = new List<Listener>();
                    _addCache[type] = list;
                }

                object tt = action;
                list.Add(new Listener()
                {
                    Action = action,
                    Level = level,
                    IsOnce = isOnce
                });
                return;
            }

            var eventT = new EventListenerData<T>(level, isOnce, action);
            var eventCount = listAction.ListActions.Count;
            //按照level 优先级排序 ,小在前，因为派发时候 是倒序派发
            for (var i = 0; i < eventCount; i++)
            {
                var eventLevel = listAction.ListActions[i].Level;
                if (level < eventLevel)
                {
                    listAction.ListActions.Insert(i, eventT);
                    return;
                }
            }

            listAction.ListActions.Add(eventT);
        }

        private void FlushPendingAdds()
        {
            ClearCache(_addCache, true);
            ClearCache(_removeCache);
        }

        private void ClearCache(Dictionary<Type, List<Listener>> data, bool isAdd = false)
        {
            foreach (var obj in data)
            {
                var t = obj.Value;
                foreach (var v in t)
                {
                    var d = _mKeyValuePairs[obj.Key] as IFEvent;
                    if (isAdd)
                    {
                        d.AddEvent(v);
                    }
                    else
                    {
                        d.RemoveEvent(v);
                    }
                }
            }

            foreach (var item in data)
            {
                item.Value.Clear();
            }

            data.Clear();
        }

        public virtual void RemoveListenerEvents(Type type)
        {
            if (_mKeyValuePairs.Remove(type, out var t)) (t as IDisposable).Dispose();
        }

        public virtual void RemoveListenerEvents<T>()
        {
            if (_mKeyValuePairs.TryGetValue(typeof(T), out var t)) (t as IDisposable).Dispose();
        }

        public virtual void RemoveListenerEvent<T>(Action<EventData<T>> action)
        {
            var type = typeof(T);

            if (_mKeyValuePairs.TryGetValue(type, out var t))
            {
                if (_isDispatching)
                {
                    if (!_removeCache.TryGetValue(type, out var list))
                    {
                        list = new List<Listener>();
                        _removeCache[type] = list;
                    }

                    list.Add(new Listener() { Action = action });
                    return;
                }

                var data = ((ListActionT<T>)t).ListActions;
                for (var i = 0; i < data.Count; i++)
                    if (data[i].Action == action)
                    {
                        data.RemoveAt(i);
                        break;
                    }
            }
        }


        public virtual void DispatchEvent<T>(T param)
        {
            var type = typeof(T);
            _isDispatching = true;
            if (_mKeyValuePairs.TryGetValue(type, out var t))
            {
                var e = Unsafe.As<ListActionT<T>>(t);
                var data = e.ListActions;
                var count = data.Count;
                var p = param;
                for (var i = count - 1; i >= 0; i--)
                {
                    if (i >= data.Count) continue;

                    var d = data[i];
                    e.Value = p;
                    try
                    {
                        d.Action(e);
                    }
                    catch (Exception err)
                    {
                        throw new Exception($"{err.StackTrace},{err}");
                    }
                    finally
                    {
                        if (d.IsOnce) data.Remove(d);
                    }

                    if (e.IsStopImmediatePropagation)
                    {
                        e.IsStopImmediatePropagation = false;
                        break;
                    }
                }
            }

            FlushPendingAdds();
            _isDispatching = false;
        }

        /// <summary>
        ///清理所有事件
        /// </summary>
        public void ClearEvent()
        {
            foreach (var t in _mKeyValuePairs.Values)
            {
                (t as IDisposable).Dispose();
            }

            _mKeyValuePairs.Clear();
        }



    }
}