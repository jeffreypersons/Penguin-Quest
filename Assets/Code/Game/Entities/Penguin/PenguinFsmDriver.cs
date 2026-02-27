using System;
using UnityEngine;
using PQ.Common.Fsm;


namespace PQ.Game.Entities.Penguin
{
    // todo: integrate with entity so the graph is initialized there, and use game object instantiate/add component instead of subclass
    public sealed class PenguinFsmDriver : FsmDriver<PenguinStateId, PenguinEntity>
    {
        protected override void OnInitialStateEntered(PenguinStateId initial) =>
            Debug.Log($"Initialized {this}");

        protected override void OnTransition(PenguinStateId source, PenguinStateId dest) =>
            Debug.Log($"Transitioning Penguin from {source} to {dest}");


        protected override void OnInitialize()
        {
            if (!gameObject.TryGetComponent<PenguinEntity>(out var penguinBlob))
            {
                throw new InvalidOperationException(
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
