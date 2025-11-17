using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// author  (hf) time：2023/2/16 10:38:20
namespace F
{
    /// <summary>
    /// 实现泛型监听派发
    /// </summary>
    public class EventListener
    {
        protected readonly Dictionary<Type, object> MKeyValuePairs = new Dictionary<Type, object>();

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
        public virtual bool IsHasAddEventListener<T>(Action<EventData<T>> action)
        {
            var type = typeof(T);
            if (MKeyValuePairs.TryGetValue(type, out var t))
            {
                var listAction = (ListActionT<T>)t;
                return listAction.IsHasAddEventListener(action);
            }

            return false;
        }

        /// <summary>
        ///相同函数，相同类型只能注入一个
        /// </summary>
        /// <param name="action"></param>
        /// <param name="level"></param>
        /// <param name="isOnce"></param>
        public virtual bool AddEventListener<T>(Action<EventData<T>> action, int level = 0, bool isOnce = false)
        {
            var type = typeof(T);
            MKeyValuePairs.TryGetValue(type, out var t);
            var listAction = Unsafe.As<ListActionT<T>>(t);
            if (t == null)
            {
                listAction = new ListActionT<T> { };
                MKeyValuePairs.Add(type, listAction);
            }
            else
            {
                if (listAction.IsHasAddEventListener(action))
                {
                    return false;
                }
            }

            if (_isDispatching)
            {
                if (!_addCache.TryGetValue(type, out var list))
                {
                    list = new List<Listener>();
                    _addCache[type] = list;
                }

                list.Add(new Listener()
                {
                    Action = action,
                    Level = level,
                    IsOnce = isOnce
                });
                return true;
            }

            listAction.AddEvent(new EventListenerData<T>(level, isOnce, action));
            return true;
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
                ///后续优化下,不频繁创建list
                foreach (var v in t)
                {
                    var e = MKeyValuePairs[obj.Key] as IFEvent;
                    if (isAdd)
                    {
                        e.AddEvent(v);
                    }
                    else
                    {
                        e.RemoveEvent(v.Action);
                    }
                }

                obj.Value.Clear();
            }

            data.Clear();
        }

        public virtual void RemoveEventListeners(Type type)
        {
            if (MKeyValuePairs.TryGetValue(type, out var t)) (t as IDisposable).Dispose();
        }

        public virtual void RemoveEventListeners<T>()
        {
            if (MKeyValuePairs.TryGetValue(typeof(T), out var t)) (t as IDisposable).Dispose();
        }

        public virtual void RemoveEventListener<T>(Action<EventData<T>> action)
        {
            var type = typeof(T);
            RemoveEventListener(type, action);
        }

        public void RemoveEventListener<T>(Delegate action)
        {
            var type = typeof(T);
            RemoveEventListener(type, action);
        }

        public void RemoveEventListener(Type type, Delegate action)
        {
            if (MKeyValuePairs.TryGetValue(type, out var t))
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

                var data = t as IFEvent;
                data.RemoveEvent(action);
            }
        }


        public virtual void DispatchEvent<T>(T param)
        {
            var type = typeof(T);
            var isOldDispatching = _isDispatching;
            _isDispatching = true;
            if (MKeyValuePairs.TryGetValue(type, out var t))
            {
                var e = Unsafe.As<ListActionT<T>>(t);
                e.DispatchEvent(param);
            }
            if (!isOldDispatching)
            {
                FlushPendingAdds();
            }
            _isDispatching = isOldDispatching;
        }

        /// <summary>
        ///清理所有事件
        /// </summary>
        public void ClearEvent()
        {
            foreach (var t in MKeyValuePairs.Values)
            {
                (t as IDisposable).Dispose();
            }

            MKeyValuePairs.Clear();
        }
    }
}