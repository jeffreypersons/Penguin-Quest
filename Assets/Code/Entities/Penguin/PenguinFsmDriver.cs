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
            SetBlackboardData(blob);

            InitializeGraph(
                (CreateState<PenguinStateOnFeet>(PenguinBlob.StateIdFeet), new[] {
                    PenguinBlob.StateIdLyingDown,
                    PenguinBlob.StateIdMidair
                }),
                (CreateState<PenguinStateOnBelly>(PenguinBlob.StateIdBelly), new[] {
                    PenguinBlob.StateIdStandingUp,
                    PenguinBlob.StateIdMidair
                }),
                (CreateState<PenguinStateStandingUp>(PenguinBlob.StateIdStandingUp), new[] {
                    PenguinBlob.StateIdFeet,
                }),
                (CreateState<PenguinStateLyingDown>(PenguinBlob.StateIdLyingDown), new[] {
                    PenguinBlob.StateIdBelly
                }),
                (CreateState<PenguinStateMidair>(PenguinBlob.StateIdMidair), new[] {
                    PenguinBlob.StateIdFeet,
                    PenguinBlob.StateIdBelly
                })
            );
            SetInitialState(PenguinBlob.StateIdFeet);
        }
    }
}
