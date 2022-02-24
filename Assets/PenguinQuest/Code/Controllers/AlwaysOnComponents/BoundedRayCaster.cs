using System;
using UnityEngine;
using PenguinQuest.Data;
using PenguinQuest.Utils;


namespace PenguinQuest.Controllers.AlwaysOnComponents
{
    /*
    Box aligned with axes extending from center to right and top sides respectively.

    In other words, given axes for x and y.
    */
    public class OrientedBounds
    {
        public Vector2 Center      { get; private set; }
        public Vector2 Size        { get; private set; }
        public float   Orientation { get; private set; }

        public Vector2 LeftBottom  { get; private set; }
        public Vector2 LeftTop     { get; private set; }
        public Vector2 RightBottom { get; private set; }
        public Vector2 RightTop    { get; private set; }
            
        public Vector2 LeftDir     { get; private set; }
        public Vector2 RightDir    { get; private set; }
        public Vector2 DownDir     { get; private set; }
        public Vector2 UpDir       { get; private set; }

        public override string ToString()
        {
            return $"Center:{Center},Size:{Size},Rotation:{Orientation}";
        }

        public OrientedBounds()
        {
            Set(Vector2.zero, Vector2.zero, Vector2.right, Vector2.up);
        }
        public OrientedBounds(Vector2 center, Vector2 size, Vector2 right, Vector2 up)
        {
            Set(center, size, right, up);
        }
        
        public void Update(Vector2 center, Vector2 size, Vector2 right, Vector2 up)
        {
            bool hasMoved   = !MathUtils.AreComponentsEqual(center, Center);
            bool hasResized = !MathUtils.AreComponentsEqual(size,   Size);
            bool hasRotated = !MathUtils.AreDirectionsEqual_Fast(right, RightDir) ||
                              !MathUtils.AreDirectionsEqual_Fast(up,    UpDir);

            if (hasMoved && !hasResized && !hasRotated)
            {
                MoveTo(center);
            }
            else if (hasMoved || hasResized || hasRotated)
            {
                Set(center, size, right, up);
            }
        }

        private void Set(Vector2 center, Vector2 size, Vector2 right, Vector2 up)
        {
            Vector2 upAxis       = up.normalized;
            Vector2 rightAxis    = right.normalized;
            Vector2 halfDiagonal = (0.50f * size.x * rightAxis) + (0.50f * size.y * upAxis);
            Vector2 min          = center + halfDiagonal;
            Vector2 max          = center - halfDiagonal;

            Center      = center;
            Size        = size;
            LeftBottom  = new Vector2(min.x, min.y);
            LeftTop     = new Vector2(min.x, max.y);
            RightBottom = new Vector2(max.x, min.y);
            RightTop    = new Vector2(max.x, max.y);
            UpDir       = upAxis;
            RightDir    = rightAxis;
            DownDir     = -1f * upAxis;
            LeftDir     = -1f * rightAxis;
            Orientation = MathUtils.AngleFromYAxis(UpDir);
        }

        private void MoveTo(Vector2 center)
        {
            Vector2 displacement = center - Center;
            Center      += displacement;
            LeftBottom  += displacement;
            LeftTop     += displacement;
            RightBottom += displacement;
            RightTop    += displacement;
        }
    }
    
    /*
    Provides a streamlined interface for casting lines from along each side of an AAB.
    */
    public class BoundedRayCaster
    {
        private int bottomStartIndex;
        private int topStartIndex;
        private int leftStartIndex;
        private int rightStartIndex;
        private CastResult[] results;

        private BoxCollider2D  box;
        private OrientedBounds originBounds;
        private LineCaster     lineCaster;

        public RayCasterSettings Settings { get; set; }
        public Vector2 CenterOfBounds => originBounds.Center;
        public Vector2 SizeOfBounds   => originBounds.Size;

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
                    $"{originBounds}}}";
        }

        public ReadOnlySpan<CastResult> AllResults    => results.AsSpan(0,                TotalNumRays);
        public ReadOnlySpan<CastResult> BottomResults => results.AsSpan(bottomStartIndex, NumRaysPerHorizontalSide);
        public ReadOnlySpan<CastResult> TopResults    => results.AsSpan(topStartIndex,    NumRaysPerHorizontalSide);
        public ReadOnlySpan<CastResult> LeftResults   => results.AsSpan(leftStartIndex,   NumRaysPerVerticalSide);
        public ReadOnlySpan<CastResult> RightResults  => results.AsSpan(rightStartIndex,  NumRaysPerVerticalSide);
      
        public BoundedRayCaster(BoxCollider2D box, RayCasterSettings settings)
        {
            this.box          = box;
            this.Settings     = settings;
            this.lineCaster   = new LineCaster(settings);
            this.originBounds = new OrientedBounds();
            this.results      = Array.Empty<CastResult>();
            UpdateOrientedBounds(box.bounds, box.transform, settings.Offset);
            ComputeRaySpacingAndCounts(settings.DistanceBetweenRays, originBounds.Size);
            Debug.Log(this);
        }

        /* Cast outwards from each side of the bounding box. */
        public void CastAll()
        {
            UpdateOrientedBounds(box.bounds, box.transform, Settings.Offset);
            ComputeRaySpacingAndCounts(Settings.DistanceBetweenRays, originBounds.Size);

            Vector2 horizontalStep = RaySpacingHorizontalSide * originBounds.RightDir;
            for (int i = 0; i < NumRaysPerHorizontalSide; i++)
            {
                Vector2 offsetFromLeftSide = (i * horizontalStep);
                results[bottomStartIndex + i] = Cast(originBounds.LeftBottom + offsetFromLeftSide, originBounds.DownDir);
                results[topStartIndex    + i] = Cast(originBounds.LeftTop    + offsetFromLeftSide, originBounds.UpDir);
            }

            Vector2 verticalStep = RaySpacingVerticalSide * originBounds.UpDir;
            for (int i = 0; i < NumRaysPerHorizontalSide; i++)
            {
                Vector2 offsetFromBottomSide = (i * verticalStep);
                results[leftStartIndex  + i] = Cast(originBounds.LeftBottom  + offsetFromBottomSide, originBounds.LeftDir);
                results[rightStartIndex + i] = Cast(originBounds.RightBottom + offsetFromBottomSide, originBounds.RightDir);
            }
        }

        private CastResult Cast(Vector2 origin, Vector2 direction)
        {
            return lineCaster.CastFromPoint(origin, direction, Settings.MaxDistance);
        }

        private void UpdateOrientedBounds(Bounds bounds, Transform transform, float boundsOffset)
        {
            Bounds expandedBounds = bounds;
            expandedBounds.Expand(boundsOffset);
            originBounds.Update(expandedBounds.center, expandedBounds.size, transform.right, transform.up);
        }

        private void ComputeRaySpacingAndCounts(float distanceBetweenRays, Vector2 size)
        {
            int numRaysPerHorizontalSide = Mathf.RoundToInt(size.x / distanceBetweenRays);
            int numRaysPerVerticalSide   = Mathf.RoundToInt(size.y / distanceBetweenRays);
            if (NumRaysPerHorizontalSide != numRaysPerHorizontalSide ||
                NumRaysPerVerticalSide   != numRaysPerVerticalSide)
            {
                NumRaysPerHorizontalSide = numRaysPerHorizontalSide;
                NumRaysPerVerticalSide   = numRaysPerVerticalSide;
                TotalNumRays = 2 * (NumRaysPerHorizontalSide + NumRaysPerVerticalSide);
            }


            RaySpacingHorizontalSide = size.y / (NumRaysPerHorizontalSide - 1);
            RaySpacingVerticalSide   = size.x / (NumRaysPerVerticalSide   - 1);

            bottomStartIndex = 0;
            topStartIndex    = bottomStartIndex + NumRaysPerVerticalSide;
            leftStartIndex   = topStartIndex    + NumRaysPerHorizontalSide;
            rightStartIndex  = leftStartIndex   + NumRaysPerHorizontalSide;

            if (results.Length != TotalNumRays)
            {
                results = new CastResult[TotalNumRays];
            }
        }
    }
}
