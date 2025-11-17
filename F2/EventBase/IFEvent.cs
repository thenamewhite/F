using System;
using System.Collections.Generic;
using System.Text;

namespace F
{
    internal interface IFEvent : IDisposable
    {
        void AddEvent(Listener action);
        void RemoveEvent(Delegate actionHaseCode);
    }
    internal struct Listener
    {
        public int Level;
        public bool IsOnce;
        public Delegate Action;
    }
}
