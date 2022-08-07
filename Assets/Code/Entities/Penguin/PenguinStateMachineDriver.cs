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
        public FsmState StateFeet       { get; private set; }
        public FsmState StateBelly      { get; private set; }
        public FsmState StateStandingUp { get; private set; }
        public FsmState StateLyingDown  { get; private set; }

        // todo: extract out a proper spawning system, or consider moving these to blob
        public void ResetPositioning()
        {
            penguinBlob.Rigidbody.velocity = Vector2.zero;
            penguinBlob.Rigidbody.position = initialSpawnPosition;
            penguinBlob.Rigidbody.transform.localEulerAngles = Vector3.zero;
        }

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

            StateFeet       = new PenguinStateOnFeet   (this, "Penguin.State.OnFeet",     penguinBlob);
            StateBelly      = new PenguinStateOnBelly  (this, "Penguin.State.OnBelly",    penguinBlob);
            StateStandingUp = new PenguinStateOnFeet   (this, "Penguin.State.StandingUp", penguinBlob);
            StateLyingDown  = new PenguinStateLyingDown(this, "Penguin.State.LyingDown",  penguinBlob);
            
            base.Initialize(StateFeet);
        }
    }
}
