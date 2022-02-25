using System;
using UnityEngine;
using PenguinQuest.Data;
using PenguinQuest.Utils;


namespace PenguinQuest.Controllers.AlwaysOnComponents
{

    public class BoxPerimeterRayCaster
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
      
        public BoxPerimeterRayCaster(BoxCollider2D box, RayCasterSettings settings)
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
            for (int i = 0; i < NumRaysPerVerticalSide; i++)
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


            RaySpacingHorizontalSide = size.x / (NumRaysPerHorizontalSide - 1);
            RaySpacingVerticalSide   = size.y / (NumRaysPerVerticalSide   - 1);

            // todo: look into if it's possible to just use spans set with start/end directly
            bottomStartIndex = 0;
            topStartIndex    = bottomStartIndex + NumRaysPerHorizontalSide;
            leftStartIndex   = topStartIndex    + NumRaysPerHorizontalSide;
            rightStartIndex  = leftStartIndex   + NumRaysPerVerticalSide;

            if (results.Length != TotalNumRays)
            {
                results = new CastResult[TotalNumRays];
            }
        }
    }
}
