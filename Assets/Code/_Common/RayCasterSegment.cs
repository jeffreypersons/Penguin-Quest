﻿using System;
using UnityEngine;


namespace PQ.Common
{
    /*
    Provides a streamlined interface for casting rays from a series of line casters along a segment.
    */
    public class RayCasterSegment
    {
        private Vector2 _rayDir;
        private Vector2 _segmentStart;
        private Vector2 _segmentEnd;
        private Vector2 _offsetBetweenRays;

        private RayCaster _rayCaster;
        private RayHit?[] _results;


        public const int MinNumRays = 1;
        public const int MaxNumRays = 10000;
        public ReadOnlySpan<RayHit?> RayCastResults => _results.AsSpan();

        public Vector2   SegmentStart    => _segmentStart;
        public Vector2   SegmentMid      => Vector2.Lerp(_segmentStart, _segmentEnd, 0.50f);
        public Vector2   SegmentEnd      => _segmentEnd;
        public float     SegmentLength   => (_segmentEnd - _segmentStart).magnitude;
        public int       RayCount        => _results.Length;
        public float     RaySpacing      => _offsetBetweenRays.magnitude;
        public Vector2   RayDirection    => _rayDir;
        public float     RayDistance     => _rayCaster.MaxDistance;
        public LayerMask RayTargetLayers => _rayCaster.LayerMask;

        public override string ToString() =>
            $"{GetType().Name}:" +
                $"Segment{{" +
                    $"start:{SegmentStart}," +
                    $"end:{SegmentEnd}}}," +
                    $"length:{SegmentLength}}}, " +
                $"RayOrigins{{" +
                    $"spacing:{RaySpacing}," +
                    $"count:{RayCount}}}, " +
                $"RayCasts{{" +
                    $"direction:{RayDirection}," +
                    $"distance:{RayDistance}," +
                    $"layerMask:{RayTargetLayers}}}";

        public RayCasterSegment() :
            this(Vector2.zero, Vector2.zero, distanceBetweenRays: 0.5f, Vector2.right)
        { }

        public RayCasterSegment(Vector2 start, Vector2 end, float distanceBetweenRays, Vector2 castDirection)
        {
            _results   = Array.Empty<RayHit?>();
            _rayCaster = new RayCaster();
            UpdatePositioning(start, end, distanceBetweenRays);
            UpdateCastDirection(castDirection);
            UpdateCastOptions(layerMask: ~0, maxDistance: Mathf.Infinity);
        }

        /* Between which points and with how much gap should ray origins be placed? */
        public void UpdatePositioning(Vector2 start, Vector2 end, float distanceBetweenRays)
        {
            Vector2 segment          = end - start;
            float   segmentDistance  = segment.magnitude;
            Vector2 segmentDirection = segment.normalized;

            int rayCount;
            float raySpacing;
            if (segmentDirection != Vector2.zero ||
                Mathf.Approximately(segmentDistance, 0f) ||
                Mathf.Approximately(distanceBetweenRays, 0f))
            {
                rayCount   = 1;
                raySpacing = 0.5f;
            }
            else
            {
                rayCount   = Mathf.RoundToInt(segmentDistance / distanceBetweenRays);
                raySpacing = segmentDistance / (rayCount - 1);
            }

            _segmentStart      = start;
            _segmentEnd        = end;
            _offsetBetweenRays = raySpacing * segmentDirection;
            if (_results.Length != rayCount)
            {
                _results = new RayHit?[rayCount];
            }
        }

        /* In what direction should we cast rays? */
        public void UpdateCastDirection(Vector2 rayDirection)
        {
            _rayDir = rayDirection.normalized;
        }

        /* What layers, and for what distance should we cast rays? */
        public void UpdateCastOptions(LayerMask layerMask, float maxDistance)
        {
            _rayCaster.LayerMask   = layerMask;
            _rayCaster.MaxDistance = maxDistance;
        }

        /* In the same direction, cast out rays from each origin along the segment. */
        public void CastAll()
        {
            for (int rayIndex = 0; rayIndex < _results.Length; rayIndex++)
            {
                Cast(rayIndex);
            }
        }

        private void Cast(int rayIndex)
        {
            Vector2 rayOrigin = _segmentStart + (rayIndex * _offsetBetweenRays);
            _rayCaster.CastFromPoint(rayOrigin, _rayDir);
        }
    }
}
