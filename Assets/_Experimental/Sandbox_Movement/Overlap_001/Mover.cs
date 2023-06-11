using System;
using System.Diagnostics.Contracts;
using UnityEngine;


namespace PQ._Experimental.Overlap_001
{
    public sealed class Mover
    {
        private Body _body;
        
        [Pure]
        private (float distance, Vector2 direction) DecomposeDelta(Vector2 delta)
        {
            // below is exactly equivalent to (delta.normalized, delta.magnitude) without the redundant
            // sqrt call (benchmarks showed ~54% faster), and without NaNs on zero length vectors by just dividing distance
            float squaredMagnitude = delta.sqrMagnitude;
            if (squaredMagnitude <= 1E-010f)
            {
                return (0f, Vector2.zero);
            }

            float magnitude = Mathf.Sqrt(squaredMagnitude);
            delta /= magnitude;
            return (magnitude, delta);
        }


        public Mover(Transform transform)
        {
            _body = transform.GetComponent<Body>();
        }

        public void MoveTo(Vector2 target)
        {
            RaycastHit2D obstruction = default;
            (float step, Vector2 direction) = DecomposeDelta(target - _body.Position);
            if (_body.CastAABB(direction, step, out ReadOnlySpan<RaycastHit2D> hits, false))
            {
                obstruction = hits[0];
                step = hits[0].distance;
            }
            _body.MoveBy(step * direction);

            // if there was an obstruction, apply any depenetration
            if (obstruction && _body.ComputeDepenetration(obstruction.collider, direction, step, out float separation) && separation < 0)
            {
                _body.MoveBy(separation * direction);
            }
        }
    }
}
