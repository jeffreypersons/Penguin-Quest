using UnityEngine;


// since Unity doesn't support .net 5 yet, we enable init field properties via the following:
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

namespace PQ.TestScenes.Minimal.Physics
{
    public record SolverParams
    {
        public int       MaxIterations   { get; init; }

        public float     Bounciness      { get; init; }
        public float     Friction        { get; init; }

        public float     ContactOffset   { get; init; }
        public LayerMask GroundLayerMask { get; init; }
        public float     MaxSlopeAngle   { get; init; }
        public float     Gravity         { get; init; }
    }
}
