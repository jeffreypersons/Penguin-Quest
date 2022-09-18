using UnityEngine;
using PQ.Common.States;


namespace PQ.Entities.Penguin
{
    /*
    switch state
        feet   [liedown signal] -> liedown
        lie    [finish  signal] -> belly
        belly  [standup signal] -> standup
        stand  [finish  signal] -> feet
    */
    public class PenguinFsmDriver : FsmDriver
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

        private const string keyStateOnFeet     = "Penguin.State.OnFeet";
        private const string keyStateOnBelly    = "Penguin.State.OnBelly";
        private const string keyStateStandingUp = "Penguin.State.StandingUp";
        private const string keyStateLyingDown  = "Penguin.State.LyingDown";
        private const string keyStateMidair     = "Penguin.State.Midair";

        protected override void OnInitialize()
        {
            _eventCenter = GameEventCenter.Instance;
            _penguinBlob = gameObject.GetComponent<PenguinBlob>();
            _initialSpawnPosition = _penguinBlob.SkeletalRootPosition;
            ResetPositioning();

            StateFeet       = new PenguinStateOnFeet    (keyStateOnFeet,     this, _penguinBlob, _eventCenter);
            StateBelly      = new PenguinStateOnBelly   (keyStateOnBelly,    this, _penguinBlob, _eventCenter);
            StateStandingUp = new PenguinStateStandingUp(keyStateStandingUp, this, _penguinBlob, _eventCenter);
            StateLyingDown  = new PenguinStateLyingDown (keyStateLyingDown,  this, _penguinBlob, _eventCenter);
            StateMidair     = new PenguinStateMidair    (keyStateMidair,     this, _penguinBlob, _eventCenter);

            InitializeStates(StateFeet, StateBelly, StateStandingUp, StateLyingDown, StateMidair);
        }
    }
}
