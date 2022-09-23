using UnityEngine;
using PQ.Common.Fsm;


namespace PQ.Entities.Penguin
{
    public sealed class PenguinFsmDriver : FsmDriver<PenguinBlob>
    {
        protected override void OnInitialStateEntered(string initial)
        {
            Debug.Log($"Entered initial state...here's what things look like:\n{this}");
        }

        protected override void OnTransition(string source, string dest)
        {
            Debug.Log($"Transitioning Penguin from {source} to {dest}");
        }

        protected override PenguinBlob Data { get; set; }

        protected override void OnInitialize()
        {
            PenguinBlob penguinBlob = gameObject.GetComponent<PenguinBlob>();

            Data = penguinBlob;

            Initialize(
                data: penguinBlob,
                initial: PenguinBlob.StateIdFeet,
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
        }
    }
}
