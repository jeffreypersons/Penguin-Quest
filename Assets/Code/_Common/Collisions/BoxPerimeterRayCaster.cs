using System;
using UnityEngine;
using PQ.Common.Casts;


namespace PQ.Common.Collisions
{
    public class BoxPerimeterRayCaster
    {
        private const int minNumRays = 0;
        private const int maxNumRays = 10000;

        private int _bottomStartIndex;
        private int _topStartIndex;
        private int _leftStartIndex;
        private int _rightStartIndex;
        private CastResult[] _results;
        
        private BoxCollider2D  _box;
        private OrientedBoundingBox _originBounds;
        private LineCaster _lineCaster;


        public RayCasterSettings Settings { get; set; }
        public Vector2 Center  => _originBounds.Center;
        public Vector2 Forward => _originBounds.AxisX;
        public Vector2 Up      => _originBounds.AxisY;

        public float   RaySpacingHorizontalSide { get; private set; }
        public float   RaySpacingVerticalSide   { get; private set; }
        public int     NumRaysPerHorizontalSide { get; private set; }
        public int     NumRaysPerVerticalSide   { get; private set; }
        public int     TotalNumRays             { get; private set; }

        public ReadOnlySpan<CastResult> AllResults    => _results.AsSpan(0,                 TotalNumRays);
        public ReadOnlySpan<CastResult> BottomResults => _results.AsSpan(_bottomStartIndex, NumRaysPerHorizontalSide);
        public ReadOnlySpan<CastResult> TopResults    => _results.AsSpan(_topStartIndex,    NumRaysPerHorizontalSide);
        public ReadOnlySpan<CastResult> LeftResults   => _results.AsSpan(_leftStartIndex,   NumRaysPerVerticalSide);
        public ReadOnlySpan<CastResult> RightResults  => _results.AsSpan(_rightStartIndex,  NumRaysPerVerticalSide);
        public override string ToString() =>
            $"{GetType().Name}:" +
                $"RayCasts{{" +
                    $"totalCount:{TotalNumRays}," +
                    $"settings:{Settings.name}}}, " +
                $"RaysPerHorizontalSide{{" +
                    $"spacing:{RaySpacingHorizontalSide}," +
                    $"count:{NumRaysPerHorizontalSide}}}, " +
                $"RaysPerVerticalSide{{" +
                    $"spacing:{RaySpacingVerticalSide}," +
                    $"count:{NumRaysPerVerticalSide}}}, " +
                $"OrientedBounds{{" +
                    $"{_originBounds}}}";

        public BoxPerimeterRayCaster(BoxCollider2D box, RayCasterSettings settings)
        {
            _box          = box;
            Settings      = settings;

            _lineCaster   = new LineCaster(Settings);
            _originBounds = new OrientedBoundingBox(_box);
            _results      = Array.Empty<CastResult>();

            _originBounds.Update();
            ComputeRaySpacingAndCounts(settings.DistanceBetweenRays, _originBounds.Size);
            Debug.Log(this);
        }

        public void CastAll()
        {
            if (!_box)
            {
                return;
            }

            _originBounds.Update();
            ComputeRaySpacingAndCounts(Settings.DistanceBetweenRays, _originBounds.Size);

            Vector2 horizontalStep = RaySpacingHorizontalSide * _originBounds.Forward;
            for (int i = 0; i < NumRaysPerHorizontalSide; i++)
            {
                Vector2 offsetFromLeftSide = (i * horizontalStep);
                _results[_bottomStartIndex + i] = Cast(_originBounds.RearBottom + offsetFromLeftSide, _originBounds.Down);
                _results[_topStartIndex    + i] = Cast(_originBounds.RearTop    + offsetFromLeftSide, _originBounds.Up);
            }

            Vector2 verticalStep = RaySpacingVerticalSide * _originBounds.Up;
            for (int i = 0; i < NumRaysPerVerticalSide; i++)
            {
                Vector2 offsetFromBottomSide = (i * verticalStep);
                _results[_leftStartIndex  + i] = Cast(_originBounds.RearBottom  + offsetFromBottomSide, _originBounds.Back);
                _results[_rightStartIndex + i] = Cast(_originBounds.FrontBottom + offsetFromBottomSide, _originBounds.Forward);
            }
        }

        private CastResult Cast(Vector2 origin, Vector2 direction)
        {
            return _lineCaster.CastFromPoint(origin, direction);
        }

        private void ComputeRaySpacingAndCounts(float distanceBetweenRays, Vector2 size)
        {
            int numRaysPerHorizontalSide = Mathf.RoundToInt(size.x / distanceBetweenRays);
            int numRaysPerVerticalSide   = Mathf.RoundToInt(size.y / distanceBetweenRays);
            if (NumRaysPerHorizontalSide != numRaysPerHorizontalSide ||
                NumRaysPerVerticalSide   != numRaysPerVerticalSide)
            {
                NumRaysPerHorizontalSide = Mathf.Clamp(numRaysPerHorizontalSide, minNumRays, maxNumRays);
                NumRaysPerVerticalSide   = Mathf.Clamp(numRaysPerVerticalSide,   minNumRays, maxNumRays);
                TotalNumRays = 2 * (NumRaysPerHorizontalSide + NumRaysPerVerticalSide);
            }

            RaySpacingHorizontalSide = size.x / (NumRaysPerHorizontalSide - 1);
            RaySpacingVerticalSide   = size.y / (NumRaysPerVerticalSide   - 1);

            _bottomStartIndex = 0;
            _topStartIndex    = _bottomStartIndex + NumRaysPerHorizontalSide;
            _leftStartIndex   = _topStartIndex    + NumRaysPerHorizontalSide;
            _rightStartIndex  = _leftStartIndex   + NumRaysPerVerticalSide;

            if (_results.Length != TotalNumRays)
            {
                _results = new CastResult[TotalNumRays];
            }
        }
    }
}
