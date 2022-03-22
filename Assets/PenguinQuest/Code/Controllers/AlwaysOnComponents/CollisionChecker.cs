using System;
using UnityEngine;
using PenguinQuest.Utils;
using PenguinQuest.Data;



namespace PenguinQuest.Controllers.AlwaysOnComponents
{
    /*
    Provides functionality querying the surrounding of a bounding box.

    For example, is there something X distance in front of me?
    What about the front half of the box's bottom side?
    */
    public class CollisionChecker : MonoBehaviour
    {
        // todo: look into integrating with box perimeter caster? Or at least putting things in different places
        [Header("Ground Settings")]
        [SerializeField] private BoxCollider2D colliderToCastFrom;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] [Range(0.25f, 25.00f)] private float toleratedDistanceFromGround = 0.30f;
        [SerializeField] private RayCasterSettings perimeterCasterSettings = default;

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
            _perimeterCaster.CastAll();
            if (HasHitAtLeastOneWithinDistance(_perimeterCaster.BottomResults, toleratedDistanceFromGround, out CastHit hit))
            {
                IsGrounded    = true;
                SurfaceNormal = hit.normal;
            }
            else
            {
                IsGrounded    = false;
                SurfaceNormal = Vector2.up;
            }
        }

        private bool HasHitAtLeastOneWithinDistance(ReadOnlySpan<CastResult> results, float distance, out CastHit hit)
        {            
            // todo: account for different layers and stuff
            // todo: try from left to right
            // todo: figure out a proper way of 'capturing' normal - perhaps a downward sphere cast centroid result?
            //       ...or maybe just look at how seblag handled it...averages or something?
            foreach (CastResult result in results)
            {
                if (result.hit.HasValue && result.hit.Value.distance <= distance)
                {
                    hit = result.hit.Value;
                    return true;
                }
            }
            hit = default;
            return false;
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
                GizmosUtils.DrawLine(from: result.origin, to: result.terminal, color: Color.red);
                if (result.hit != null)
                {
                    GizmosUtils.DrawLine(from: result.origin, to: result.hit.Value.point, color: Color.green);
                }
            }
        }
        #endif
    }
}
