using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    [System.Flags]
    public enum PenguinFsmParams
    {
        Uninitialized = 0,
        Grounded      = 1 << 1,
        Upright       = 1 << 2,
        Moving        = 1 << 3,
    }

    public class StateMachineContext
    {
        public FsmState<PenguinFsmParams> stateFeet;
        public FsmState<PenguinFsmParams> stateBelly;
        public FsmState<PenguinFsmParams> stateStandingUp;
        public FsmState<PenguinFsmParams> stateLyingDown;
    }

    /*
    switch state
        feet   [liedown signal] -> liedown
        lie    [finish  signal] -> belly
        belly  [standup signal] -> standup
        stand  [finish  signal] -> feet
    */
    public class PenguinStateMachineDriver : FsmStateMachineDriver<PenguinFsmParams>
    {
        private PlayerGameplayInputReceiver input;
        private PenguinBlob penguinBlob;

        private Vector2 initialSpawnPosition;

        // todo: replace with a cleaner, more reusable way to do this
        private FsmState<PenguinFsmParams> stateFeet;
        private FsmState<PenguinFsmParams> stateBelly;
        private FsmState<PenguinFsmParams> stateStandingUp;
        private FsmState<PenguinFsmParams> stateLyingDown;

        private PenguinFsmParams fsmParams = PenguinFsmParams.Uninitialized;

        protected override void OnTransition(FsmState<PenguinFsmParams> previous, FsmState<PenguinFsmParams> next)
        {
            Debug.Log($"Transitioning Penguin from {previous} to {next}");
        }

        protected override void Initialize(FsmState<PenguinFsmParams> initialState, ref PenguinFsmParams a)
        {
            penguinBlob = gameObject.GetComponent<PenguinBlob>();
            initialSpawnPosition = penguinBlob.Rigidbody.position;
            ResetPositioning();

            // todo: this should be in state machine for onFeet and we should start in a blank state and then
            //       entered rather than assuming we start onFeet here...
            gameObject.GetComponent<CharacterController2D>().Settings = penguinBlob.OnFeetSettings;

            fsmParams = PenguinFsmParams.Upright | PenguinFsmParams.Grounded | ~PenguinFsmParams.Moving;
            stateFeet       = new PenguinStateOnFeet   ("Penguin.State.OnFeet",     penguinBlob, ref fsmParams);
            stateBelly      = new PenguinStateOnBelly  ("Penguin.State.OnBelly",    penguinBlob, ref fsmParams);
            stateStandingUp = new PenguinStateOnFeet   ("Penguin.State.StandingUp", penguinBlob, ref fsmParams);
            stateLyingDown  = new PenguinStateLyingDown("Penguin.State.LyingDown",  penguinBlob, ref fsmParams);
            
            base.Initialize(stateFeet, ref fsmParams);
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
