using System;
using UnityEngine;
using PQ.Common.Fsm;
using StateId = PQ.Entities.Penguin.PenguinBlob.StateId;


namespace PQ.Entities.Penguin
{
    public sealed class PenguinFsmDriver : FsmDriver<PenguinBlob>
    {
        protected override void OnInitialStateEntered(string initial)
        {
            Debug.Log($"Entered initial state");
        }

        protected override void OnTransition(string source, string dest)
        {
            Debug.Log($"Transitioning Penguin from {source} to {dest}");
        }

        protected override void OnInitialize()
        {
            if (!gameObject.TryGetComponent<PenguinBlob>(out var penguinBlob))
            {
                throw new System.InvalidOperationException(
                    $"PenguinBlob not found - driver must be attached to same gameObject as PenguinFsmDriver");
            }

            Initialize(new Builder(persistentData: penguinBlob, initial: StateId.Feet)

                .AddNode<PenguinStateOnFeet>(StateId.Feet, new[] {
                    StateId.LyingDown,
                    StateId.Midair,
                })
                .AddNode<PenguinStateOnBelly>(StateId.Belly, new[] {
                    StateId.StandingUp,
                    StateId.Midair,
                })
                .AddNode<PenguinStateStandingUp>(StateId.StandingUp, new[] {
                    StateId.Feet,
                })
                .AddNode<PenguinStateLyingDown>(StateId.LyingDown, new[] {
                    StateId.Belly,
                })
                .AddNode<PenguinStateMidair>(StateId.Midair, new[] {
                    StateId.Feet,
                    StateId.Belly,
                })
            );
        }
    }
}
