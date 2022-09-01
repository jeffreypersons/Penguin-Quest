using UnityEngine;


namespace PQ.Common
{
    /*
    Provides a streamlined interface for casting lines from specific points or colliders.
    */
    public class RayCaster
    {
        public float     MaxDistance { get; set; } = Mathf.Infinity;
        public LayerMask LayerMask   { get; set; } = ~0;

        public RayCaster() { }


        /* Shoot out a line between given points, seeing if a TargetLayer is hit. */
        public CastHit? CastBetween(Vector2 from, Vector2 to)
        {
            return Cast(from, (to - from).normalized, MaxDistance, LayerMask);
        }

        /* Shoot out a line from point to max distance from that point until a TargetLayer is hit. */
        public CastHit? CastFromPoint(Vector2 point, Vector2 direction)
        {
            return Cast(point, direction, MaxDistance, LayerMask);
        }

        /* Shoot out a line from edge of collider to distance from that point until a TargetLayer is hit. */
        public CastHit? CastFromCollider(Collider2D collider, Vector2 direction)
        {
            Vector2 point = FindPositionOnColliderEdgeInGivenDirection(collider, direction);
            return Cast(point, direction, MaxDistance, LayerMask);
        }


        private static CastHit? Cast(Vector2 origin, Vector2 direction, float distance, LayerMask layerMask)
        {
            RaycastHit2D castHit2D = Physics2D.Raycast(origin, direction, distance, layerMask);
            if (!castHit2D)
            {
                return null;
            }

            return new CastHit(
                point:    castHit2D.point,
                normal:   castHit2D.normal,
                distance: castHit2D.distance,
                collider: castHit2D.collider
            );
        }

        private static Vector2 FindPositionOnColliderEdgeInGivenDirection(Collider2D collider, Vector2 direction)
        {
            Vector2 center = collider.bounds.center;
            collider.bounds.IntersectRay(new Ray(center, direction), out float distanceFromCenterToEdge);
            return center - (distanceFromCenterToEdge * direction);
        }
    }
}
