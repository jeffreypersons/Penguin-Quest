using UnityEngine;


namespace PQ.TestScenes.Minimal.Physics
{
    // todo: replace Params with a readonly ref to params when ref fields are added to C# 11 and supported by Unity
    /*
    Collide and slide style solver for 2D that works in linear steps.

    Assumes always upright bounding box, with kinematic rigidbody.
    */
    public class LinearPhysicsSolver2D
    {
        private readonly Rigidbody2D     _body;
        private readonly BoxCollider2D   _box;
        private readonly ContactFilter2D _filter;
        private readonly RaycastHit2D[]  _hits;
        private readonly SolverParams    _params;

        public Rigidbody2D  Body   => _body;
        public Bounds       AAB    => _box.bounds;
        public SolverParams Params => _params;

        public bool DrawCastsInEditor              { get; set; } = true;
        public bool DrawMovementResolutionInEditor { get; set; } = true;

        public override string ToString() =>
            $"{GetType()}, " +
                $"Position: {_body.position}" +
            $")";

        
        private void SyncPropertiesFromSettings()
        {
            _filter.SetLayerMask(_params.GroundLayerMask);
        }

        public LinearPhysicsSolver2D(Rigidbody2D body, BoxCollider2D box, SolverParams solverParams)
        {
            _body   = body;
            _box    = box;
            _filter = new ContactFilter2D();
            _hits   = new RaycastHit2D[body.attachedColliderCount];
            _params = solverParams;

            _body.isKinematic = true;
            _body.useFullKinematicContacts = true;
            _body.constraints = RigidbodyConstraints2D.FreezeRotation;
            _filter.SetLayerMask(_params.GroundLayerMask);

            Flip(horizontal: false, vertical: false);
        }


        public void Flip(bool horizontal, bool vertical)
        {
            _body.constraints &= ~RigidbodyConstraints2D.FreezeRotation;
            _body.transform.localEulerAngles = new Vector3(
                x: vertical?   180f : 0f,
                y: horizontal? 180f : 0f,
                z: 0f);
            _body.constraints |= RigidbodyConstraints2D.FreezeRotation;
        }

        public void Move(Vector2 deltaPosition)
        {
            _filter.SetLayerMask(_params.GroundLayerMask);

            Vector2 up         = Vector2.up;
            Vector2 vertical   = Vector2.Dot(deltaPosition, up) * up;
            Vector2 horizontal = deltaPosition - vertical;

            // note that we resolve horizontal first as the movement is simpler than vertical
            MoveHorizontal(horizontal);
            MoveVertical(vertical);
        }



        /* Iteratively move body along surface one linear step at a time until target reached, or iteration cap exceeded. */
        private void MoveHorizontal(Vector2 targetDelta)
        {
            int iteration = 0;
            Vector2 currentDelta = targetDelta;
            while (iteration < _params.MaxIterations && currentDelta != Vector2.zero)
            {
                if (!TryFindClosestCollisionAlongDelta(currentDelta, out float hitDistance, out Vector2 hitNormal))
                {
                    // nothing blocking our path, move straight ahead, and don't worry about energy loss (for now)
                    _body.position += currentDelta;
                    return;
                }

                // unless there's an overly steep slope, move a linear step with properties taken into account
                float slopeAngle = Vector2.Angle(Vector2.up, hitNormal);
                if (slopeAngle <= _params.MaxSlopeAngle)
                {
                    // move a single linear step along our delta until the detected collision
                    currentDelta = hitDistance * currentDelta.normalized;
                    currentDelta = ComputeCollisionDelta(currentDelta, hitNormal);
                }
                else
                {
                    currentDelta = Vector2.zero;
                }
                
                #if UNITY_EDITOR
                if (DrawMovementResolutionInEditor)
                    DrawMovementStepInEditor(_body.position, currentDelta);
                #endif

                // feed our adjusted movement back into Unity's physics
                _body.position += currentDelta;

                iteration++;
            }
        }
        
        /* Iteratively move body along surface one linear step at a time until target reached, or iteration cap exceeded. */
        private void MoveVertical(Vector2 targetDelta)
        {
            int iteration = 0;
            Vector2 currentDelta = targetDelta;
            while (iteration < _params.MaxIterations && currentDelta != Vector2.zero)
            {
                if (!TryFindClosestCollisionAlongDelta(currentDelta, out float hitDistance, out Vector2 hitNormal))
                {
                    // nothing blocking our path, move straight ahead, and don't worry about energy loss (for now)
                    _body.position += currentDelta;
                    return;
                }
                
                // only if there's an overly steep slope, do we want to take action (eg sliding down)
                float slopeAngle = Vector2.Angle(Vector2.up, hitNormal);
                if (slopeAngle > _params.MaxSlopeAngle)
                {
                    // move a single linear step along our delta until the detected collision
                    currentDelta = hitDistance * currentDelta.normalized;
                    currentDelta = ComputeCollisionDelta(currentDelta, hitNormal);
                }
                
                #if UNITY_EDITOR
                if (DrawMovementResolutionInEditor)
                    DrawMovementStepInEditor(_body.position, currentDelta);
                #endif

                // feed our adjusted movement back into Unity's physics
                _body.position += currentDelta;

                iteration++;
            }
        }

        /* Project rigidbody forward, taking skin width and attached colliders into account, and return the closest rigidbody hit. */
        private bool TryFindClosestCollisionAlongDelta(Vector2 delta, out float hitDistance, out Vector2 hitNormal)
        {
            var closestHitNormal   = Vector2.zero;
            var closestHitDistance = delta.magnitude;
            int hitCount = _body.Cast(delta, _filter, _hits, closestHitDistance + _params.ContactOffset);
            for (int i = 0; i < hitCount; i++)
            {
                #if UNITY_EDITOR
                if (DrawCastsInEditor)
                    DrawCastResultAsLineInEditor(_hits[i], _params.ContactOffset, delta, closestHitDistance);
                #endif
                float adjustedDistance = _hits[i].distance - _params.ContactOffset;
                if (adjustedDistance > 0f && adjustedDistance < closestHitDistance)
                {
                    closestHitNormal   = _hits[i].normal;
                    closestHitDistance = adjustedDistance;
                }
            }

            if (closestHitNormal == Vector2.zero)
            {
                hitDistance = default;
                hitNormal   = default;
                return false;
            }
            hitDistance = closestHitDistance;
            hitNormal   = closestHitNormal;
            return true;
        }

        /*
        Apply bounciness/friction coefficients to hit position/normal, in proportion with the desired movement distance.

        In other words, for a given collision what is the adjusted delta when taking impact angle, velocity, bounciness,
        and friction into account (using a linear model similar to Unity's dynamic physics)?
        
        Note that collisions are resolved via: adjustedDelta = moveDistance * [(Sbounciness)Snormal + (1-Sfriction)Stangent]
            * where bounciness is from 0 (no bounciness) to 1 (completely reflected)
            * friction is from -1 ('boosts' velocity) to 0 (no resistance) to 1 (max resistance)
        */
        private Vector2 ComputeCollisionDelta(Vector2 desiredDelta, Vector2 hitNormal)
        {
            float remainingDistance = desiredDelta.magnitude;
            Vector2 reflected  = Vector2.Reflect(desiredDelta, hitNormal);
            Vector2 projection = Vector2.Dot(reflected, hitNormal) * hitNormal;
            Vector2 tangent    = reflected - projection;

            Vector2 perpendicularContribution = (_params.Bounciness * remainingDistance) * projection.normalized;
            Vector2 tangentialContribution    = ((1f - _params.Friction) * remainingDistance) * tangent.normalized;
            return perpendicularContribution + tangentialContribution;
        }        
        

        #if UNITY_EDITOR
        private static void DrawMovementStepInEditor(Vector2 position, Vector2 delta)
        {
            Debug.DrawLine(position, position + delta, Color.blue, Time.fixedDeltaTime);
        }

        private static void DrawCastResultAsLineInEditor(RaycastHit2D hit, float offset, Vector2 direction, float distance)
        {
            if (!hit)
            {
                // unfortunately we can't reliably find the origin of the cast
                // if there was no hit (as far as I'm aware), so nothing to draw
                return;
            }

            float duration = Time.fixedDeltaTime;
            var origin = hit.point - (distance * direction);
            var start  = origin    + (offset   * direction);
            var end    = hit.point;
            Debug.DrawLine(start,  end,    Color.red, duration);
            Debug.DrawLine(start,  origin, Color.magenta, duration);
            Debug.DrawLine(origin, end,    Color.green, duration);
        }
        #endif
    }
}
