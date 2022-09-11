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
        private GameEventCenter _eventCenter;

        private PenguinBlob _penguinBlob;

        private Vector2 _initialSpawnPosition;

        public FsmState StateFeet       { get; private set; }
        public FsmState StateBelly      { get; private set; }
        public FsmState StateStandingUp { get; private set; }
        public FsmState StateLyingDown  { get; private set; }
        public FsmState StateMidair     { get; private set; }


        public void ResetPositioning()
        {
            _penguinBlob.CharacterController.PlaceAt(_initialSpawnPosition, rotation: 0);
        }

        protected override void OnTransition(FsmState previous, FsmState next)
        {
            Debug.Log($"Transitioning Penguin from {previous} to {next}");
        }

        protected override void OnInitialize()
        {
            _eventCenter = GameEventCenter.Instance;
            _penguinBlob = gameObject.GetComponent<PenguinBlob>();
            _initialSpawnPosition = _penguinBlob.SkeletalRootPosition;
            ResetPositioning();

            StateFeet       = new PenguinStateOnFeet    ("Penguin.State.OnFeet",     this, _penguinBlob, _eventCenter);
            StateBelly      = new PenguinStateOnBelly   ("Penguin.State.OnBelly",    this, _penguinBlob, _eventCenter);
            StateStandingUp = new PenguinStateStandingUp("Penguin.State.StandingUp", this, _penguinBlob, _eventCenter);
            StateLyingDown  = new PenguinStateLyingDown ("Penguin.State.LyingDown",  this, _penguinBlob, _eventCenter);
            StateMidair     = new PenguinStateMidair    ("Penguin.State.Midair",     this, _penguinBlob, _eventCenter);

            InitializeStates(initialState: StateFeet, StateBelly, StateStandingUp, StateLyingDown, StateMidair);
        }
    }
}
