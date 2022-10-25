using UnityEngine;


namespace PQ.Common.Casts
{
    /*
    Provides a streamlined interface for casting lines from specific points or colliders.
    */
    public sealed class RayCaster
    {
        public const int   AllLayers   = ~0;
        public const float MaxDistance = Mathf.Infinity;

        public bool DrawCastInEditor { get; set; } = true;

        public override string ToString() =>
            $"{GetType().Name}(" +
                $"drawCastInEditor:{DrawCastInEditor}" +
            $")";


        public RayCaster() { }


        /* Shoot out a line between given points, seeing if a TargetLayer is hit. */
        public RayHit CastBetween(Vector2 from, Vector2 to, int layerMask = AllLayers)
        {
            return Cast(
                origin:    from,
                offset:    0f,
                direction: (to - from).normalized,
                layerMask: layerMask,
                maxDistanceFromOrigin:  Vector2.Distance(from, to));
        }

        /* Shoot out a line from point to max distance from that point until a TargetLayer is hit. */
        public RayHit CastFromPoint(Vector2 point, float offset, Vector2 direction, int layerMask = AllLayers,
            float distance = MaxDistance)
        {
            return Cast(
                origin:    point,
                offset:    offset,
                direction: direction.normalized,
                layerMask: layerMask,
                maxDistanceFromOrigin:  distance);
        }

        /* Shoot out a line from edge of collider to distance from that point until a TargetLayer is hit. */
        public RayHit CastFromCollider(Collider2D collider, float offset, Vector2 direction, int layerMask = AllLayers,
            float distance = MaxDistance)
        {
            return Cast(
                origin:    FindPositionOnColliderEdgeInGivenDirection(collider, direction),
                offset:    offset,
                direction: direction.normalized,
                layerMask: layerMask,
                maxDistanceFromOrigin: distance);
        }


        private RayHit Cast(Vector2 origin, float offset, Vector2 direction, LayerMask layerMask, float maxDistanceFromOrigin)
        {
            float offsetCompensation = -1f * offset;

            Vector2 offsetAmount = offset * direction;
            RaycastHit2D castHit2D = Physics2D.Raycast(origin + offsetAmount, direction, maxDistanceFromOrigin, layerMask);

            #if UNITY_EDITOR
            if (DrawCastInEditor)
                DrawCastResultAsLineInEditor(origin, offset, direction, maxDistanceFromOrigin, castHit2D);
            #endif

            if (!castHit2D)
            {
                return default;
            }

            return new RayHit(
                point:    castHit2D.point,
                normal:   castHit2D.normal,
                distance: castHit2D.distance + offsetCompensation,
                collider: castHit2D.collider
            );
        }


        private static Vector2 FindPositionOnColliderEdgeInGivenDirection(Collider2D collider, Vector2 direction)
        {
            Vector2 center = collider.bounds.center;
            collider.bounds.IntersectRay(new Ray(center, direction), out float distanceFromCenterToEdge);
            return center - (distanceFromCenterToEdge * direction);
        }
        
        #if UNITY_EDITOR
        private static void DrawCastResultAsLineInEditor(Vector2 origin, float offset, Vector2 direction, float distance, RaycastHit2D hit)
        {
            float duration = Time.deltaTime;
            Vector2 start = origin + (offset   * direction);
            Vector2 end   = origin + (distance * direction);

            Debug.DrawLine(start, end,    Color.red,     duration);
            Debug.DrawLine(start, origin, Color.magenta, duration);
            if (hit)
            {
                Debug.DrawLine(origin, hit.point, Color.green, duration);
            }
        }
        #endif
    }
}
