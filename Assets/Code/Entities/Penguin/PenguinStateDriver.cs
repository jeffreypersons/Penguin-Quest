using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    public class PenguinStateDriver : FsmStateMachineDriver
    {
        private PlayerGameplayInputReceiver input;
        private PenguinBlob penguinBlob;

        private Vector2 initialSpawnPosition;

        // todo: replace with a cleaner, more reusable way to do this
        private FsmState onFeet;
        private FsmState onBelly;
        private bool CanEnterOnFeetState => !IsCurrently(onFeet);
        private bool CanEnterOnbellyState => !IsCurrently(onBelly);

        protected override void Initialize(FsmState initialState)
        {
            onFeet = new PenguinStateOnFeet("Penguin.State.OnFeet");
            onBelly = new PenguinStateOnBelly("Penguin.State.OnBelly");

            penguinBlob = gameObject.GetComponent<PenguinBlob>();
            initialSpawnPosition = penguinBlob.Rigidbody.position;
            ResetPositioning();


            base.Initialize(onFeet);
        }

        private void OnEnable()
        {
            penguinBlob.CharacterController.OnGroundContactChanged += OnGroundedPropertyChanged;
        }
        private void OnDisable()
        {
            
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
            if (CanEnterOnFeetState)
            {
                MoveToState(onFeet);
            }
            else if (CanEnterOnFeetState)
            {
                MoveToState(onBelly);
            }
            */
        }

        protected override void OnTransition(FsmState previous, FsmState next)
        {
            Debug.Log($"Transitioning Penguin from {previous} to {next}");
        }

        protected void OnGroundedPropertyChanged(bool isGrounded)
        {
            penguinBlob.Animation.SetParamIsGrounded(isGrounded);
        }
    }
}
