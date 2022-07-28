using UnityEngine;


namespace PQ.Common.Collisions
{
    /*
    Provides a streamlined interface for casting lines from specific points or colliders.
    */
    public class LineCaster
    {
        public RayCasterSettings Settings { get; set; }

        public LineCaster(RayCasterSettings settings)
        {
            Settings = settings;
        }

        /* Shoot out a line from point to max distance from that point until a TargetLayer is hit. */
        public CastResult CastFromPoint(Vector2 point, Vector2 direction)
        {
            return CastLine(point, point + (Settings.MaxDistance * direction));
        }

        /* Shoot out a line from edge of collider to distance from that point until a TargetLayer is hit. */
        public CastResult CastFromCollider(Collider2D collider, Vector2 direction)
        {
            Vector2 point = FindPositionOnColliderEdgeInGivenDirection(collider, direction);
            return CastLine(point, point + (Settings.MaxDistance * direction));
        }

        /* Shoot out a line between given points, seeing if a TargetLayer is hit. */
        public CastResult CastBetween(Vector2 from, Vector2 to)
        {
            return CastLine(from, to);
        }

        private CastResult CastLine(Vector2 from, Vector2 to)
        {
            float offsetAmount = Settings.Offset;
            LayerMask layerMask = Settings.TargetLayers;

            Vector2 offset = offsetAmount * (to - from).normalized;
            Vector2 start  = from + offset;
            Vector2 end    = to   + offset;
            
            CastHit? hit = null;
            RaycastHit2D castHit2D = Physics2D.Linecast(start, end, layerMask);
            if (castHit2D)
            {
                hit = new CastHit(
                    point:    castHit2D.point,
                    normal:   castHit2D.normal,
                    distance: castHit2D.distance - Mathf.Abs(offsetAmount),
                    collider: castHit2D.collider
                );
            }

            return new CastResult(start, end, hit);
        }

        private static Vector2 FindPositionOnColliderEdgeInGivenDirection(Collider2D collider, Vector2 direction)
        {
            Vector2 center = collider.bounds.center;
            collider.bounds.IntersectRay(new Ray(center, direction), out float distanceFromCenterToEdge);
            return center - (distanceFromCenterToEdge * direction);
        }
    }
}
