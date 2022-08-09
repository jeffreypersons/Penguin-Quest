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

        // todo: replace with a cleaner, more reusable way to do this
        public FsmState StateFeet       { get; private set; }
        public FsmState StateBelly      { get; private set; }
        public FsmState StateStandingUp { get; private set; }
        public FsmState StateLyingDown  { get; private set; }

        // todo: extract out a proper spawning system, or consider moving these to blob
        public void ResetPositioning()
        {
            _penguinBlob.Rigidbody.velocity = Vector2.zero;
            _penguinBlob.Rigidbody.position = _initialSpawnPosition;
            _penguinBlob.Rigidbody.transform.localEulerAngles = Vector3.zero;
        }

        protected override void OnTransition(FsmState previous, FsmState next)
        {
            Debug.Log($"Transitioning Penguin from {previous} to {next}");
        }

        protected override void Initialize(FsmState initialState)
        {
            _eventCenter = GameEventCenter.Instance;

            _penguinBlob = gameObject.GetComponent<PenguinBlob>();
            _initialSpawnPosition = _penguinBlob.Rigidbody.position;
            ResetPositioning();

            StateFeet       = new PenguinStateOnFeet    (this, "Penguin.State.OnFeet",     _penguinBlob, _eventCenter);
            StateBelly      = new PenguinStateOnBelly   (this, "Penguin.State.OnBelly",    _penguinBlob, _eventCenter);
            StateStandingUp = new PenguinStateStandingUp(this, "Penguin.State.StandingUp", _penguinBlob, _eventCenter);
            StateLyingDown  = new PenguinStateLyingDown (this, "Penguin.State.LyingDown",  _penguinBlob, _eventCenter);
            
            base.Initialize(StateFeet);
        }
    }
}
