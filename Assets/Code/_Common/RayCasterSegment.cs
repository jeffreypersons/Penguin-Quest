﻿using System;
using UnityEngine;


namespace PQ.Common
{
    /*
    Provides a streamlined interface for casting rays from a series of line casters along a segment.
    */
    public sealed class RayCasterSegment
    {
        private Vector2 _segmentStart;
        private Vector2 _segmentEnd;

        private Vector2 _rayDirection;
        private RayCaster _rayCaster;
        private RayHit[] _results;

        public const int MinRayCount = 3;
        public const int MaxRayCount = 1000;


        public ReadOnlySpan<RayHit> RayCastResults => _results.AsSpan();

        public Vector2   SegmentStart     => _segmentStart;
        public Vector2   SegmentMid       => Vector2.Lerp(_segmentStart, _segmentEnd, 0.50f);
        public Vector2   SegmentEnd       => _segmentEnd;
        public Vector2   SegmentDirection => (_segmentEnd - _segmentStart).normalized;
        public float     SegmentLength    => (_segmentEnd - _segmentStart).magnitude;

        public int       RayCount         => _results.Length;
        public float     RaySpacing       => (_segmentEnd - _segmentStart).magnitude / (_results.Length - 1);
        public Vector2   RayDirection     => _rayDirection;
        public float     RayDistance      => _rayCaster.MaxDistance;
        public LayerMask RayTargetLayers  => _rayCaster.LayerMask;

        public override string ToString() =>
            $"{GetType().Name}{{" +
                $"endpoints[{SegmentStart},{SegmentEnd}], " +
                $"raySpacing:{RaySpacing}," +
                $"rayCount:{RayCount}," +
                $"rayDirection:{RayDirection}}}";


        public RayCasterSegment() :
            this(segmentStart: Vector2.zero, segmentEnd: Vector2.zero, rayCount: MinRayCount, Vector2.right)
        { }

        public RayCasterSegment(Vector2 segmentStart, Vector2 segmentEnd, int rayCount, Vector2 rayDirection)
        {
            _rayCaster = new RayCaster { MaxDistance = Mathf.Infinity, LayerMask = ~0 };
            SetRayCount(rayCount);
            UpdatePositioning(segmentStart, segmentEnd, rayDirection);
        }

        /* How many rays should we shoot out from our segment? Warning: incurs allocations when changed. */
        public void SetRayCount(int rayCount)
        {
            if (rayCount < MinRayCount || rayCount > MaxRayCount)
            {
                throw new ArgumentException($"Ray count {rayCount} out of range [{MinRayCount},{MaxRayCount}]");
            }

            if (_results == null || _results.Length != rayCount)
            {
                _results = new RayHit[rayCount];
            }
        }

        /* Between which points and how many times should the line be divided for computing ray origins? */
        public void UpdatePositioning(Vector2 segmentStart, Vector2 segmentEnd, Vector2 rayDirection)
        {
            _segmentStart = segmentStart;
            _segmentEnd   = segmentEnd;
            _rayDirection = rayDirection.normalized;
        }

        /* In the same direction, cast out rays from each origin along the segment with given options. */
        public RayHitGroup Cast(LayerMask layerMask, float maxDistance)
        {
            RayHitGroup hitGroup = default;
            Vector2 offsetBetweenRays = RaySpacing * SegmentDirection;
            if (offsetBetweenRays == Vector2.zero)
            {
                Debug.LogWarning($"Insufficient spacing between ray origins {RaySpacing} - skipping casts");
                return new RayHitGroup(hitPercentage: 0f, hitDistance: 0f);
            }

            _rayCaster.LayerMask = layerMask;
            _rayCaster.MaxDistance = maxDistance;
            for (int rayIndex = 0; rayIndex < _results.Length; rayIndex++)
            {
                Vector2 rayOrigin = _segmentStart + (rayIndex * offsetBetweenRays);
                _results[rayIndex] = _rayCaster.CastFromPoint(rayOrigin, _rayDirection);
            }

            // todo: properly compute actual results, and add result/standard-deviation/etc functionality
            return hitGroup;
        }
    }
}
