using System;


namespace PQ.Common
{
    /*
    Collection of states and transitions between them.
    */
    public class FsmStateGraph
    {
        private readonly FsmState[] _states;

        public readonly string Name;
        public override string ToString() => Name;

        public FsmStateGraph(string name, params FsmState[] states)
        {
            if (states == null || states.Length < 2)
            {
                throw new ArgumentException($"Fsm State Graph must have at least two states - received {states} instead");
            }

            Name = name;
            _states = states;
        }
    }
}
