using UnityEngine;


namespace PQ.Common
{
    /*
    Provides functionality querying the surrounding of a bounding box.

    For example, is there something X distance in front of me?
    What about the front half of the box's bottom side?
    */
    public sealed class RayCasterBox
    {
        private Vector2 _center;
        private Vector2 _xAxis;
        private Vector2 _yAxis;
        private KinematicBody2D _body;
        private RayCasterSegment _backSensor;
        private RayCasterSegment _frontSensor;
        private RayCasterSegment _bottomSensor;
        private RayCasterSegment _topSensor;

        public struct Result
        {
            public readonly float hitPercentage;
            public readonly float hitDistance;

            public Result(float hitPercentage, float hitDistance)
            {
                this.hitPercentage = hitPercentage;
                this.hitDistance = hitDistance;
            }
        }
        
        
        public Vector2 Center      => _center;
        public Vector2 ForwardAxis => _xAxis;
        public Vector2 UpAxis      => _yAxis;

        public (Vector2, Vector2) BackSide   => (_backSensor.SegmentStart,   _backSensor.SegmentEnd);
        public (Vector2, Vector2) FrontSide  => (_frontSensor.SegmentStart,  _frontSensor.SegmentEnd);
        public (Vector2, Vector2) BottomSide => (_bottomSensor.SegmentStart, _bottomSensor.SegmentEnd);
        public (Vector2, Vector2) TopSide    => (_topSensor.SegmentStart,    _topSensor.SegmentEnd);

        public override string ToString() =>
            $"{GetType().Name}{{" +
                $"Back{_backSensor}, " +
                $"Front{_frontSensor}, " +
                $"Bottom{_bottomSensor}, " +
                $"Top{_topSensor}}}";


        public RayCasterBox(KinematicBody2D body)
        {
            _body = body;
            _backSensor   = new();
            _frontSensor  = new();
            _bottomSensor = new();
            _topSensor    = new();
        }

        public void SetBehindRayCount(int rayCount) => _backSensor.SetRayCount(rayCount);
        public void SetFrontRayCount(int rayCount)  => _frontSensor.SetRayCount(rayCount);
        public void SetBelowRayCount(int rayCount)  => _bottomSensor.SetRayCount(rayCount);
        public void SetAboveRayCount(int rayCount)  => _topSensor.SetRayCount(rayCount);
        public void SetAllRayCounts(int rayCount)
        {
            _backSensor  .SetRayCount(rayCount);
            _frontSensor .SetRayCount(rayCount);
            _bottomSensor.SetRayCount(rayCount);
            _topSensor   .SetRayCount(rayCount);
        }

        public Result CheckBehind(LayerMask target, float distance) => Cast(_backSensor,   target, distance);
        public Result CheckFront(LayerMask target,  float distance) => Cast(_frontSensor,  target, distance);
        public Result CheckAbove(LayerMask target,  float distance) => Cast(_topSensor,    target, distance);
        public Result CheckBelow(LayerMask target,  float distance) => Cast(_bottomSensor, target, distance);


        private Result Cast(RayCasterSegment caster, LayerMask layerMask, float distanceToCast)
        {
            // todo: properly compute actual results, and add result/standard-deviation/etc functionality
            UpdateBounds();

            caster.Cast(layerMask, distanceToCast);
            var results = caster.RayCastResults;
            return new Result(hitPercentage: 0.50f, hitDistance: 0.25f);
        }

        private void UpdateBounds()
        {
            Vector2 center = _body.Position;
            Vector2 xAxis  = _body.BoundExtents.x * _body.Forward;
            Vector2 yAxis  = _body.BoundExtents.y * _body.Up;

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
            _backSensor  .UpdatePositioning(segmentStart: rearBottom,  segmentEnd: rearTop,     rayDirection: -xAxis);
            _frontSensor .UpdatePositioning(segmentStart: frontBottom, segmentEnd: frontTop,    rayDirection:  xAxis);
            _bottomSensor.UpdatePositioning(segmentStart: rearBottom,  segmentEnd: frontBottom, rayDirection: -yAxis);
            _topSensor   .UpdatePositioning(segmentStart: rearTop,     segmentEnd: frontTop,    rayDirection:  yAxis);
        }
    }
}
