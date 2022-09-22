using UnityEngine;
using PQ.Common.Fsm;


namespace PQ.Entities.Penguin
{
    public sealed class PenguinFsmDriver : FsmDriver<PenguinBlob>
    {
        protected override PenguinBlob Data { get; set; }

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
            PenguinBlob blob = gameObject.GetComponent<PenguinBlob>();
            Data = blob;

            InitializeGraph(
                (new PenguinStateOnFeet(PenguinBlob.StateIdFeet, blob), new[] {
                    PenguinBlob.StateIdLyingDown,
                    PenguinBlob.StateIdMidair
                }),
                (new PenguinStateOnBelly(PenguinBlob.StateIdBelly, blob), new[] {
                    PenguinBlob.StateIdStandingUp,
                    PenguinBlob.StateIdMidair
                }),
                (new PenguinStateStandingUp(PenguinBlob.StateIdStandingUp, blob), new[] {
                    PenguinBlob.StateIdFeet,
                }),
                (new PenguinStateLyingDown(PenguinBlob.StateIdLyingDown, blob), new[] {
                    PenguinBlob.StateIdBelly
                }),
                (new PenguinStateMidair(PenguinBlob.StateIdMidair, blob), new[] {
                    PenguinBlob.StateIdFeet,
                    PenguinBlob.StateIdBelly
                })
            );
            SetInitialState(PenguinBlob.StateIdFeet);
            SetBlackboardData(blob);
        }
    }
}
