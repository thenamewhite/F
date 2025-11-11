using System;

namespace F
{
    internal struct EventListenerData<T> : IEquatable<EventListenerData<T>>
    {
        internal readonly int Level;
        internal readonly bool IsOnce;
        internal readonly Action<EventData<T>> Action;
        public EventListenerData(int level, bool isOnce, Action<EventData<T>> action)
        {
            Level = level;
            IsOnce = isOnce;
            Action = action;
        }

        public EventListenerData(int level, bool isOnce, object action)
        {
            Level = level;
            IsOnce = isOnce;
            Action = (Action<EventData<T>>)action;
        }


        public bool Equals(EventListenerData<T> other)
        {
            return other.Action == Action;
        }

        // public override int GetHashCode()
        // {
        //     //unchecked
        //     //{
        //     //    // var hashCode = Value.GetHashCode();
        //     //    // hashCode = (hashCode * 397) ^ Level;
        //     //    // hashCode = (hashCode * 397) ^ IsOnce.GetHashCode();
        //     //    // hashCode = (hashCode * 397) ^ (Action != null ? Action.GetHashCode() : 0);
        //     //    // return hashCode;
        //     // 
        //     //}
        //     return base.GetHashCode();
        // }
    }
}