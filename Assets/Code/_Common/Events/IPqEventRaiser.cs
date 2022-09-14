

namespace PQ.Common.Events
{
    public interface IPqEventRaiser
    {
        public void Raise();
    }

    public interface IPqEventRaiser<in T>
    {
        public void Raise(T args);
    }
}
