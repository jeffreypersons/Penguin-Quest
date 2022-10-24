using UnityEngine;


namespace PQ.Common.Physics.MovementDriver
{
    /*
    Driver for movement.

    Meant to queue up 'movement input' and feed things into Unity's physics engine at the right time.
    */
    public abstract class MovementDriver : MonoBehaviour
    {
        public override string ToString() => $"{GetType()}(gameObject:{base.name})";


        /*** Internal Hooks for Setting up a Specific State Machine Instance ***/



        /*** Internal Hooks to MonoBehavior ***/

        private void Awake()
        {
            // no op
        }

        private void Start()
        {
            // no op
        }

        private void FixedUpdate()
        {
            // no op
        }

        private void OnAnimatorMove()
        {
            // no op
        }

        private void OnAnimatorIK(int layerIndex)
        {
            // no op
        }

        private void Update()
        {
            // no op
        }

        private void LateUpdate()
        {
            // no op
        }
    }
}
