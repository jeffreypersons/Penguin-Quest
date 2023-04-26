using System;
using System.Diagnostics.Contracts;
using UnityEngine;


namespace PQ.TestScenes.Box
{
    [Flags]
    public enum CollisionFlags2D
    {
        None   = 0,
        Front  = 1 << 1,
        Below  = 1 << 2,
        Behind = 1 << 3,
        Above  = 1 << 4,
        All    = ~0,
    }
    public sealed class Mover
    {
        private Body _body;
        private float _maxAngle;
        private int _maxIterations;
        private CollisionFlags2D _collisions;


        [Pure]
        private bool ApproximatelyZero(Vector2 delta)
        {
            // note that the epsilon used for equality checks handles small values far better than
            // checking square magnitude with mathf/k epsilons
            return delta == Vector2.zero;
        }

        public Mover(Transform transform)
        {
            _body = transform.GetComponent<Body>();
            _collisions = CollisionFlags2D.None;
            _body.Flip(horizontal: false, vertical: false);
        }

        public void SetParams(float maxSlopeAngle, int maxSolverIterations)
        {
            _maxAngle = maxSlopeAngle;
            _maxIterations = maxSolverIterations;
        }

        public void Flip(bool horizontal)
        {
            _body.Flip(horizontal, false);
        }

        /* Note - collision responses are accounted for, but any other externalities such as gravity must be passed in. */
        public void Move(Vector2 deltaPosition)
        {
            // todo: add some special-cased sort of move initial/and or depenetration/overlap resolution (and at end)
            _collisions = CollisionFlags2D.None;

            // scale deltas in proportion to the y-axis
            Vector2 up         = _body.Up;
            Vector2 vertical   = Vector2.Dot(deltaPosition, up) * up;
            Vector2 horizontal = deltaPosition - vertical;

            // note that we resolve horizontal first as the movement is simpler than vertical
            MoveHorizontal(horizontal);
            MoveVertical(vertical);
        }

        public bool InContact(CollisionFlags2D flags)
        {
            return (_collisions & flags) == flags;
        }


        private void MoveHorizontal(Vector2 initialDelta)
        {
            Vector2 delta = initialDelta;
            for (int i = 0; i < _maxIterations && !ApproximatelyZero(delta); i++)
            {
                // move directly to target if unobstructed
                if (!_body.CastClosest(delta, out RaycastHit2D hit))
                {
                    _body.MoveBy(delta);
                    delta = Vector2.zero;
                    continue;
                }

                delta = ApplyCollisionResponse(delta, hit.normal);
            }
        }

        private void MoveVertical(Vector2 initialDelta)
        {
            Vector2 delta = initialDelta;
            for (int i = 0; i < _maxIterations && !ApproximatelyZero(delta); i++)
            {
                // move directly to target if unobstructed
                if (!_body.CastClosest(delta, out RaycastHit2D hit))
                {
                    _body.MoveBy(delta);
                    delta = Vector2.zero;
                    continue;
                }

                delta = ApplyCollisionResponse(delta, hit.normal);
            }
        }
        
        private Vector2 ApplyCollisionResponse(Vector2 delta, Vector2 normal)
        {
            // todo: replace with actual collision response computations
            return Vector3.ProjectOnPlane(delta, normal);
        }
    }
}
