using UnityEngine;


namespace PenguinQuest.Data
{
    public struct CastHit
    {
        public readonly Vector2    point;
        public readonly Vector2    normal;
        public readonly float      distance;
        public readonly Collider2D collider;

        public CastHit(Vector2 point, Vector2 normal, float distance, Collider2D collider)
        {
            this.point    = point;
            this.normal   = normal;
            this.distance = distance;
            this.collider = collider;
        }
    }

    public struct CastResult
    {
        public readonly Vector2 origin;
        public readonly Vector2 terminal;

        public readonly CastHit? hit;
        public CastResult(Vector2 origin, Vector2 terminal, CastHit? hit)
        {
            this.origin   = origin;
            this.terminal = terminal;
            this.hit      = hit;
        }
    }

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
        public CastResult CastFromPoint(Vector2 point, Vector2 direction, float distance)
        {
            return CastLine(point, point + (distance * direction));
        }

        /* Shoot out a line from edge of collider to distance from that point until a TargetLayer is hit. */
        public CastResult CastFromCollider(Collider2D collider, Vector2 direction, float distance)
        {
            Vector2 point = FindPositionOnColliderEdgeInGivenDirection(collider, direction);
            return CastLine(point, point + (distance * direction));
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
            RaycastHit2D rayHit = Physics2D.Linecast(start, end, layerMask);
            if (rayHit)
            {
                hit = new CastHit(rayHit.point, rayHit.normal, rayHit.distance, rayHit.collider);
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
