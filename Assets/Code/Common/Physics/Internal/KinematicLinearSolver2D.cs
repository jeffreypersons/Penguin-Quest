using System;
using UnityEngine;


namespace PQ.Common.Physics.Internal
{
    internal sealed class KinematicLinearSolver2D
    {
        // todo: replace with struct and store as `ref readonly struct` when we finally get C#11
        public record Params
        {
            public int   MaxMoveIterations    { get; set; }
            public int   MaxOverlapIterations { get; set; }
            public float MaxSlopeAngle        { get; set; }
            public bool  VisualizePath        { get; set; }
        }

        private Params _params;
        private KinematicRigidbody2D _body;
        private CollisionFlags2D _collisions;


        public KinematicLinearSolver2D(KinematicRigidbody2D body, in Params solverParams)
        {
            if (body == null)
            {
                throw new ArgumentNullException($"Expected non-null {nameof(KinematicRigidbody2D)}");
            }

            _body       = body;
            _collisions = CollisionFlags2D.None;
            _params     = solverParams;
        }

        public void SolveMovement(Vector2 deltaPosition)
        {
            if (deltaPosition == Vector2.zero)
            {
                _collisions = _body.CheckForOverlappingContacts(_body.SkinWidth);
                return;
            }

            // scale deltas in proportion to the y-axis
            Vector2 up         = _body.Up;
            Vector2 vertical   = Vector2.Dot(deltaPosition, up) * up;
            Vector2 horizontal = deltaPosition - vertical;
            Vector2 position   = _body.Position;

            // note that we resolve horizontal first as the movement is simpler than vertical
            MoveHorizontal(horizontal);
            MoveVertical(vertical);

            _collisions = _body.CheckForOverlappingContacts(_body.SkinWidth);
            _body.MovePosition(startPositionThisFrame: position, targetPositionThisFrame: _body.Position);
        }

        public bool InContact(CollisionFlags2D flags)
        {
            return (_collisions & flags) == flags;
        }


        private void MoveHorizontal(Vector2 initialDelta)
        {
            Vector2 delta = initialDelta;
            for (int i = 0; i < _params.MaxMoveIterations && delta != Vector2.zero; i++)
            {
                MoveAABBAlongDelta(ref delta, out RaycastHit2D hit);
            }
        }

        private void MoveVertical(Vector2 initialDelta)
        {
            Vector2 delta = initialDelta;
            for (int i = 0; i < _params.MaxMoveIterations && delta != Vector2.zero; i++)
            {
                MoveAABBAlongDelta(ref delta, out RaycastHit2D hit);
            }
        }
        
        /* Project AABB along delta until (if any) obstruction. Max distance caps at body-radius to prevent tunneling. */
        private void MoveAABBAlongDelta(ref Vector2 delta, out RaycastHit2D hit)
        {
            if (delta == Vector2.zero)
            {
                hit = default;
                return;
            }

            float remainingDistance = delta.magnitude;
            Vector2 direction = delta / remainingDistance;
            Vector2 step = Mathf.Min(_body.ComputeDistanceToEdge(direction), remainingDistance) * direction;
            if (_body.CastAABB_Closest(step, out hit))
            {
                step = hit.distance * direction;
            }

            _body.MoveBy(step);
            delta -= step;
        }

        /*
        Apply bounciness/friction coefficients to hit position/normal, in proportion with the desired movement distance.

        In other words, for a given collision what is the adjusted delta when taking impact angle, velocity, bounciness,
        and friction into account (using a linear model similar to Unity's dynamic physics)?
        
        Note that collisions are resolved via: adjustedDelta = moveDistance * [(Sbounciness)Snormal + (1-Sfriction)Stangent]
        * where bounciness is from 0 (no bounciness) to 1 (completely reflected)
        * friction is from -1 ('boosts' velocity) to 0 (no resistance) to 1 (max resistance)
        */
        private Vector2 ComputeCollisionDelta(Vector2 delta, Vector2 hitNormal, float bounciness=0f, float friction=0f)
        {
            float remainingDistance = delta.magnitude;
            Vector2 reflected  = Vector2.Reflect(delta, hitNormal);
            Vector2 projection = Vector2.Dot(reflected, hitNormal) * hitNormal;
            Vector2 tangent    = reflected - projection;

            Vector2 perpendicularContribution = (bounciness      * remainingDistance) * projection.normalized;
            Vector2 tangentialContribution    = ((1f - friction) * remainingDistance) * tangent.normalized;
            return perpendicularContribution + tangentialContribution;
        }
    }
}
