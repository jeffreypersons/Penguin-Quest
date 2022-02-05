using UnityEngine;
using PenguinQuest.Utils;
using System.Collections.Generic;


namespace PenguinQuest.Controllers.AlwaysOnComponents
{
    /*
    Provides a streamlined interface for casting lines from specific points or colliders.
    */
    public class BoxColliderPerimeterCaster
    {
        public Collider2D Box                     { get; set; } = default;
        public float      CastOffset              { get; set; } =  0f;
        public float      MaxDistance             { get; set; } = Mathf.Infinity;
        public int        ChecksPerHorizontalSide { get; set; } =  3;
        public int        ChecksPerVerticalSide   { get; set; } =  3;
        public LayerMask  TargetLayers            { get; set; } = ~0;

        private LineCaster lineCaster;
        
        private List<Vector2> leftSideCastOrigins;
        private List<Vector2> rightSideCastOrigins;
        private List<Vector2> topSideCastOrigins;
        private List<Vector2> bottomSideCastOrigins;

        public List<Result> allResults;
        public List<Result> leftResults;
        public List<Result> rightResults;
        public List<Result> topResults;
        public List<Result> bottomResults;

        public BoxColliderPerimeterCaster(BoxCollider2D box)
        {
            Box = box;
            lineCaster = new LineCaster()
            {
                DistanceOffset = CastOffset,
                TargetLayers   = TargetLayers
            };

            allResults    = new List<Result>();
            leftResults   = new List<Result>();
            rightResults  = new List<Result>();
            topResults    = new List<Result>();
            bottomResults = new List<Result>();
            ComputeLocalOrigins();
        }
        
        public struct Result
        {
            public readonly LineCaster.Line  line;
            public readonly LineCaster.Hit?  hit;
            public Result(LineCaster.Line line, LineCaster.Hit? hit)
            {
                this.line = line;
                this.hit  = hit;
            }
        }

        public void Cast()
        {
            allResults   .Clear();
            leftResults  .Clear();
            rightResults .Clear();
            topResults   .Clear();
            bottomResults.Clear();

            foreach (Vector2 origin in leftSideCastOrigins)
            {
                CastLine(origin, Vector2.left, out Result result);
                leftResults.Add(result);
            }

            foreach (Vector2 origin in rightSideCastOrigins)
            {
                CastLine(origin, Vector2.right, out Result result);
                rightResults.Add(result);
            }

            foreach (Vector2 origin in topSideCastOrigins)
            {
                CastLine(origin, Vector2.up, out Result result);
                topResults.Add(result);
            }
            
            foreach (Vector2 origin in bottomSideCastOrigins)
            {
                CastLine(origin, Vector2.down, out Result result);
                bottomResults.Add(result);
            }

            allResults.AddRange(leftResults);
            allResults.AddRange(rightResults);
            allResults.AddRange(topResults);
            allResults.AddRange(bottomResults);
        }

        private void CastLine(Vector2 localOrigin, Vector2 localDirection, out Result result)
        {
            Vector2 origin    = Box.transform.TransformPoint(localOrigin);
            Vector2 direction = Box.transform.TransformDirection(localDirection);
            bool isHit = lineCaster.CastFromPoint(origin, direction, MaxDistance, out LineCaster.Line line, out LineCaster.Hit hit);

            result = new Result(line, isHit ? hit : null);
        }

        private void ComputeLocalOrigins()
        {
            Vector2 min = Box.bounds.min;
            Vector2 max = Box.bounds.max;
            Vector2 leftBottom  = new Vector2(min.x, min.y);
            Vector2 leftTop     = new Vector2(min.x, max.y);
            Vector2 rightTop    = new Vector2(max.x, max.y);
            Vector2 rightBottom = new Vector2(max.x, min.y);

            leftSideCastOrigins   = ComputeOriginsForSide(leftBottom,  leftTop,     ChecksPerVerticalSide);
            rightSideCastOrigins  = ComputeOriginsForSide(rightBottom, rightTop,    ChecksPerVerticalSide);
            topSideCastOrigins    = ComputeOriginsForSide(leftTop,     rightTop,    ChecksPerHorizontalSide);
            bottomSideCastOrigins = ComputeOriginsForSide(leftBottom,  rightBottom, ChecksPerHorizontalSide);
        }
        
        public static List<Vector2> ComputeOriginsForSide(Vector2 min, Vector2 max, int numOrigins)
        {
            if (numOrigins <= 0)
            {
                return default;
            }
            else if (numOrigins == 1)
            {
                return new List<Vector2> { MathUtils.MidPoint(min, max) };
            }
            else
            {
                return MathUtils.InterpolatePoints(min, max, numOrigins);
            }
        }
    }
}
