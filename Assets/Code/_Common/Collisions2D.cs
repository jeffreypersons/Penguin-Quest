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
        public struct SensorSettings
        {
            public float     offset;
            public float     distance;
            public float     distanceBetweenRays;
            public LayerMask layerMask;
        }

        [Header("Sensor Settings")]
        [SerializeField] private SensorSettings _backSideCasterSettings;
        [SerializeField] private SensorSettings _frontSideCasterSettings;
        [SerializeField] private SensorSettings _bottomSideCasterSettings;
        [SerializeField] private SensorSettings _topSideCasterSettings;

        private Vector2 _center;
        private Vector2 _xAxis;
        private Vector2 _yAxis;
        private PhysicsBody2D _physicsBody;
        private RayCasterSegment _backSideCaster;
        private RayCasterSegment _frontSideCaster;
        private RayCasterSegment _bottomSideCaster;
        private RayCasterSegment _topSideCaster;


        void Awake()
        {
            _physicsBody = gameObject.GetComponent<PhysicsBody2D>();
            _backSideCaster   = new();
            _frontSideCaster  = new();
            _bottomSideCaster = new();
            _topSideCaster    = new();
        }

        void Start()
        {
            UpdateAll();
        }

        void FixedUpdate()
        {
            _backSideCaster  .CastAll();
            _frontSideCaster .CastAll();
            _bottomSideCaster.CastAll();
            _topSideCaster   .CastAll();
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
                caster:       _backSideCaster,
                settings:     _backSideCasterSettings,
                start:        rearBottom,
                end:          rearTop,
                rayDirection: -_xAxis);

            UpdateCaster(
                caster:       _frontSideCaster,
                settings:     _frontSideCasterSettings,
                start:        frontBottom,
                end:          frontTop,
                rayDirection: _xAxis);

            UpdateCaster(
                caster:       _bottomSideCaster,
                settings:     _bottomSideCasterSettings,
                start:        rearBottom,
                end:          frontBottom,
                rayDirection: -_yAxis);

            UpdateCaster(
                caster:       _topSideCaster,
                settings:     _topSideCasterSettings,
                start:        rearTop,
                end:          frontTop,
                rayDirection: _yAxis);
        }


        private static void UpdateCaster(RayCasterSegment caster, SensorSettings settings,
            Vector2 start, Vector2 end, Vector2 rayDirection)
        {
            caster.UpdateCastParams(rayDirection, settings.layerMask, settings.distance);
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
            GizmoExtensions.DrawLine(_backSideCaster  .SegmentStart, _backSideCaster  .SegmentEnd, Color.gray);
            GizmoExtensions.DrawLine(_frontSideCaster .SegmentStart, _frontSideCaster .SegmentEnd, Color.gray);
            GizmoExtensions.DrawLine(_bottomSideCaster.SegmentStart, _bottomSideCaster.SegmentEnd, Color.gray);
            GizmoExtensions.DrawLine(_topSideCaster   .SegmentStart, _topSideCaster   .SegmentEnd, Color.gray);

            // draw a pair of arrows from the that should be identical to the transform's axes in the editor window
            GizmoExtensions.DrawArrow(from: _center, to: _center + _xAxis, color: Color.red);
            GizmoExtensions.DrawArrow(from: _center, to: _center + _yAxis, color: Color.green);
        }
        #endif
    }
}
