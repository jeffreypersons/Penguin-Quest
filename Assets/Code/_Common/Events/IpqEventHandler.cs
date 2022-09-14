using System;


namespace PQ.Common.Events
{
    public interface IpqEventHandler
    {
        public void AddHandler(Action onRaise);
        public void RemoveHandler(Action onRaise);
    }

    public interface IPqEventHandler<out T>
    {
        public void AddHandler(Action<T> onRaise);
        public void RemoveHandler(Action<T> onRaise);
    }
}
