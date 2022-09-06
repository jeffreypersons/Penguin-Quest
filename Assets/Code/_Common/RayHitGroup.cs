using System;
using UnityEngine;


namespace PQ.Common
{
    /* Results of multiple ray intersections. */
    public struct RayHitGroup
    {
        public readonly int   rayCount;
        public readonly int   hitCount;
        public readonly float hitPercentage;
        public readonly float hitDistance;
        
        public override string ToString() =>
            $"{GetType().Name}:{{" +
                $"rayCount:{rayCount}," +
                $"hitCount:{hitCount}," +
                $"hitPercentage:{hitPercentage}," +
                $"hitDistance:{hitDistance}}}";

        // todo: move results _into_ this class, or at least a reference to it
        public RayHitGroup(ReadOnlySpan<RayHit> hits)
        {
            int hitCount = 0;
            float distanceSum = 0f;
            int rayCount = hits.Length;
            for (int rayIndex = 0; rayIndex < rayCount; rayIndex++)
            {
                if (hits[rayIndex])
                {
                    hitCount++;
                    distanceSum += hits[rayIndex].distance;
                }
            }

            this.rayCount      = rayCount;
            this.hitCount      = hitCount;
            this.hitPercentage = hitCount    > 0f? ((float)rayCount / hitCount)    : 0f;
            this.hitDistance   = distanceSum > 0f? ((float)rayCount / distanceSum) : 0f;
        }


        public static implicit operator bool(RayHitGroup hitGroup)
        {
            return hitGroup.hitCount > 0f;
        }
    }
}
