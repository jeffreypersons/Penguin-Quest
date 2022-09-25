using UnityEngine;
using PQ.Common.Fsm;


namespace PQ.Entities.Penguin
{
    using PenguinStateId = PenguinBlob.StateId;

    public sealed class PenguinFsmDriver : FsmDriver<PenguinStateId, PenguinBlob>
    {
        protected override void OnInitialStateEntered(PenguinStateId initial) =>
            Debug.Log($"Entered initial state");

        protected override void OnTransition(PenguinStateId source, PenguinStateId dest) =>
            Debug.Log($"Transitioning Penguin from {source} to {dest}");


        protected override void OnInitialize()
        {
            if (!gameObject.TryGetComponent<PenguinBlob>(out var penguinBlob))
            {
                throw new System.InvalidOperationException(
                    $"PenguinBlob not found - driver must be attached to same gameObject as PenguinFsmDriver");
            }

            Initialize(new Builder(persistentData: penguinBlob, initial: PenguinStateId.Feet)

                .AddNode<PenguinStateOnFeet>(PenguinStateId.Feet, new[] {
                    PenguinStateId.LyingDown,
                    PenguinStateId.Midair,
                })
                .AddNode<PenguinStateOnBelly>(PenguinStateId.Belly, new[] {
                    PenguinStateId.StandingUp,
                    PenguinStateId.Midair,
                })
                .AddNode<PenguinStateStandingUp>(PenguinStateId.StandingUp, new[] {
                    PenguinStateId.Feet,
                })
                .AddNode<PenguinStateLyingDown>(PenguinStateId.LyingDown, new[] {
                    PenguinStateId.Belly,
                })
                .AddNode<PenguinStateMidair>(PenguinStateId.Midair, new[] {
                    PenguinStateId.Feet,
                    PenguinStateId.Belly,
                })
            );
        }
    }
}
