using System;
using UnityEngine;


namespace PQ.Game.Entities
{
    [CreateAssetMenu(
        fileName = "PenguinEntitySettings",
        menuName = "ScriptableObjects/PenguinEntitySettings",
        order = 1)]
    public class PenguinEntitySettings : ScriptableObject
    {
        // Any time script is loaded, or field values are modified, where do we want that input to go?
        // Note that since we treat there editor-configured settings as input, we deliberately expose a single listener at a time
        public Action OnChanged { set; get; }
        private void OnValidate() => OnChanged?.Invoke();


        [Header("Character Movement Settings")]

        [Tooltip("Step size used to adjust blend percent when transitioning between idle/moving states " +
                 "(ie 0.05 for blended delayed transition taking at least 20 frames, 1 for instant transition)")]
        [Range(0.01f, 1.00f)][SerializeField] public float locomotionBlendStep = 0.10f;

        [Tooltip("At what speed does the character walk along the horizontal?")]
        [Range(0, 10)][SerializeField] public float walkSpeed = 0.50f;

        [Tooltip("At what speed does the character move upwards?")]
        [Range(0, 50)][SerializeField] public float jumpSpeed = 2.50f;

        [Tooltip("Horizontal distance from jump origin to bottom center of arc")]
        [Range(0, 50)][SerializeField] public float jumpLengthToApex = 3.50f;

        [Tooltip("Vertical Distance from jump origin to top of arc")]
        [Range(0, 50)][SerializeField] public float jumpHeightToApex = 3.50f;


        [Header("Solver Method")]

        [Tooltip("How many times do we iterate the physics solver during a single fixed frame, where we evaluate movement?")]
        [SerializeField][Range(0, 50)] public int solverIterationsPerPhysicsUpdate = 10;


        // todo: ideally bounciness/friction would be dealt with per surface/material instead
        [Header("Collision Properties")]

        [Tooltip("How how much do reflect along the tangent (friction is from -1 ('boosts' velocity) to 0 (no resistance) to 1 (max resistance))?")]
        [SerializeField][Range(0, 1)] public float collisionBounciness = 0f;

        [Tooltip("How how much do reflect along the normal (bounciness is from 0 (no bounciness) to 1 (completely reflected))?")]
        [SerializeField][Range(-1, 1)] public float collisionFriction = 0f;

        [Tooltip("At what slope angle do we allow the character to walk up to?")]
        [SerializeField][Range(0, 90)] public float maxAscendableSlopeAngle = 45f;

        [Tooltip("What's the buffer (aka skin width) that we offset collisions and casts with?")]
        [SerializeField][Range(0, 1)] public float skinWidth = 0.25f;

        [Tooltip("What layers do we consider to make up ceilings, obstacles, ground, platforms, etc?")]
        [SerializeField] public LayerMask groundLayerMask = default;

        [Tooltip("How strong (as a multiple of the global setting) is the pull of gravity?")]
        [SerializeField][Range(0, 10)] public float gravityScale = 1.00f;
    }
}
