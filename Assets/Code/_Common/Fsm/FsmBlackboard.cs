using System;
using UnityEngine;


namespace PQ.Common.Fsm
{
    /*
    Generic, persistent blob of components that can be used by state machine.
    */
    public class FsmBlackboard<T>
        where T : FsmBlackboardData
    {
        private readonly FsmBlackboardData _data;

        public FsmBlackboardData Data => Data;

        public override string ToString() => _data.ToString();

        public FsmBlackboard(T data)
        {
            if (data == null)
            {
                throw new ArgumentNullException($"Cannot set fsm blackboard data to null");
            }

            _data = data;
        }
    }
}
