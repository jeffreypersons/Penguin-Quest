

namespace PQ
{
    public struct HorizontalInput
    {
        public enum Type { None, Left, Right }
        public readonly Type value;
        public HorizontalInput(Type value)
        {
            this.value = value;
        }
    }
}
