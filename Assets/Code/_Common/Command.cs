using System;


namespace PQ.Common
{
    // todo: once we have C# 10, we can use a global using to hide the no op parameter specialization like
    // global using Command = Command<EmptyCommandArgs>;
    public struct EmptyArgs { }

    /*
    Simple one slot only queue, that takes input if requested and hands it off to caller
    when Process is called.
    */
    public class Command<Args>
    {
        private bool _requestPending;
        private Args _requestArgs;
        private Action<Args> _onExecute;

        public Command(Action onExecute)
        {
            // unfortunately, there's no easy way to specialize params unlike in c++ for this
            if (typeof(Args) != typeof(EmptyArgs))
            {
                throw new ArgumentException("Expected arguments as registered type Args != EmptyArgs");
            }

            Reset();
            _onExecute = (_) => onExecute.Invoke();
        }

        public Command(Action<Args> onExecute)
        {
            // unfortunately, there's no easy way to specialize params unlike in c++ for this
            if (typeof(Args) == typeof(EmptyArgs))
            {
                throw new ArgumentException("Expected no arguments as registered type Args == EmptyArgs");
            }

            Reset();
            _onExecute = onExecute;
        }

        public void Reset()
        {
            _requestPending = false;
            _requestArgs = default;
        }

        public void Request()
        {
            // unfortunately, there's no easy way to specialize params unlike in c++ for this
            if (typeof(Args) != typeof(EmptyArgs))
            {
                throw new ArgumentException("Expected arguments as registered type Args != EmptyArgs");
            }

            _requestPending = true;
            _requestArgs = default;
        }

        public void Request(Args args)
        {
            // unfortunately, there's no easy way to specialize params unlike in c++ for this
            if (typeof(Args) == typeof(EmptyArgs))
            {
                throw new ArgumentException("Expected no arguments as registered type Args == EmptyArgs");
            }

            _requestPending = true;
            _requestArgs = args;
        }

        public bool ExecuteIfRequested()
        {
            if (!_requestPending)
            {
                return false;
            }

            Reset();
            _onExecute.Invoke(_requestArgs);
            return true;
        }
    }
}
