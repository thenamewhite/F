using System;
using System.Collections.Generic;

namespace F
{
    public abstract class EventData<T>
    {
        internal bool IsStopImmediatePropagation;
        public T Value;

        public abstract void StopImmediatePropagation();
        //abstract internal void RemoveHandler();

        //abstract internal void AddHandler();
    }

    internal class ListActionT<T> : EventData<T>, IFEvent
    {
        public List<EventListenerData<T>> ListActions { get; private set; } = new List<EventListenerData<T>>(4);

        public void Dispose()
        {
            ListActions?.Clear();
        }

        public override void StopImmediatePropagation()
        {
            IsStopImmediatePropagation = true;
        }


        public void AddEvent(Listener action)
        {
            var eventT = new EventListenerData<T>(action.Level, action.IsOnce, action.Action) { };
            AddEvent(eventT);
        }

        public void AddEvent(EventListenerData<T> eventT)
        {
            var eventCount = ListActions.Count;
            //按照level 优先级排序 ,小在前，因为派发时候 是倒序派发
            for (var i = 0; i < eventCount; i++)
            {
                var eventLevel = ListActions[i].Level;
                if (eventT.Level < eventLevel)
                {
                    ListActions.Insert(i, eventT);
                    return;
                }
            }

            ListActions.Add(eventT);
        }

        public bool IsHasAddEventListener(Action<EventData<T>> action)
        {
            foreach (var v in ListActions)
            {
                if (v.Action == action)
                {
                    return true;
                }
            }
            return false;
        }
        public virtual void DispatchEvent(T param)
        {
            var data = ListActions;
            var count = data.Count;
            var p = param;
            for (var i = count - 1; i >= 0; i--)
            {
                if (i >= data.Count) continue;
                var d = data[i];
                Value = p;
                try
                {
                    d.Action(this);
                }
                catch (Exception err)
                {
                    throw new Exception($"{err.StackTrace},{err}");
                }
                finally
                {
                    if (d.IsOnce) data.Remove(d);
                }
                if (IsStopImmediatePropagation)
                {
                    IsStopImmediatePropagation = false;
                    break;
                }
            }
        }

        public void RemoveEvent(Delegate action)
        {
            foreach (var t in ListActions)
            {
                if (t.Action == (Action<EventData<T>>)action)
                {
                    ListActions.Remove(t);
                    break;
                }
            }
        }
    }
}