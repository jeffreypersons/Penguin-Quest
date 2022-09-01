using UnityEngine;
using PQ.Common.Extensions;


namespace PQ.Common
{
    /*
    Provides functionality querying the surrounding of a bounding box.

    For example, is there something X distance in front of me?
    What about the front half of the box's bottom side?
    */
    public class RayCasterBox : MonoBehaviour
    {
        private Vector2 _center;
        private Vector2 _xAxis;
        private Vector2 _yAxis;
        private KinematicBody2D _physicsBody;
        private RayCasterSegment _backSensor;
        private RayCasterSegment _frontSensor;
        private RayCasterSegment _bottomSensor;
        private RayCasterSegment _topSensor;

        private bool _hasAnySpacingChanged;
        private float _backRaySpacing;
        private float _frontRaySpacing;
        private float _bottomRaySpacing;
        private float _topRaySpacing;
        private void SetSpacing(ref float field, float value)
        {
            if (!Mathf.Approximately(field, value))
            {
                field = value;
                _hasAnySpacingChanged = true;
            }
        }

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

        public float BackSensorSpacing   { get => _backRaySpacing;   set => SetSpacing(ref _backRaySpacing,   value); }
        public float FrontSensorSpacing  { get => _frontRaySpacing;  set => SetSpacing(ref _frontRaySpacing,  value); }
        public float BottomSensorSpacing { get => _bottomRaySpacing; set => SetSpacing(ref _bottomRaySpacing, value); }
        public float TopSensorSpacing    { get => _topRaySpacing;    set => SetSpacing(ref _topRaySpacing,    value); }

        public Result CheckBehind(LayerMask target, float distance) => Cast(_backSensor,   target, distance);
        public Result CheckFront(LayerMask target,  float distance) => Cast(_frontSensor,  target, distance);
        public Result CheckAbove(LayerMask target,  float distance) => Cast(_topSensor,    target, distance);
        public Result CheckBelow(LayerMask target,  float distance) => Cast(_bottomSensor, target, distance);


        void Awake()
        {
            _physicsBody = gameObject.GetComponent<KinematicBody2D>();
            _backSensor   = new();
            _frontSensor  = new();
            _bottomSensor = new();
            _topSensor    = new();
        }

        void Start()
        {
            UpdateAll();
        }

        void Update()
        {
            UpdateAll();
        }

        private Result Cast(RayCasterSegment caster, LayerMask layerMask, float distanceToCast)
        {
            // todo: properly compute actual results, and add result/standard-deviation/etc functionality
            caster.UpdateCastOptions(layerMask, distanceToCast);
            caster.CastAll();
            var results = caster.RayCastResults;
            return new Result(hitPercentage: 0.50f, hitDistance: 0.25f);
        }

        private void UpdateAll()
        {
            SetBounds(
                center: _physicsBody.Position,
                xAxis:  _physicsBody.BoundExtents.x * _physicsBody.Forward,
                yAxis:  _physicsBody.BoundExtents.y * _physicsBody.Up);
        }

        private void SetBounds(Vector2 center, Vector2 xAxis, Vector2 yAxis)
        {
            // since change in bounds or ray spacing can result in different ray counts,
            // only update updating positioning when we have to
            if (!_hasAnySpacingChanged &&
                center == _center &&
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
            _hasAnySpacingChanged = false;
            _backSensor  .UpdateCastDirection(-xAxis);
            _frontSensor .UpdateCastDirection( xAxis);
            _bottomSensor.UpdateCastDirection(-yAxis);
            _topSensor   .UpdateCastDirection( yAxis);
            _backSensor  .UpdatePositioning(rearBottom,  rearTop,     _backRaySpacing);
            _frontSensor .UpdatePositioning(frontBottom, frontTop,    _frontRaySpacing);
            _bottomSensor.UpdatePositioning(rearBottom,  frontBottom, _bottomRaySpacing);
            _topSensor   .UpdatePositioning(rearTop,     frontTop,    _topRaySpacing);
        }

        #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!Application.IsPlaying(this) || !enabled)
            {
                return;
            }

            // draw a bounding box that should be identical to the BoxCollider2D bounds in the editor window
            GizmoExtensions.DrawLine(_backSensor  .SegmentStart, _backSensor  .SegmentEnd, Color.gray);
            GizmoExtensions.DrawLine(_frontSensor .SegmentStart, _frontSensor .SegmentEnd, Color.gray);
            GizmoExtensions.DrawLine(_bottomSensor.SegmentStart, _bottomSensor.SegmentEnd, Color.gray);
            GizmoExtensions.DrawLine(_topSensor   .SegmentStart, _topSensor   .SegmentEnd, Color.gray);

            // draw a pair of arrows from the that should be identical to the transform's axes in the editor window
            GizmoExtensions.DrawArrow(from: _center, to: _center + _xAxis, color: Color.red);
            GizmoExtensions.DrawArrow(from: _center, to: _center + _yAxis, color: Color.green);
        }
        #endif
    }
}
