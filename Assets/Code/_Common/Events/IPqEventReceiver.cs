using System;


namespace PQ.Common.Events
{
    public interface IPqEventReceiver
    {
        public string Name { get; }
        public void AddHandler(Action onRaise);
        public void RemoveHandler(Action onRaise);
    }

    public interface IPqEventReceiver<out T>
    {
        public string Name { get; }
        public void AddHandler(Action<T> onRaise);
        public void RemoveHandler(Action<T> onRaise);
    }
}
