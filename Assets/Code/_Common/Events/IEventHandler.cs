using System;


namespace PQ.Common.Events
{
    public interface IEventHandler
    {
        public void AddHandler(Action onRaise);
        public void RemoveHandler(Action onRaise);
    }

    public interface IEventHandler<out T>
    {
        public void AddHandler(Action<T> onRaise);
        public void RemoveHandler(Action<T> onRaise);
    }
}
