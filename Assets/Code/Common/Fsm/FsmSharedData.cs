using UnityEngine;


namespace PQ.Common.Fsm
{
    /*
    Generic, persistent blob of components that can be used by state machine.
    */
    [ExecuteAlways]
    public abstract class FsmSharedData : MonoBehaviour
    {
        public override string ToString() => $"{GetType()}";
    }
}
