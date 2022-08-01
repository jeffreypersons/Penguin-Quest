﻿using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    // todo: overhaul the placeholder code, and move things to states more
    public class PenguinStateMachineDriver : FsmStateMachineDriver
    {
        private PlayerGameplayInputReceiver input;
        private PenguinBlob penguinBlob;

        private Vector2 initialSpawnPosition;

        // todo: replace with a cleaner, more reusable way to do this
        private FsmState onFeet;
        private FsmState onBelly;
        private bool CanEnterOnFeetState => !IsCurrently(onFeet);
        private bool CanEnterOnbellyState => !IsCurrently(onBelly);


        [SerializeField] private CharacterController2DSettings initialSettings;

        protected override void Initialize(FsmState initialState)
        {
            penguinBlob = gameObject.GetComponent<PenguinBlob>();
            initialSpawnPosition = penguinBlob.Rigidbody.position;
            ResetPositioning();

            // todo: this should be in state machine for onFeet and we should start in a blank state and then
            //       entered rather than assuming we start onFeet here...
            gameObject.GetComponent<CharacterController2D>().Settings = initialSettings;

            onFeet  = new PenguinStateOnFeet ("Penguin.State.OnFeet",  penguinBlob);
            onBelly = new PenguinStateOnBelly("Penguin.State.OnBelly", penguinBlob);
            base.Initialize(onFeet);
        }

        private void OnEnable()
        {
            // penguinBlob.CharacterController.OnGroundContactChanged += OnGroundedPropertyChanged;
        }
        private void OnDisable()
        {
            // penguinBlob.CharacterController.OnGroundContactChanged -= OnGroundedPropertyChanged;
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
