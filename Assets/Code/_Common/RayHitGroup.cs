using UnityEngine;


namespace PQ.Common
{
    /* Results of multiple ray intersections. */
    public struct RayHitGroup
    {
        public readonly Vector2    point;
        public readonly Vector2    normal;
        public readonly float      distance;
        public readonly Collider2D collider;

        public RayHitGroup(Vector2 point, Vector2 normal, float distance, Collider2D collider)
        {
            this.point    = point;
            this.normal   = normal;
            this.distance = distance;
            this.collider = collider;
        }
    }
}
