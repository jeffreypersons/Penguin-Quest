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
        private struct Side
        {
            public readonly Vector2 start;
            public readonly Vector2 end;
            public readonly Vector2 normal;
            public Side(in Vector2 start, in Vector2 end, in Vector2 normal)
            {
                this.start  = start;
                this.end    = end;
                this.normal = normal;
            }
        }

        private bool _boundsAreZero;
        private Vector2 _center;
        private Vector2 _xAxis;
        private Vector2 _yAxis;

        private KinematicBody2D _body;
        private RayCaster _caster;

        private Side _backSide;
        private Side _frontSide;
        private Side _bottomSide;
        private Side _topSide;

        public Vector2 Center      => _center;
        public Vector2 ForwardAxis => _xAxis;
        public Vector2 UpAxis      => _yAxis;

        public (Vector2, Vector2) BackSide   => (_backSide.start,   _backSide.end);
        public (Vector2, Vector2) FrontSide  => (_frontSide.start,  _frontSide.end);
        public (Vector2, Vector2) BottomSide => (_bottomSide.start, _bottomSide.end);
        public (Vector2, Vector2) TopSide    => (_topSide.start,    _topSide.end);

        public override string ToString() =>
            $"{GetType().Name}(" +
                $"center:{_center}," +
                $"xAxis:{_xAxis}," +
                $"yAxis:{_yAxis})";


        public RayCasterBox(KinematicBody2D body)
        {
            _body       = body;
            _caster     = new();
            _backSide   = new();
            _frontSide  = new();
            _bottomSide = new();
            _topSide    = new();
        }

        public bool DrawCastInEditor { get => _caster.DrawCastInEditor; set => _caster.DrawCastInEditor = value; }

        public RayHit CastBehind(float t, in LayerMask mask, float distance) => Cast(_backSide,   t, mask, distance);
        public RayHit CastFront(float t,  in LayerMask mask, float distance) => Cast(_frontSide,  t, mask, distance);
        public RayHit CastBelow(float t,  in LayerMask mask, float distance) => Cast(_bottomSide, t, mask, distance);
        public RayHit CastAbove(float t,  in LayerMask mask, float distance) => Cast(_topSide,    t, mask, distance);


        /* Perform a one off ray cast at given t in range [-1,1]. */
        private RayHit Cast(in Side side, float t, in LayerMask layerMask, float distance)
        {
            if (t < -1f || t > 1f)
            {
                throw new ArgumentOutOfRangeException($"Given t {t} is outside segment [-1,1] - skipping cast");
            }

            UpdateBoundsIfChanged();

            if (_boundsAreZero)
            {
                throw new InvalidOperationException("Bounds cannot be zero");
            }

            Vector2 rayOrigin = Vector2.Lerp(side.start, side.end, t);
            return _caster.CastFromPoint(rayOrigin, side.normal, layerMask, distance);
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

            _center     = center;
            _xAxis      = xAxis;
            _yAxis      = yAxis;
            _backSide   = new(start: rearBottom,  end: rearTop,     normal: (-xAxis).normalized);
            _frontSide  = new(start: frontBottom, end: frontTop,    normal: xAxis.normalized);
            _bottomSide = new(start: rearBottom,  end: frontBottom, normal: (-yAxis).normalized);
            _topSide    = new(start: rearTop,     end: frontTop,    normal: yAxis.normalized);
        }
    }
}
