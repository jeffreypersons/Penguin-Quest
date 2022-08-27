﻿using System;
using UnityEngine;


namespace PQ.Common.Casts
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

        private RayCaster  _rayCaster;
        private RayCaster.Hit?[] _results;


        public const int MinNumRays = 1;
        public const int MaxNumRays = 10000;
        public ReadOnlySpan<RayCaster.Hit?> RayCastResults => _results.AsSpan();

        public Vector2   SegmentStart    => _segmentStart;
        public Vector2   SegmentEnd      => _segmentEnd;
        public float     SegmentLength   => (_segmentEnd - _segmentStart).magnitude;
        public int       RayCount        => _results.Length;
        public float     RaySpacing      => _offsetBetweenRays.magnitude;
        public Vector2   RayDirection    => _rayDir;
        public float     RayLength       => _rayCaster.MaxDistance;
        public LayerMask RayTargetLayers => _rayCaster.LayerMask;


        public RayCasterSegment(Vector2 start, Vector2 end, float distanceBetweenRays, Vector2 castDirection)
        {
            _results   = Array.Empty<RayCaster.Hit?>();
            _rayCaster = new RayCaster();
            UpdatePositioning(start, end, distanceBetweenRays);
            UpdateCastParams(castDirection, layerMask: ~0, maxDistance: Mathf.Infinity);
        }

        /* In the same direction, cast out rays from each origin along the segment. */
        public void CastAll()
        {
            for (int rayIndex = 0; rayIndex < _results.Length; rayIndex++)
            {
                Cast(rayIndex);
            }
        }

        /* Between which points and with how much gap should ray origins be placed? */
        public void UpdatePositioning(Vector2 start, Vector2 end, float distanceBetweenRays)
        {
            Vector2 segment          = end - start;
            float   segmentDistance  = segment.magnitude;
            Vector2 segmentDirection = segment.normalized;

            int   rayCount   = Mathf.Clamp(Mathf.RoundToInt(segmentDistance / distanceBetweenRays), MinNumRays, MaxNumRays);
            float raySpacing = segmentDistance / (rayCount - 1);

            _segmentStart      = start;
            _segmentEnd        = end;
            _offsetBetweenRays = raySpacing * segmentDirection;
            if (_results.Length != rayCount)
            {
                _results = new RayCaster.Hit?[rayCount];
            }
        }

        /* In what direction, what layers, and for what distance should we cast rays? */
        public void UpdateCastParams(Vector2 direction, LayerMask layerMask, float maxDistance)
        {
            _rayDir                = direction;
            _rayCaster.LayerMask   = layerMask;
            _rayCaster.MaxDistance = maxDistance;
        }


        private void Cast(int rayIndex)
        {
            Vector2 rayOrigin = _segmentStart + (rayIndex * _offsetBetweenRays);
            _rayCaster.CastFromPoint(rayOrigin, _rayDir);
        }
    }
}
