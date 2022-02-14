using System;
using UnityEngine;
using PenguinQuest.Utils;


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
                this.hit = hit;
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

        // todo: clean this stuff up!
        public Collider2D Box          { get; set; } = default;
        public float      CastOffset   { get; set; } = 0f;
        public float      MaxDistance  { get; set; } = Mathf.Infinity;
        public float      RaySpacing   { get; set; } = 0.25f;
        public LayerMask  TargetLayers { get; set; } = ~0;

        public int NumRaysPerHorizontalSide { get; private set; }
        public int NumRaysPerVerticalSide   { get; private set; }
        public int TotalNumRays { get; private set; }

        private LineCaster lineCaster;
        
        // todo: look into a single array with span for left/right/top/bottom
        public Result[] allResults;
        public Result[] leftResults;
        public Result[] rightResults;
        public Result[] topResults;
        public Result[] bottomResults;

        public BoxColliderPerimeterCaster(BoxCollider2D box)
        {
            Box = box;
            Update();
        }

        // todo: add caching so we don't reallocate every single time!
        private void Update()
        {
            lineCaster = new LineCaster()
            {
                DistanceOffset = CastOffset,
                TargetLayers   = TargetLayers
            };

            BoxInfo boxInfo = new BoxInfo(Box, CastOffset);
            NumRaysPerHorizontalSide = MathUtils.ComputeDivisions(boxInfo.Size.x, RaySpacing);
            NumRaysPerVerticalSide   = MathUtils.ComputeDivisions(boxInfo.Size.y, RaySpacing);
            TotalNumRays = 2 * (NumRaysPerHorizontalSide + NumRaysPerVerticalSide);

            allResults    = new Result[TotalNumRays];
            leftResults   = new Result[NumRaysPerVerticalSide];
            rightResults  = new Result[NumRaysPerVerticalSide];
            topResults    = new Result[NumRaysPerHorizontalSide];
            bottomResults = new Result[NumRaysPerHorizontalSide];
        }

        public void Cast()
        {
            Update();
            BoxInfo boxInfo = new BoxInfo(Box, CastOffset);

            int rayIndex = 0;
            
            for (int i = 0; i < NumRaysPerHorizontalSide; i++)
            {
                bottomResults[i]       = CastLine(boxInfo.Center, boxInfo.DownDir);
                topResults[i]          = CastLine(boxInfo.Center, boxInfo.UpDir);
                allResults[rayIndex++] = bottomResults[i];
                allResults[rayIndex++] = topResults[i];
            }
            for (int i = 0; i < NumRaysPerVerticalSide; i++)
            {
                leftResults[i]         = CastLine(boxInfo.Center, boxInfo.LeftDir);
                rightResults[i]        = CastLine(boxInfo.Center, boxInfo.RightDir);
                allResults[rayIndex++] = leftResults[i];
                allResults[rayIndex++] = rightResults[i];
            }
        }

        private Result CastLine(Vector2 origin, Vector2 direction)
        {
            bool isHit = lineCaster.CastFromPoint(origin, direction, MaxDistance,
                out LineCaster.Line line,
                out LineCaster.Hit hit);
            return new Result(line, isHit ? hit : null);
        }
    }
}
