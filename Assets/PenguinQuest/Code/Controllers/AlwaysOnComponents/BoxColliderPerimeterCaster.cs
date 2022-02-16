using System;
using UnityEngine;
using PenguinQuest.Utils;
using PenguinQuest.Data;


namespace PenguinQuest.Controllers.AlwaysOnComponents
{
    /*
    Provides a streamlined interface for casting lines from specific points or colliders.
    */
    public class BoxColliderPerimeterCaster
    {
        public struct Result
        {
            public readonly LineCaster.Line line;
            public readonly LineCaster.Hit? hit;
            public Result(LineCaster.Line line, LineCaster.Hit? hit)
            {
                this.line = line;
                this.hit  = hit;
            }
        }

        // todo: generalize this and apply the same idea to what we use with cameras and extract it out to the data dir
        // todo: use local coords instead
        private struct BoxInfo
        {
            public readonly Vector2 Center;
            public readonly Vector2 Size;

            public readonly Vector2 LeftBottom;
            public readonly Vector2 LeftTop;
            public readonly Vector2 RightBottom;
            public readonly Vector2 RightTop;

            public readonly Vector2 LeftDir;
            public readonly Vector2 RightDir;
            public readonly Vector2 DownDir;
            public readonly Vector2 UpDir;

            public BoxInfo(Collider2D box, float boundsOffset)
            {
                Bounds bounds = box.bounds;
                bounds.Expand(boundsOffset);
                Vector2 mid = box.transform.position;
                Vector2 min = box.transform.TransformPoint(box.bounds.min);
                Vector2 max = box.transform.TransformPoint(box.bounds.max);

                Center      = mid;
                Size        = max - min;
                LeftBottom  = new Vector2(min.x, min.y);
                LeftTop     = new Vector2(min.x, max.y);
                RightBottom = new Vector2(max.x, min.y);
                RightTop    = new Vector2(max.x, max.y);
                UpDir       = box.transform.up.normalized;
                RightDir    = box.transform.forward.normalized;
                DownDir     = -1f * UpDir;
                LeftDir     = -1f * RightDir;
            }
        }


        private int bottomStartIndex;
        private int topStartIndex;
        private int leftStartIndex;
        private int rightStartIndex;
        private LineCaster lineCaster;
        private Result[] results;

        public Collider2D        Box      { get; set; }
        public RayCasterSettings Settings { get; set; }

        public int NumRaysPerHorizontalSide { get; private set; }
        public int NumRaysPerVerticalSide   { get; private set; }
        public int TotalNumRays             { get; private set; }

        public ReadOnlySpan<Result> AllResults    => results.AsSpan(0,                TotalNumRays);
        public ReadOnlySpan<Result> BottomResults => results.AsSpan(bottomStartIndex, NumRaysPerHorizontalSide);
        public ReadOnlySpan<Result> TopResults    => results.AsSpan(topStartIndex,    NumRaysPerHorizontalSide);
        public ReadOnlySpan<Result> LeftResults   => results.AsSpan(leftStartIndex,   NumRaysPerVerticalSide);
        public ReadOnlySpan<Result> RightResults  => results.AsSpan(rightStartIndex,  NumRaysPerVerticalSide);

        public BoxColliderPerimeterCaster(BoxCollider2D box, RayCasterSettings settings)
        {
            Box = box;
            Settings = settings;
            Init();
        }
        
        // todo: add caching so we don't reallocate every single time!
        private void Init()
        {
            // todo: consider integrating entirely into here without using linecaster objects
            lineCaster = new LineCaster()
            {
                DistanceOffset = Settings.Offset,
                TargetLayers   = Settings.TargetLayers
            };
            
            BoxInfo boxInfo = new BoxInfo(Box, Settings.Offset);
            NumRaysPerHorizontalSide = MathUtils.ComputeDivisions(boxInfo.Size.x, Settings.RaySpacing);
            NumRaysPerVerticalSide   = MathUtils.ComputeDivisions(boxInfo.Size.y, Settings.RaySpacing);
            TotalNumRays = 2 * (NumRaysPerHorizontalSide + NumRaysPerVerticalSide);
            
            results = new Result[TotalNumRays];
            bottomStartIndex = 0;
            topStartIndex    = bottomStartIndex + NumRaysPerVerticalSide;
            leftStartIndex   = topStartIndex    + NumRaysPerHorizontalSide;
            rightStartIndex  = leftStartIndex   + NumRaysPerHorizontalSide;
        }
        
        public void Cast()
        {
            BoxInfo boxInfo = new BoxInfo(Box, Settings.Offset);            
            for (int i = 0; i < NumRaysPerHorizontalSide; i++)
            {
                results[bottomStartIndex + i] = CastLine(boxInfo.Center, boxInfo.DownDir);
                results[topStartIndex    + i] = CastLine(boxInfo.Center, boxInfo.UpDir);
            }
            for (int i = 0; i < NumRaysPerVerticalSide; i++)
            {
                results[leftStartIndex  + i] = CastLine(boxInfo.Center, boxInfo.LeftDir);
                results[rightStartIndex + i] = CastLine(boxInfo.Center, boxInfo.RightDir);
            }
        }

        private Result CastLine(Vector2 origin, Vector2 direction)
        {
            bool isHit = lineCaster.CastFromPoint(origin, direction, Settings.MaxDistance,
                out LineCaster.Line line,
                out LineCaster.Hit  hit);
            return new Result(line, isHit ? hit : null);
        }
    }
}
