using System;
using UnityEngine;


namespace PQ.Common.Physics
{
    /*
    Data container for kinematic body settings.
    */
    [AddComponentMenu("KinematicBody2DSettings")]
    public sealed class KinematicBody2DSettings : MonoBehaviour
    {
        // Any time script is loaded, or field values are modified, where do we want that input to go?
        // Note that since we treat there editor-configured settings as input, we deliberately expose a single listener at a time
        private event Action _onChanged = delegate { };

        private void OnValidate() => _onChanged.Invoke();
        public void RegisterOnChanged(Action onChanged) => _onChanged += onChanged;


        [Header("Bounds")]

        [Tooltip("Minimum for bounding box, with coordinates relative to rigidbody position")]
        [SerializeField] public Vector2 AABBCornerMin = -Vector2.one;

        [Tooltip("Maximum for bounding box, with coordinates relative to rigidbody position")]
        [SerializeField] public Vector2 AABBCornerMax =  Vector2.one;

        [Tooltip("Maximum permissible overlap with other colliders")]
        [SerializeField][Range(0, 1)] public float overlapTolerance = 0.04f;


        [Header("Collision Detection")]

        [Tooltip("Layers to include in collision detection")]
        [SerializeField] public LayerMask layerMask = default;
        
        [Tooltip("Max degrees allowed for climbing a slope")]
        [SerializeField][Range(0, 90)] public float maxAscendableSlopeAngle = 90f;

        [Tooltip("Multiplier for 2D gravity")]
        [SerializeField][Range(0, 10)] public float gravityScale = 1.00f;

        [Tooltip("Scalar for reflection along the normal (bounciness is from 0 (no bounciness) to 1 (completely reflected))")]
        [SerializeField] [Range(0, 1)] public float collisionBounciness = 0f;

        [Tooltip("Scalar for reflection along the tangent (friction is from -1 ('boosts' velocity) to 0 (no resistance) to 1 (max resistance))")]
        [SerializeField] [Range(-1, 1)] public float collisionFriction = 0f;


        [Header("Solver Settings")]

        [Tooltip("Cap on number of movement solves")]
        [SerializeField][Range(0, 50)] public int maxSolverMoveIterations = 10;

        [Tooltip("Cap on number of overlap resolution solves")]
        [SerializeField][Range(0, 50)] public int maxSolverOverlapIterations = 2;

        [Tooltip("Size of buffer used to cache cast results")]
        [SerializeField][Range(1, 100)] public int preallocatedHitBufferSize = 16;


        public override string ToString() =>
            $"{GetType()}\n" +
                $"  gravity - (scale={gravityScale})\n" +
                $"  maxSolverIterations - moves={maxSolverMoveIterations},overlaps={maxSolverOverlapIterations})\n" +
                $"  collisionDetection - layers={layerMask},hitBufferSize={preallocatedHitBufferSize})\n" +
                $"  collisionResponse - bounciness={collisionBounciness},friction={collisionFriction})\n" +
                $"  AABB - x:{{{AABBCornerMin.x},{AABBCornerMax.x}}},y:{{{AABBCornerMin.x},{AABBCornerMax.x}}},buffer={overlapTolerance}";
    }
}
