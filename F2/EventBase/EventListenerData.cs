using System;

namespace F
{
    public struct EventListenerData<T> : IEquatable<EventListenerData<T>> where T : struct
    {
        public T Value;

        ///// <summary>
        ///// 是否退出后续触发
        ///// </summary>
        //public bool IsBreak;
        internal readonly int Level;
        internal readonly bool IsOnce;
        internal readonly Action<EventListenerData<T>> Action;

        public EventListenerData(int level, bool isOnce, Action<EventListenerData<T>> action)
        {
            //this.IsBreak = isBreak;
            Level = level;
            IsOnce = isOnce;
            Action = action;
            Value = default;
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