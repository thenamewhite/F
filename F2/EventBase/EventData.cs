using F;
using System;
using System.Collections.Generic;
using System.Text;

namespace F
{
    public abstract class EventData<T>
    {
        internal bool IsStopImmediatePropagation;
        public T Value;

        public abstract void StopImmediatePropagation();
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
            ListActions.Add(new EventListenerData<T>(action.Level, action.IsOnce, action.Action) { });
        }

        public void AddEvent(EventListenerData<T> action)
        {
            ListActions.Add(action);
        }

        public void RemoveEvent(EventListenerData<T> action)
        {
            foreach (var v in ListActions)
            {
                if (v.Action == action.Action)
                {
                    ListActions.Remove(v);
                    break;
                }
            }
        }

        public void RemoveEvent(Listener action)
        {
            foreach (var t in ListActions)
            {
                if (t.Action == (Action<EventData<T>>)action.Action)
                {
                    ListActions.Remove(t);
                    break;
                }
            }
        }
    }
}
