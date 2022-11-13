using System;
using System.Diagnostics.Contracts;
using UnityEngine;


namespace PQ.TestScenes.Minimal.Physics
{
    /*
    Collide and slide style solver for 2D that works in linear steps.

    Assumes always upright bounding box, with kinematic rigidbody.
    Basically, this class is all about projecting rigidbody along desired delta,
    taking skin width, surface collisions, and attached colliders into account.
    */
    public sealed class CollideAndSlideSolver2D
    {
        private CollisionFlags2D _collisions;
        private readonly KinematicBody2D _body;
        private readonly SolverParams    _params;

        public SolverParams     Params => _params;
        public CollisionFlags2D Flags  => _collisions;

        public override string ToString() =>
            $"{GetType()}, " +
                $"Flags: {_collisions}," +
                $"Params: {_params}," +
                $"Body: {_body}," +
            $")";

        [Pure]
        private bool HasReachedTarget(Vector2 delta)
        {
            // note that the epsilon used for equality checks handles small values far better than
            // checking square magnitude with mathf/k epsilons
            return delta == Vector2.zero;
        }

        public CollideAndSlideSolver2D(KinematicBody2D body, in SolverParams solverParams)
        {
            _body       = body;
            _params     = solverParams;
            _collisions = CollisionFlags2D.None;
            _body.SetSkinWidth(_params.ContactOffset);
        }

        public void Move(Vector2 deltaPosition)
        {
            _body.SetSkinWidth(_params.ContactOffset);

            // todo: add some special-cased sort of move initial/and or depenetration/overlap resolution (and at end)
            _collisions = CollisionFlags2D.None;

            // scale deltas in proportion to the y-axis
            Vector2 up         = _body.Up;
            Vector2 vertical   = Vector2.Dot(deltaPosition, up) * up;
            Vector2 horizontal = deltaPosition - vertical;

            // note that we resolve horizontal first as the movement is simpler than vertical
            MoveHorizontal(horizontal);
            MoveVertical(vertical);

            // now that we have solved for both movement independently, get our flags up to date
            _collisions = _body.CheckForOverlappingContacts(_params.LayerMask, _params.MaxSlopeAngle);
        }



        /* Iteratively move body along surface one linear step at a time until target reached, or iteration cap exceeded. */
        private void MoveHorizontal(Vector2 targetDelta)
        {
            int iteration = 0;
            Vector2 currentDelta = targetDelta;
            while (iteration < _params.MaxIterations && !HasReachedTarget(currentDelta))
            {
                if (!_body.CastAAB(currentDelta, _params.LayerMask, out ReadOnlySpan<RaycastHit2D> hits))
                {
                    // nothing blocking our path, move straight ahead, and don't worry about energy loss (for now)
                    _body.MoveBy(currentDelta);
                    break;
                }

                // move a single linear step along our delta until the detected collision
                RaycastHit2D hit = FindClosest(hits);
                if (hit.distance < _params.ContactOffset)
                {
                    currentDelta = hit.distance <= _params.ContactOffset?
                        Vector2.zero : hit.distance * currentDelta.normalized;
                }

                // unless there's an overly steep slope, move a linear step with properties taken into account
                float slopeAngle = Vector2.Angle(_body.Up, hit.normal);
                if (slopeAngle <= _params.MaxSlopeAngle)
                {
                    currentDelta += ComputeCollisionDelta(currentDelta, hit.normal, _params.Bounciness, _params.Friction);
                }
                else
                {
                    currentDelta = Vector2.zero;
                }

                _body.MoveBy(currentDelta);
                iteration++;
            }
        }

        /* Iteratively move body along surface one linear step at a time until target reached, or iteration cap exceeded. */
        private void MoveVertical(Vector2 targetDelta)
        {
            int iteration = 0;
            Vector2 currentDelta = targetDelta;
            while (iteration < _params.MaxIterations && !HasReachedTarget(currentDelta))
            {
                if (!_body.CastAAB(currentDelta, _params.LayerMask, out ReadOnlySpan<RaycastHit2D> hits))
                {
                    // nothing blocking our path, move straight ahead, and don't worry about energy loss (for now)
                    _body.MoveBy(currentDelta);
                    break;
                }

                // move a single linear step along our delta until the detected collision
                RaycastHit2D hit = FindClosest(hits);
                if (hit.distance < _params.ContactOffset)
                {
                    currentDelta = hit.distance <= _params.ContactOffset ?
                        Vector2.zero : hit.distance * currentDelta.normalized;
                }

                // only if there's an overly steep slope, do we want to take action (eg sliding down)
                float slopeAngle = Vector2.Angle(Vector2.up, hit.normal);
                if (slopeAngle > _params.MaxSlopeAngle)
                {
                    currentDelta += ComputeCollisionDelta(currentDelta, hit.normal, _params.Bounciness, _params.Friction);
                }

                _body.MoveBy(currentDelta);
                iteration++;
            }
        }

        /*
        Assuming successful ray casts, what's our closest hit?
        */
        private static RaycastHit2D FindClosest(ReadOnlySpan<RaycastHit2D> hits)
        {
            int closestHitIndex = 0;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].distance < hits[closestHitIndex].distance)
                {
                    closestHitIndex = i;
                }
            }
            return hits[closestHitIndex];
        }

        /*
        Apply bounciness/friction coefficients to hit position/normal, in proportion with the desired movement distance.

        In other words, for a given collision what is the adjusted delta when taking impact angle, velocity, bounciness,
        and friction into account (using a linear model similar to Unity's dynamic physics)?
        
        Note that collisions are resolved via: adjustedDelta = moveDistance * [(Sbounciness)Snormal + (1-Sfriction)Stangent]
            * where bounciness is from 0 (no bounciness) to 1 (completely reflected)
            * friction is from -1 ('boosts' velocity) to 0 (no resistance) to 1 (max resistance)
        */
        private static Vector2 ComputeCollisionDelta(Vector2 desiredDelta, Vector2 hitNormal, float bounciness, float friction)
        {
            float remainingDistance = desiredDelta.magnitude;
            Vector2 reflected  = Vector2.Reflect(desiredDelta, hitNormal);
            Vector2 projection = Vector2.Dot(reflected, hitNormal) * hitNormal;
            Vector2 tangent    = reflected - projection;

            Vector2 perpendicularContribution = (bounciness      * remainingDistance) * projection.normalized;
            Vector2 tangentialContribution    = ((1f - friction) * remainingDistance) * tangent.normalized;
            return perpendicularContribution + tangentialContribution;
        }
    }
}
