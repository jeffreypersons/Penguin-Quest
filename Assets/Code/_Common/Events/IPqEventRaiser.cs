

namespace PQ.Common.Events
{
    public interface IPqEventRaiser
    {
        public string Name { get; }
        public void Raise();
    }

    public interface IPqEventRaiser<in T>
    {
        public string Name { get; }
        public void Raise(T args);
    }
}
