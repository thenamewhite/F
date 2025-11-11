using System;
using System.Collections.Generic;
using System.Text;

namespace F
{
    internal interface IFEvent : IDisposable
    {
        void AddEvent(Listener action);
        void RemoveEvent(Listener action);
    }
    internal struct Listener
    {
        public int Level;
        public bool IsOnce;
        public object Action;
    }
}
