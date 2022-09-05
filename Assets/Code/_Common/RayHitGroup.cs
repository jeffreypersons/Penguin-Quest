using System;
using UnityEngine;


namespace PQ.Common
{
    /* Results of multiple ray intersections. */
    public struct RayHitGroup
    {
        public readonly float hitPercentage;
        public readonly float hitDistance;

        public RayHitGroup(float hitPercentage, float hitDistance)
        {
            this.hitPercentage = hitPercentage;
            this.hitDistance = hitDistance;
        }

        public static implicit operator bool(RayHitGroup hitGroup)
        {
            return hitGroup.hitPercentage > 0f;
        }
    }
}
