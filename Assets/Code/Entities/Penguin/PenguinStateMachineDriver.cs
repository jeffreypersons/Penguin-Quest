using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    /*
    switch state
        feet   [liedown signal] -> liedown
        lie    [finish  signal] -> belly
        belly  [standup signal] -> standup
        stand  [finish  signal] -> feet
    */
    public class PenguinStateMachineDriver : FsmStateMachineDriver
    {
        private PlayerGameplayInputReceiver input;
        private PenguinBlob penguinBlob;

        private Vector2 initialSpawnPosition;

        // todo: replace with a cleaner, more reusable way to do this
        private FsmState stateFeet;
        private FsmState stateBelly;
        private FsmState stateStandingUp;
        private FsmState stateLyingDown;

        protected override void OnTransition(FsmState previous, FsmState next)
        {
            Debug.Log($"Transitioning Penguin from {previous} to {next}");
        }

        protected override void Initialize(FsmState initialState)
        {
            penguinBlob = gameObject.GetComponent<PenguinBlob>();
            initialSpawnPosition = penguinBlob.Rigidbody.position;
            ResetPositioning();

            // todo: this should be in state machine for onFeet and we should start in a blank state and then
            //       entered rather than assuming we start onFeet here...
            gameObject.GetComponent<CharacterController2D>().Settings = penguinBlob.OnFeetSettings;

            stateFeet       = new PenguinStateOnFeet   ("Penguin.State.OnFeet",     penguinBlob);
            stateBelly      = new PenguinStateOnBelly  ("Penguin.State.OnBelly",    penguinBlob);
            stateStandingUp = new PenguinStateOnFeet   ("Penguin.State.StandingUp", penguinBlob);
            stateLyingDown  = new PenguinStateLyingDown("Penguin.State.LyingDown",  penguinBlob);
            
            base.Initialize(stateFeet);
        }

        // todo: try integrating events into state machine at framework level
        private void OnEnable()
        {
            GameEventCenter.lieDownCommand.AddListener(OnLieDownInputReceived);
            GameEventCenter.standUpCommand.AddListener(OnStandUpInputReceived);
        }
        private void OnDisable()
        {
            GameEventCenter.lieDownCommand.RemoveListener(OnLieDownInputReceived);
            GameEventCenter.standUpCommand.RemoveListener(OnStandUpInputReceived);
        }

        private void OnLieDownInputReceived(string _)
        {
            if (IsCurrently(stateFeet))
            {
                MoveToState(stateLyingDown);
            }
        }
        private void OnStandUpInputReceived(string _)
        {
            if (IsCurrently(stateBelly))
            {
                MoveToState(stateStandingUp);
            }
        }

        // todo: extract out a proper spawning system, or consider moving these to blob
        public void ResetPositioning()
        {
            penguinBlob.Rigidbody.velocity = Vector2.zero;
            penguinBlob.Rigidbody.position = initialSpawnPosition;
            penguinBlob.Rigidbody.transform.localEulerAngles = Vector3.zero;
        }
    }
}
