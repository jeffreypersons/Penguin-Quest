using System;


namespace PQ.Common.States
{
    /*
    Provides info about the state machine - the states, history.
    */
    public class FsmContext
    {
        private FsmState _initial;
        private FsmState _current;
        private FsmState _last;
        private FsmState _next;

        public FsmState InitialState  => _initial;
        public FsmState CurrentState  => _current;
        public FsmState PreviousState => _last;

        public FsmState NextState => _next;

        public override string ToString() =>
            $"FsmContext:{{" +
                $"history(" +
                    $"initial:{InitialState.Name}," +
                    $"current:{CurrentState.Name}," +
                    $"previous:{PreviousState.Name})}}";


        public FsmContext(FsmState initial)
        {
            if (initial == null)
            {
                throw new ArgumentException("Fsm state cannot be null");
            }

            _initial = initial;
            _current = initial;
            _last    = null;
            _next    = null;
    }

        // schedule the next state, while strictly enforcing only one item in queue at a time
        public void PushNextState(FsmState next)
        {
            if (next == null)
            {
                throw new ArgumentNullException("Cannot push a null state");
            }

            if (_next != null)
            {
                throw new InvalidOperationException("Cannot push another state - another state is already queued");
            }

            _next = next;
        }

        // process the next state, while strictly enforcing popping to when there is an item in the queue
        public FsmState PopNextState()
        {
            if (_next == null)
            {
                throw new InvalidOperationException("Cannot pop state - no state found in queue");
            }

            FsmState nextState = _next;
            _next = null;
            return nextState;
        }
    }
}
