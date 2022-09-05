using UnityEngine;


namespace PQ.Common
{
    /* Result info of a single ray-surface intersection. */
    public struct RayHit
    {
        public readonly Vector2    point;
        public readonly Vector2    normal;
        public readonly float      distance;
        public readonly Collider2D collider;


        public RayHit(Vector2 point, Vector2 normal, float distance, Collider2D collider)
        {
            this.point    = point;
            this.normal   = normal;
            this.distance = distance;
            this.collider = collider;
        }

        public static implicit operator bool(RayHit hit)
        {
            return hit.collider != null;
        }
    }
}
