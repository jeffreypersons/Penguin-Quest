using UnityEngine;


namespace PQ.Common.Fsm
{
    /*
    Generic, persistent blob of components that intended as a blackboard for the finite state machine.
    */
    [ExecuteAlways]
    public abstract class FsmSharedData : MonoBehaviour
    {
        public override string ToString() => $"{GetType()}";
    }
}
