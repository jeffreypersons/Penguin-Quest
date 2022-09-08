using System;
using UnityEngine;


namespace PQ.Common
{
    /* Results of multiple ray intersections. */
    public struct RayHitGroup
    {
        public readonly int   hitCount;
        public readonly int   rayCount;
        public readonly float hitRatio;
        public readonly float hitDistanceAverage;
        
        public override string ToString() =>
            $"{GetType().Name}:{{" +
                $"hits: {hitCount}/{rayCount}({hitRatio:0.0%}), " +
                $"averageDistance:{hitDistanceAverage}}}";

        public RayHitGroup(int hitCount, int rayCount, float hitDistance)
        {
            this.rayCount    = rayCount;
            this.hitCount    = hitCount;
            this.hitRatio    = hitCount > 0? (float)hitCount / rayCount : 0f;
            this.hitDistanceAverage = hitDistance;
        }


        public static implicit operator bool(RayHitGroup hitGroup)
        {
            return hitGroup.hitCount > 0f;
        }
    }
}
