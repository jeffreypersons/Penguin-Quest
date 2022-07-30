using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    public class PenguinStateDriver : FsmStateMachineDriver
    {
        private Vector2 initialSpawnPosition;
        private PlayerInputReceiver input;
        private PenguinBlob penguinBlob;

        // todo: replace with a cleaner, more reusable way to do this
        private FsmState upright;
        private FsmState onBelly;
        private bool CanEnterUprightState => !IsCurrently(upright);
        private bool CanEnterOnbellyState => !IsCurrently(onBelly);

        protected override void Initialize(FsmState initialState)
        {
            upright = new PenguinUprightState("Penguin.State.OnFeet");
            onBelly = new PenguinOnBellyState("Penguin.State.OnBelly");

            penguinBlob = gameObject.GetComponent<PenguinBlob>();
            initialSpawnPosition = penguinBlob.Rigidbody.position;
            ResetPositioning();


            base.Initialize(upright);
        }

        // todo: extract out a proper spawning system, or consider moving these to blob
        public void ResetPositioning()
        {
            penguinBlob.Rigidbody.velocity = Vector2.zero;
            penguinBlob.Rigidbody.position = initialSpawnPosition;
            penguinBlob.Rigidbody.transform.localEulerAngles = Vector3.zero;
        }


        protected override void ExecuteAnyTransitions()
        {
            /*
            Disable state changes until state subclasses are properly implemented
            if (CanEnterUprightState)
            {
                MoveToState(upright);
            }
            else if (CanEnterOnbellyState)
            {
                MoveToState(onBelly);
            }
            */
        }

        protected override void OnTransition(FsmState previous, FsmState next)
        {
            Debug.Log($"Transitioning Penguin from {previous} to {next}");
        }
    }
}
