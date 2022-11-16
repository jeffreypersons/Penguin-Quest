using UnityEngine;


namespace PQ.TestScenes.Minimal.Physics
{
    // todo: replace with struct and store as `ref readonly struct` when we finally get C#11
    public record SolverParams
    {
        public int       MaxIterations { get; set; }

        public float     Bounciness    { get; set; }
        public float     Friction      { get; set; }

        public float     ContactOffset { get; set; }
        public LayerMask LayerMask     { get; set; }
        public float     MaxSlopeAngle { get; set; }
        public float     Gravity       { get; set; }
    }
}