using System.Diagnostics.Contracts;
using UnityEngine;


namespace PQ.TestScenes.Minimal
{
    /* Result info of a single ray-surface intersection, with extra convenience methods over built in cast result struct. */
    public struct CastResult
    {
        public readonly Rigidbody2D rigidBody;
        public readonly Vector2     normal;
        public readonly float       distance;

        public override string ToString() =>
            $"{GetType().Name}:{{" +
                $"normal:{normal}," +
                $"distance:{distance}," +
                $"rigidBody:{rigidBody?.ToString() ?? "<None>"}}}";


        public CastResult(Rigidbody2D rigidBody, Vector2 normal, float distance)
        {
            this.rigidBody = rigidBody;
            this.normal    = normal;
            this.distance  = distance;
        }

        [Pure]
        public bool HitInRange(float min, float max)
        {
            return rigidBody != null && distance >= min && distance <= max;
        }

        public static implicit operator bool(CastResult hit)
        {
            return hit.rigidBody != null;
        }
    }
}
