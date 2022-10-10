using System;
using UnityEngine;
using PQ.Common.Physics;


namespace PQ.Common.Casts
{
    /*
    Provides functionality querying the surrounding of a bounding box.

    For example, is there something X distance in front of me?
    What about the front half of the box's bottom side?
    */
    public sealed class RayCasterBox
    {
        private bool _boundsAreZero;
        private Vector2 _center;
        private Vector2 _xAxis;
        private Vector2 _yAxis;
        private KinematicBody2D _body;
        private RayCasterSegment _backCaster;
        private RayCasterSegment _frontCaster;
        private RayCasterSegment _bottomSensor;
        private RayCasterSegment _topCaster;

        public Vector2 Center      => _center;
        public Vector2 ForwardAxis => _xAxis;
        public Vector2 UpAxis      => _yAxis;

        public (Vector2, Vector2) BackSide   => (_backCaster.SegmentStart,   _backCaster.SegmentEnd);
        public (Vector2, Vector2) FrontSide  => (_frontCaster.SegmentStart,  _frontCaster.SegmentEnd);
        public (Vector2, Vector2) BottomSide => (_bottomSensor.SegmentStart, _bottomSensor.SegmentEnd);
        public (Vector2, Vector2) TopSide    => (_topCaster.SegmentStart,    _topCaster.SegmentEnd);

        public override string ToString() =>
            $"{GetType().Name}{{" +
                $"Back{_backCaster}, " +
                $"Front{_frontCaster}, " +
                $"Bottom{_bottomSensor}, " +
                $"Top{_topCaster}}}";


        public RayCasterBox(KinematicBody2D body)
        {
            _body = body;
            _backCaster   = new();
            _frontCaster  = new();
            _bottomSensor = new();
            _topCaster    = new();
        }

        public RayHit CheckBehind(float t, LayerMask mask, float distance) => Cast(_backCaster,   t, mask, distance);
        public RayHit CheckFront(float t, LayerMask mask,  float distance) => Cast(_frontCaster,  t, mask, distance);
        public RayHit CheckAbove(float t, LayerMask mask,  float distance) => Cast(_topCaster,    t, mask, distance);
        public RayHit CheckBelow(float t, LayerMask mask,  float distance) => Cast(_bottomSensor, t, mask, distance);


        private RayHit Cast(RayCasterSegment caster, float t, LayerMask layerMask, float distanceToCast)
        {
            UpdateBoundsIfChanged();

            if (_boundsAreZero)
            {
                throw new InvalidOperationException("Bounds cannot be zero");
            }
            return caster.CastAt(t, layerMask, distanceToCast);
        }

        private void UpdateBoundsIfChanged()
        {
            Vector2 center;
            Vector2 xAxis;
            Vector2 yAxis;
            if (_body.BoundExtents == Vector2.zero)
            {
                _boundsAreZero = true;
                center = _body.Position;
                xAxis  = _body.Forward;
                yAxis  = _body.Up;
            }
            else
            {
                _boundsAreZero = false;
                center = _body.Position;
                xAxis  = _body.BoundExtents.x * _body.Forward;
                yAxis  = _body.BoundExtents.y * _body.Up;
            }
            
            if (center == _center &&
                Mathf.Approximately(xAxis.x, _xAxis.x) && Mathf.Approximately(xAxis.y, _xAxis.y) &&
                Mathf.Approximately(yAxis.x, _yAxis.x) && Mathf.Approximately(yAxis.y, _yAxis.y))
            {
                return;
            }
            
            Vector2 min = center - xAxis - yAxis;
            Vector2 max = center + xAxis + yAxis;
            Vector2 rearBottom  = new(min.x, min.y);
            Vector2 rearTop     = new(min.x, max.y);
            Vector2 frontBottom = new(max.x, min.y);
            Vector2 frontTop    = new(max.x, max.y);

            _center = center;
            _xAxis  = xAxis;
            _yAxis  = yAxis;
            _backCaster  .UpdatePositioning(segmentStart: rearBottom,  segmentEnd: rearTop,     rayDirection: -xAxis);
            _frontCaster .UpdatePositioning(segmentStart: frontBottom, segmentEnd: frontTop,    rayDirection:  xAxis);
            _bottomSensor.UpdatePositioning(segmentStart: rearBottom,  segmentEnd: frontBottom, rayDirection: -yAxis);
            _topCaster   .UpdatePositioning(segmentStart: rearTop,     segmentEnd: frontTop,    rayDirection:  yAxis);
        }
    }
}
