using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    // todo: try integrating generic flags into state machine/graph/transitions
    // at the framework level
    [System.Flags]
    public enum PenguinFsmParams
    {
        Grounded,
        Upright,
        Moving
    }


    // todo: overhaul the placeholder code, and move things to states more
    public class PenguinStateMachineDriver : FsmStateMachineDriver<PenguinFsmParams>
    {
        private PlayerGameplayInputReceiver input;
        private PenguinBlob penguinBlob;

        private Vector2 initialSpawnPosition;

        // todo: replace with a cleaner, more reusable way to do this
        private FsmState<PenguinFsmParams> stateFeet;
        private FsmState<PenguinFsmParams> stateBelly;
        private FsmState<PenguinFsmParams> stateBellyToFeet;
        private FsmState<PenguinFsmParams> stateFeetToBelly;

        protected override void Initialize(FsmState<PenguinFsmParams> initialState)
        {
            penguinBlob = gameObject.GetComponent<PenguinBlob>();
            initialSpawnPosition = penguinBlob.Rigidbody.position;
            ResetPositioning();

            // todo: this should be in state machine for onFeet and we should start in a blank state and then
            //       entered rather than assuming we start onFeet here...
            gameObject.GetComponent<CharacterController2D>().Settings = penguinBlob.OnFeetSettings;
            
            stateFeet  = new PenguinStateOnFeet   ("Penguin.State.OnFeet",     penguinBlob);
            stateBelly = new PenguinStateOnBelly  ("Penguin.State.OnBelly",    penguinBlob);
            stateFeet  = new PenguinStateOnFeet   ("Penguin.State.StandingUp", penguinBlob);
            stateBelly = new PenguinStateLyingDown("Penguin.State.LyingDown",  penguinBlob);
            base.Initialize(stateFeet);
        }
        
        private void OnEnable()
        {
            GameEventCenter.lieDownCommand.AddListener(OnLieDownInputReceived);
            /*
            GameEventCenter.lieDownCommand            .AddListener(OnLieDownInputReceived);
            GameEventCenter.standUpCommand            .AddListener(OnLieDownInputReceived);
            GameEventCenter.startHorizontalMoveCommand.AddListener(OnLieDownInputReceived);
            GameEventCenter.stopHorizontalMoveCommand .AddListener(OnLieDownInputReceived);
            */
            // penguinBlob.CharacterController.OnGroundContactChanged += OnGroundedPropertyChanged;
        }
        private void OnDisable()
        {
            GameEventCenter.lieDownCommand.RemoveListener(OnLieDownInputReceived);
            // penguinBlob.CharacterController.OnGroundContactChanged -= OnGroundedPropertyChanged;
        }
        private void OnLieDownInputReceived(string _)
        {
            if (!IsCurrently(stateBelly))
            {

            }
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
            belly => standup => feet
            feet  => liedown => belly

            switch state
                feet   [liedown signal] -> liedown
                lie    [finish  signal] -> belly
                belly  [standup signal] -> standup
                stand  [finish signal] -> feet


            FsmState newState;
            switch (onFeet)
            {

            }
            //MoveToState(newState);

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

        protected override void OnTransition(FsmState<PenguinFsmParams> previous, FsmState<PenguinFsmParams> next)
        {
            Debug.Log($"Transitioning Penguin from {previous} to {next}");
        }

        protected void OnGroundedPropertyChanged(bool isGrounded)
        {
            penguinBlob.Animation.SetParamIsGrounded(isGrounded);
        }
    }
}
