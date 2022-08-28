using UnityEngine;
using PQ.Common.Extensions;
using PQ.Common.Casts;


namespace PQ.Common
{
    /*
    Provides functionality querying the surrounding of a bounding box.

    For example, is there something X distance in front of me?
    What about the front half of the box's bottom side?
    */
    public class Collisions2D : MonoBehaviour
    {
        public struct Settings
        {
            public readonly float distanceToCast;
            public readonly float distanceBetweenRays;
            public readonly LayerMask layerMask;

            public Settings(float distanceToCast, float distanceBetweenRays, LayerMask layerMask)
            {
                this.distanceToCast      = distanceToCast;
                this.distanceBetweenRays = distanceBetweenRays;
                this.layerMask           = layerMask;
            }
        }
        public struct Result
        {
            public readonly float hitPercentage;
            public readonly float hitDistance;

            public Result(float hitPercentage, float hitDistance)
            {
                this.hitPercentage = hitPercentage;
                this.hitDistance   = hitDistance;
            }
        }

        private Vector2 _center;
        private Vector2 _xAxis;
        private Vector2 _yAxis;
        private PhysicsBody2D _physicsBody;
        private RayCasterSegment _backSensor;
        private RayCasterSegment _frontSensor;
        private RayCasterSegment _bottomSensor;
        private RayCasterSegment _topSensor;
        
        public Settings BackSensorSettings   { get; set; }
        public Settings FrontSensorSettings  { get; set; }
        public Settings BottomSensorSettings { get; set; }
        public Settings TopSensorSettings    { get; set; }
        
        public Result BackSensorResults   { get; set; }
        public Result FrontSensorResults  { get; set; }
        public Result BottomSensorResults { get; set; }
        public Result TopSensorResults    { get; set; }

        void Awake()
        {
            _physicsBody = gameObject.GetComponent<PhysicsBody2D>();
            _backSensor   = new();
            _frontSensor  = new();
            _bottomSensor = new();
            _topSensor    = new();
        }

        void Start()
        {
            UpdateAll();
        }

        void FixedUpdate()
        {
            _backSensor  .CastAll();
            _frontSensor .CastAll();
            _bottomSensor.CastAll();
            _topSensor   .CastAll();
        }

        void Update()
        {
            UpdateAll();
        }


        private void UpdateAll()
        {
            _center = _physicsBody.Position;
            _xAxis  = _physicsBody.BoundExtents.x * _physicsBody.Forward;
            _yAxis  = _physicsBody.BoundExtents.y * _physicsBody.Up;
            if (_xAxis == Vector2.zero || _yAxis == Vector2.zero)
            {
                return;
            }

            Vector2 rearBottom  = _center - _xAxis - _yAxis;
            Vector2 rearTop     = _center - _xAxis + _yAxis;
            Vector2 frontBottom = _center + _xAxis - _yAxis;
            Vector2 frontTop    = _center + _xAxis + _yAxis;
            UpdateCaster(
                caster:       _backSensor,
                settings:     BackSensorSettings,
                start:        rearBottom,
                end:          rearTop,
                rayDirection: -_xAxis);

            UpdateCaster(
                caster:       _frontSensor,
                settings:     FrontSensorSettings,
                start:        frontBottom,
                end:          frontTop,
                rayDirection: _xAxis);

            UpdateCaster(
                caster:       _bottomSensor,
                settings:     BottomSensorSettings,
                start:        rearBottom,
                end:          frontBottom,
                rayDirection: -_yAxis);

            UpdateCaster(
                caster:       _topSensor,
                settings:     TopSensorSettings,
                start:        rearTop,
                end:          frontTop,
                rayDirection: _yAxis);
        }


        private static void UpdateCaster(RayCasterSegment caster, Settings settings,
            Vector2 start, Vector2 end, Vector2 rayDirection)
        {
            caster.UpdateCastParams(rayDirection, settings.layerMask, settings.distanceToCast);
            caster.UpdatePositioning(start, end, settings.distanceBetweenRays);
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
