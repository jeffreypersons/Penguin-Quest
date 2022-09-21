using UnityEngine;
using PQ.Common.Fsm;


namespace PQ.Entities.Penguin
{

    public class PenguinFsmDriver : FsmDriver
    {
        private PenguinBlob _penguinBlob;
        private Vector2 _initialSpawnPosition;

        public void ResetPositioning()
        {
            _penguinBlob.CharacterController.PlaceAt(_initialSpawnPosition, rotation: 0);
        }

        protected override void OnTransition(string sourceId, string destinationId)
        {
            Debug.Log($"Transitioning Penguin from {sourceId} to {destinationId}");
        }


        protected override void OnInitialize()
        {
            _penguinBlob = gameObject.GetComponent<PenguinBlob>();
            _initialSpawnPosition = _penguinBlob.SkeletalRootPosition;
            ResetPositioning();

            InitializeGraph(
                (new PenguinStateOnFeet(PenguinBlob.StateIdFeet, this, _penguinBlob), new[] {
                    PenguinBlob.StateIdLyingDown,
                    PenguinBlob.StateIdMidair
                }),
                (new PenguinStateOnBelly(PenguinBlob.StateIdBelly, this, _penguinBlob), new[] {
                    PenguinBlob.StateIdStandingUp,
                    PenguinBlob.StateIdMidair
                }),
                (new PenguinStateStandingUp(PenguinBlob.StateIdStandingUp, this, _penguinBlob), new[] {
                    PenguinBlob.StateIdFeet,
                }),
                (new PenguinStateLyingDown(PenguinBlob.StateIdLyingDown, this, _penguinBlob), new[] {
                    PenguinBlob.StateIdBelly
                }),
                (new PenguinStateMidair(PenguinBlob.StateIdMidair, this, _penguinBlob), new[] {
                    PenguinBlob.StateIdFeet,
                    PenguinBlob.StateIdBelly
                })
            );
            SetInitialState(PenguinBlob.StateIdFeet);
        }
    }
}
