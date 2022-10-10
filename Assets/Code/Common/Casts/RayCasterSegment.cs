using UnityEngine;


namespace PQ.Common.Casts
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

        public const int MinRayCount = 3;
        public const int MaxRayCount = 1000;

        public Vector2   SegmentStart     => _segmentStart;
        public Vector2   SegmentMid       => Vector2.Lerp(_segmentStart, _segmentEnd, 0.50f);
        public Vector2   SegmentEnd       => _segmentEnd;
        public Vector2   SegmentDirection => (_segmentEnd - _segmentStart).normalized;
        public float     SegmentLength    => (_segmentEnd - _segmentStart).magnitude;

        public Vector2   RayDirection     => _rayDirection;
        public LayerMask RayTargetLayers  => _rayCaster.LayerMask;

        public override string ToString() =>
            $"{GetType().Name}{{" +
                $"endpoints[{SegmentStart},{SegmentEnd}], " +
                $"rayDirection:{RayDirection}}}";


        public RayCasterSegment() :
            this(segmentStart: Vector2.zero, segmentEnd: Vector2.zero, rayCount: MinRayCount, Vector2.right)
        { }

        public RayCasterSegment(Vector2 segmentStart, Vector2 segmentEnd, int rayCount, Vector2 rayDirection)
        {
            _rayCaster = new RayCaster { MaxDistance = Mathf.Infinity, LayerMask = ~0 };
            UpdatePositioning(segmentStart, segmentEnd, rayDirection);
        }

        /* Between which points and how many times should the line be divided for computing ray origins? */
        public void UpdatePositioning(Vector2 segmentStart, Vector2 segmentEnd, Vector2 rayDirection)
        {
            _segmentStart = segmentStart;
            _segmentEnd   = segmentEnd;
            _rayDirection = rayDirection.normalized;
        }

        /* Perform a one off ray cast at given t in range [0,1]. */
        public RayHit CastAt(float t, LayerMask layerMask, float maxDistance)
        {
            if (t < 0f || t > 1f)
            {
                Debug.LogWarning($"Given t {t} is outside segment [0,1] - skipping cast");
                return default;
            }

            Vector2 rayOrigin = Vector2.Lerp(_segmentStart, _segmentEnd, t);
            _rayCaster.LayerMask = layerMask;
            _rayCaster.MaxDistance = maxDistance;
            return _rayCaster.CastFromPoint(rayOrigin, _rayDirection);
        }
    }
}
