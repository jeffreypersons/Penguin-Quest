using System;
using UnityEngine;
using PQ.Common.Extensions;
using PQ.Common.Collisions;
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

        private RayCasterSegment _backSideCaster;
        private RayCasterSegment _frontSideCaster;
        private RayCasterSegment _bottomSideCaster;
        private RayCasterSegment _topSideCaster;

        private BoxCollider2D _boundingBox;
        private OrientedBoundingBox _bounds;


        void Awake()
        {
            _boundingBox = gameObject.GetComponent<BoxCollider2D>();
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
            if (!_boundingBox)
            {
                return;
            }

            _bounds.Update();

            UpdateCaster(
                caster:       _backSideCaster,
                settings:     _backSideCasterSettings,
                start:        _bounds.RearBottom,
                end:          _bounds.RearTop,
                rayDirection: _bounds.Back);

            UpdateCaster(
                caster:       _frontSideCaster,
                settings:     _frontSideCasterSettings,
                start:        _bounds.FrontBottom,
                end:          _bounds.FrontTop,
                rayDirection: _bounds.Forward);

            UpdateCaster(
                caster:       _bottomSideCaster,
                settings:     _bottomSideCasterSettings,
                start:        _bounds.RearBottom,
                end:          _bounds.FrontBottom,
                rayDirection: _bounds.Down);

            UpdateCaster(
                caster:       _topSideCaster,
                settings:     _topSideCasterSettings,
                start:        _bounds.RearTop,
                end:          _bounds.FrontTop,
                rayDirection: _bounds.Up);
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

            //GizmoExtensions.DrawArrow(from: _bounds.Center, to: _bounds.Center + _bounds.AxisX, color: Color.red);
            //GizmoExtensions.DrawArrow(from: _bounds.Center, to: _bounds.Center + _bounds.AxisY, color: Color.green);
            //GizmoExtensions.DrawRect(center, xAxis, yAxis, Color.gray);
        }
        #endif
    }
}
