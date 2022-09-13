

namespace PQ.Common.Events
{
    public interface IEventRaiser
    {
        public void Raise();
    }

    public interface IEventRaiser<in T>
    {
        public void Raise(T args);
    }
}
