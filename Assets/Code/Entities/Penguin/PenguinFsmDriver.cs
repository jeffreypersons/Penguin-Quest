using UnityEngine;
using PQ.Common.Fsm;


namespace PQ.Entities.Penguin
{
    public sealed class PenguinFsmDriver : FsmDriver
    {
        protected override void OnInitialStateEntered(string initial)
        {
            Debug.Log($"Entered initial state...here's what things look like:\n{this}");
        }

        protected override void OnTransition(string source, string dest)
        {
            Debug.Log($"Transitioning Penguin from {source} to {dest}");
        }

        protected override void OnInitialize()
        {
            PenguinBlob _penguinBlob = gameObject.GetComponent<PenguinBlob>();

            InitializeGraph(
                (new PenguinStateOnFeet(PenguinBlob.StateIdFeet, _penguinBlob), new[] {
                    PenguinBlob.StateIdLyingDown,
                    PenguinBlob.StateIdMidair
                }),
                (new PenguinStateOnBelly(PenguinBlob.StateIdBelly, _penguinBlob), new[] {
                    PenguinBlob.StateIdStandingUp,
                    PenguinBlob.StateIdMidair
                }),
                (new PenguinStateStandingUp(PenguinBlob.StateIdStandingUp, _penguinBlob), new[] {
                    PenguinBlob.StateIdFeet,
                }),
                (new PenguinStateLyingDown(PenguinBlob.StateIdLyingDown, _penguinBlob), new[] {
                    PenguinBlob.StateIdBelly
                }),
                (new PenguinStateMidair(PenguinBlob.StateIdMidair, _penguinBlob), new[] {
                    PenguinBlob.StateIdFeet,
                    PenguinBlob.StateIdBelly
                })
            );
            SetInitialState(PenguinBlob.StateIdFeet);
            SetBlob(_penguinBlob);
        }
    }
}
