using UnityEngine;
using PenguinQuest.Extensions;


namespace PenguinQuest.Controllers.AlwaysOnComponents
{
    /*
    Box aligned with axes extending from center to right and top sides respectively.

    In other words, given axes for x and y.
    
    Note that for simplicity, there is no 'scale' or facing taken into account.
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
            Set(Vector2.zero, Vector2.zero);
        }
        public OrientedBounds(Vector2 min, Vector2 max)
        {
            Set(min, max);
        }
        
        public void Update(Vector2 min, Vector2 max)
        {
            if (!MathExtensions.AreComponentsEqual(min, LeftBottom) ||
                !MathExtensions.AreComponentsEqual(max, RightTop))
            {
                Set(min, max);
            }
        }

        private void Set(Vector2 min, Vector2 max)
        {
            LeftBottom  = new Vector2(min.x, min.y);
            LeftTop     = new Vector2(min.x, max.y);
            RightBottom = new Vector2(max.x, min.y);
            RightTop    = new Vector2(max.x, max.y);

            Vector2 centerPoint       = Vector2.Lerp(LeftBottom,  RightTop, 0.50f);
            Vector2 rightSideMidPoint = Vector2.Lerp(RightBottom, RightTop, 0.50f);
            Vector2 topSideMidPoint   = Vector2.Lerp(LeftTop,     RightTop, 0.50f);
            Vector2 rightAxis         = rightSideMidPoint - centerPoint;
            Vector2 upAxis            = topSideMidPoint   - centerPoint;

            Center      = centerPoint;
            Size        = new Vector2(2.0f * rightAxis.magnitude, 2.0f * upAxis.magnitude);
            UpDir       = upAxis.normalized;
            RightDir    = rightAxis.normalized;
            DownDir     = -1f * UpDir;
            LeftDir     = -1f * RightDir;
            Orientation = MathExtensions.AngleFromYAxis(UpDir);
        }
    }
}
