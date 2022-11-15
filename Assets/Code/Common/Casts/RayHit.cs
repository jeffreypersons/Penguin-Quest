﻿using System.Diagnostics.Contracts;
using UnityEngine;


namespace PQ.Common.Casts
{
    /* Result info of a single ray-surface intersection, with extra convenience methods over built in cast result struct. */
    public struct RayHit
    {
        public readonly Vector2    point;
        public readonly Vector2    normal;
        public readonly float      distance;
        public readonly Collider2D collider;

        public override string ToString() =>
            $"{GetType().Name}:{{" +
                $"point:{point}," +
                $"normal:{normal}," +
                $"distance:{distance}," +
                $"collider:{collider?.ToString() ?? "<None>"}}}";


        public RayHit(Vector2 point, Vector2 normal, float distance, Collider2D collider)
        {
            this.point    = point;
            this.normal   = normal;
            this.distance = distance;
            this.collider = collider;
        }

        [Pure]
        public bool HitInRange(float min, float max)
        {
            return collider != null && distance >= min && distance <= max;
        }

        public static implicit operator bool(RayHit hit)
        {
            return hit.collider != null;
        }
    }
}
