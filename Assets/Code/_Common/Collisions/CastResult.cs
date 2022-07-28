using UnityEngine;


namespace PQ.Common.Collisions
{
    public struct CastResult
    {
        public readonly Vector2 origin;
        public readonly Vector2 terminal;

        public readonly CastHit? hit;
        public CastResult(Vector2 origin, Vector2 terminal, CastHit? hit)
        {
            this.origin = origin;
            this.terminal = terminal;
            this.hit = hit;
        }
    }
}
