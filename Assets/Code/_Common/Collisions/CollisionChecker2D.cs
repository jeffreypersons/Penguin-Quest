using System;
using UnityEngine;


namespace PQ.Common.Collisions
{
    /*
    Provides functionality querying the surrounding of a bounding box.

    For example, is there something X distance in front of me?
    What about the front half of the box's bottom side?

    TODO: Account for different layers and different sides being activated/deactivated etc?
    */
    public class CollisionChecker2D : MonoBehaviour
    {
        // todo: look into integrating with box perimeter caster? Or at least putting things in different places
        [Header("Ground Settings")]
        [SerializeField] private BoxCollider2D colliderToCastFrom;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] [Range(0.25f, 25.00f)] private float toleratedDistanceFromGround = 0.30f;
        [SerializeField] private RayCasterSettings perimeterCasterSettings;

        private BoxPerimeterRayCaster _perimeterCaster;
        public bool    IsGrounded    { get; private set; } = false;
        public Vector2 SurfaceNormal { get; private set; } = Vector2.up;

        // TODO: build out above/below/front/back that uses the BoxPerimeterRayCaster for querying those areas
        void Start()
        {
            _perimeterCaster = new BoxPerimeterRayCaster(colliderToCastFrom, perimeterCasterSettings);
            CheckForGround();
        }

        void FixedUpdate()
        {
            if (colliderToCastFrom)
            {
                CheckForGround();
            }
        }
        
        public void CheckForGround()
        {
            // we consider the entity grounded if layer found directly below within distance threshold,
            // with the surface normal taken if we find ground within perimeter caster's maxRayDistance
            _perimeterCaster.CastAll();
            ReadOnlySpan<CastResult> downwardCastResults = _perimeterCaster.BottomResults;
            IsGrounded = HasHitAtLeastOneWithinDistance(downwardCastResults, toleratedDistanceFromGround);
            SurfaceNormal = SurfaceNormalOfMiddleHit(downwardCastResults, defaultNormalIfNoHit: Vector2.up);
        }


        private static bool HasHitAtLeastOneWithinDistance(ReadOnlySpan<CastResult> results, float distance)
        {
            foreach (CastResult result in results)
            {
                if (result.hit.HasValue && result.hit.Value.distance <= distance)
                {
                    return true;
                }
            }
            return false;
        }

        private static Vector2 SurfaceNormalOfMiddleHit(ReadOnlySpan<CastResult> results, Vector2 defaultNormalIfNoHit)
        {
            int midIndex = (int)(results.Length * 0.50f);
            return !results.IsEmpty && results[midIndex].hit.HasValue ?
                results[midIndex].hit.Value.normal :
                defaultNormalIfNoHit;
        }

        #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!Application.IsPlaying(this) || !enabled)
            {
                return;
            }

            foreach (CastResult result in _perimeterCaster.AllResults)
            {
                Extensions.GizmoExtensions.DrawLine(from: result.origin, to: result.terminal, color: Color.red);
                if (result.hit != null)
                {
                    Extensions.GizmoExtensions.DrawLine(from: result.origin, to: result.hit.Value.point, color: Color.green);
                }
            }
        }
        #endif
    }
}
