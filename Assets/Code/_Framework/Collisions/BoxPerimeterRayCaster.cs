using System;
using UnityEngine;
using PenguinQuest.Extensions;


namespace PenguinQuest.Framework.Collisions
{
    public class BoxPerimeterRayCaster
    {
        private int _bottomStartIndex;
        private int _topStartIndex;
        private int _leftStartIndex;
        private int _rightStartIndex;
        private CastResult[] _results;
        
        private BoxCollider2D  _box;
        private OrientedBounds _originBounds;
        private LineCaster     _lineCaster;

        private const int minNumRays = 0;
        private const int maxNumRays = 10000;

        public RayCasterSettings Settings { get; set; }
        public Vector2 CenterOfBounds => _originBounds.Center;
        public Vector2 SizeOfBounds   => _originBounds.Size;

        public float   RaySpacingHorizontalSide { get; private set; }
        public float   RaySpacingVerticalSide   { get; private set; }
        public int     NumRaysPerHorizontalSide { get; private set; }
        public int     NumRaysPerVerticalSide   { get; private set; }
        public int     TotalNumRays             { get; private set; }

        public override string ToString()
        {
            return $"{GetType().Name}:" +
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
        }

        public ReadOnlySpan<CastResult> AllResults    => _results.AsSpan(0,                 TotalNumRays);
        public ReadOnlySpan<CastResult> BottomResults => _results.AsSpan(_bottomStartIndex, NumRaysPerHorizontalSide);
        public ReadOnlySpan<CastResult> TopResults    => _results.AsSpan(_topStartIndex,    NumRaysPerHorizontalSide);
        public ReadOnlySpan<CastResult> LeftResults   => _results.AsSpan(_leftStartIndex,   NumRaysPerVerticalSide);
        public ReadOnlySpan<CastResult> RightResults  => _results.AsSpan(_rightStartIndex,  NumRaysPerVerticalSide);

        public BoxPerimeterRayCaster(BoxCollider2D box, RayCasterSettings settings)
        {
            this._box          = box;
            this.Settings      = settings;
            this._lineCaster   = new LineCaster(settings);
            this._originBounds = new OrientedBounds();
            this._results      = Array.Empty<CastResult>();

            UpdateOrientedBounds(box.bounds, box.transform, settings.Offset);
            ComputeRaySpacingAndCounts(settings.DistanceBetweenRays, _originBounds.Size);
            Debug.Log(this);
        }

        public void CastAll()
        {
            if (!_box)
            {
                return;
            }

            UpdateOrientedBounds(_box.bounds, _box.transform, Settings.Offset);
            ComputeRaySpacingAndCounts(Settings.DistanceBetweenRays, _originBounds.Size);

            Vector2 horizontalStep = RaySpacingHorizontalSide * _originBounds.RightDir;
            for (int i = 0; i < NumRaysPerHorizontalSide; i++)
            {
                Vector2 offsetFromLeftSide = (i * horizontalStep);
                _results[_bottomStartIndex + i] = Cast(_originBounds.LeftBottom + offsetFromLeftSide, _originBounds.DownDir);
                _results[_topStartIndex    + i] = Cast(_originBounds.LeftTop    + offsetFromLeftSide, _originBounds.UpDir);
            }

            Vector2 verticalStep = RaySpacingVerticalSide * _originBounds.UpDir;
            for (int i = 0; i < NumRaysPerVerticalSide; i++)
            {
                Vector2 offsetFromBottomSide = (i * verticalStep);
                _results[_leftStartIndex  + i] = Cast(_originBounds.LeftBottom  + offsetFromBottomSide, _originBounds.LeftDir);
                _results[_rightStartIndex + i] = Cast(_originBounds.RightBottom + offsetFromBottomSide, _originBounds.RightDir);
            }
        }

        private CastResult Cast(Vector2 origin, Vector2 direction)
        {
            return _lineCaster.CastFromPoint(origin, direction);
        }

        private void UpdateOrientedBounds(Bounds bounds, Transform transform, float boundsOffset)
        {
            Bounds expandedBounds = bounds;
            expandedBounds.Expand(boundsOffset);
            Vector2 min = MathExtensions.RotatePointAroundPivot(bounds.min, bounds.center, transform.localEulerAngles.z);
            Vector2 max = MathExtensions.RotatePointAroundPivot(bounds.max, bounds.center, transform.localEulerAngles.z);
            _originBounds.Update(min, max);
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
