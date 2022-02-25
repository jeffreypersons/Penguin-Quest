using UnityEngine;
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
            Vector2 min          = center - halfDiagonal;
            Vector2 max          = center + halfDiagonal;

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
}
