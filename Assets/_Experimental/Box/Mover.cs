using System;
using System.Diagnostics.Contracts;
using Unity.VisualScripting;
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
        private const int PreallocatedHitBufferSize = 16;

        private CollisionFlags2D _collisions;
        private float            _maxAngle;
        private float            _skinWidth;
        private int              _maxIterations;
        private Rigidbody2D      _body;
        private BoxCollider2D    _aabb;
        private ContactFilter2D  _castFilter;
        private RaycastHit2D[]   _castHits;

        public override string ToString() =>
            $"Mover{{" +
                $"Position:{Position}," +
                $"Depth:{Depth}," +
                $"Forward:{Forward}," +
                $"Up:{Up}," +
                $"SkinWidth:{SkinWidth}," +
                $"AAB: bounds(center:{Bounds.center}, extents:{Bounds.extents})," +
            $"}}";

        public Vector2 Position  => _body.position;
        public float   Depth     => _body.transform.position.z;
        public Bounds  Bounds    => _aabb.bounds;
        public float   SkinWidth => _skinWidth;
        public Vector2 Forward   => _body.transform.right.normalized;
        public Vector2 Up        => _body.transform.up.normalized;

        public bool InContact(CollisionFlags2D flags) => (_collisions & flags) == flags;

        [Pure]
        private bool ApproximatelyZero(Vector2 delta)
        {
            // note that the epsilon used for equality checks handles small values far better than
            // checking square magnitude with mathf/k epsilons
            return delta == Vector2.zero;
        }

        public Mover(Transform transform)
        {
            if (!transform.TryGetComponent<Rigidbody2D>(out var rigidBody))
            {
                throw new MissingComponentException($"Expected attached rigidbody2D - not found on {transform}");
            }
            if (!transform.TryGetComponent<BoxCollider2D>(out var boxCollider))
            {
                throw new MissingComponentException($"Expected attached collider2D - not found on {transform}");
            }

            _collisions = CollisionFlags2D.None;
            _skinWidth  = 0f;
            _body       = rigidBody;
            _aabb       = boxCollider;
            _castFilter = new ContactFilter2D();
            _castHits   = new RaycastHit2D[PreallocatedHitBufferSize];
            _castFilter.useLayerMask = true;

            _body.isKinematic = true;
            _body.simulated   = true;
            _body.useFullKinematicContacts = true;
            _body.constraints = RigidbodyConstraints2D.FreezeRotation;

            Flip(horizontal: false, vertical: false);
        }

        public void SetMaxAngle(float angle) => _maxAngle = angle;
        public void SetSkinWidth(float amount) => _skinWidth = amount;
        public void SetLayerMask(LayerMask mask) => _castFilter.SetLayerMask(mask);
        public void SetMaxSolverIterations(int iterations) => _maxIterations = iterations;

        public void Flip(bool horizontal, bool vertical)
        {
            _body.constraints &= ~RigidbodyConstraints2D.FreezeRotation;
            _body.transform.localEulerAngles = new Vector3(
                x: vertical   ? 180f : 0f,
                y: horizontal ? 180f : 0f,
                z: 0f);
            _body.constraints |= RigidbodyConstraints2D.FreezeRotation;
        }

        /* Note - collision responses are accounted for, but any other externalities such as gravity must be passed in. */
        public void Move(Vector2 deltaPosition)
        {
            // todo: add some special-cased sort of move initial/and or depenetration/overlap resolution (and at end)
            _collisions = CollisionFlags2D.None;

            // scale deltas in proportion to the y-axis
            Vector2 up         = _body.transform.up.normalized;
            Vector2 vertical   = Vector2.Dot(deltaPosition, up) * up;
            Vector2 horizontal = deltaPosition - vertical;

            // note that we resolve horizontal first as the movement is simpler than vertical
            MoveHorizontal(horizontal);
            MoveVertical(vertical);
        }



        /* Iteratively move body along surface one linear step at a time until target reached, or iteration cap exceeded. */
        private void MoveHorizontal(Vector2 initialDelta)
        {
            Vector2 delta = initialDelta;
            for (int i = 0; i < _maxIterations && !ApproximatelyZero(delta); i++)
            {
                // move a single linear step along our delta until the detected collision
                ExtrapolateLinearStep(delta, out Vector2 step, out RaycastHit2D hit);
                delta -= step;

                // move directly to target if unobstructed
                if (!hit)
                {
                    _body.position += step;
                    continue;
                }

                // unless there's an overly steep slope, move a linear step with properties taken into account
                if (Vector2.Angle(Vector2.up, hit.normal) <= _maxAngle)
                {
                    delta += ComputeCollisionDelta(step, hit.normal);
                }
                _body.position += delta;
            }
        }


        /* Iteratively move body along surface one linear step at a time until target reached, or iteration cap exceeded. */
        private void MoveVertical(Vector2 initialDelta)
        {
            Vector2 delta = initialDelta;
            for (int i = 0; i < _maxIterations && !ApproximatelyZero(delta); i++)
            {
                // move a single linear step along our delta until the detected collision
                ExtrapolateLinearStep(delta, out Vector2 step, out RaycastHit2D hit);
                delta -= step;

                // move directly to target if unobstructed
                if (!hit)
                {
                    _body.position += step;
                    delta = Vector2.zero;
                    continue;
                }

                // only if there's an overly steep slope, do we want to take action (eg sliding down)
                if (Vector2.Angle(Vector2.up, hit.normal) > _maxAngle)
                {
                    delta += ComputeCollisionDelta(step, hit.normal);
                }
                _body.position += delta;
            }
        }


        /*
        Compute projection of AABB linearly along given delta until first obstruction. Takes skin width into account.
        */
        private void ExtrapolateLinearStep(Vector2 delta, out Vector2 step, out RaycastHit2D hit)
        {
            float maxDistance = delta.magnitude;
            int hitCount = _aabb.Cast(delta, _castFilter, _castHits, delta.magnitude, ignoreSiblingColliders: true);
            if (delta == Vector2.zero || hitCount < 1)
            {
                step = delta;
                hit = default;
                return;
            }

            int closestHitIndex = 0;
            for (int i = 0; i < hitCount; i++)
            {
                DrawCastResultAsLineInEditor(_castHits[i], delta, _skinWidth);
                if (_castHits[i].distance < _castHits[closestHitIndex].distance)
                {
                    closestHitIndex = i;
                }
            }
            hit = _castHits[closestHitIndex];

            ComputeRayOffset(delta, out Vector2 _, out Vector2 offset);
            step = hit.point - hit.centroid - offset;
        }


        /*
        Apply bounciness/friction coefficients to hit position/normal, in proportion with the desired movement distance.

        In other words, for a given collision what is the adjusted delta when taking impact angle, velocity, bounciness,
        and friction into account (using a linear model similar to Unity's dynamic physics)?
        
        Note that collisions are resolved via: adjustedDelta = moveDistance * [(Sbounciness)Snormal + (1-Sfriction)Stangent]
        * where bounciness is from 0 (no bounciness) to 1 (completely reflected)
        * friction is from -1 ('boosts' velocity) to 0 (no resistance) to 1 (max resistance)
        */
        private Vector2 ComputeCollisionDelta(Vector2 desiredDelta, Vector2 hitNormal, float bounciness=0.00f, float friction=1.00f)
        {
            float remainingDistance = desiredDelta.magnitude;
            Vector2 reflected  = Vector2.Reflect(desiredDelta, hitNormal);
            Vector2 projection = Vector2.Dot(reflected, hitNormal) * hitNormal;
            Vector2 tangent    = reflected - projection;

            Vector2 perpendicularContribution = (bounciness      * remainingDistance) * projection.normalized;
            Vector2 tangentialContribution    = ((1f - friction) * remainingDistance) * tangent.normalized;
            return perpendicularContribution + tangentialContribution;
        }

        /* What's the delta between the AABB and the expanded AAB (with skin width) from center in given direction? */
        private void ComputeRayOffset(Vector2 direction, out Vector2 deltaInner, out Vector2 deltaOuter)
        {
            Vector2 center    = Vector2.zero;
            Vector2 size      = new(_aabb.bounds.size.x, _aabb.bounds.size.y);
            Vector2 maxOffset = new(_skinWidth, _skinWidth);

            Ray    ray   = new(center, direction);
            Bounds inner = new(center, size);
            Bounds outer = new(center, size + maxOffset);
            inner.IntersectRay(ray, out float distanceToInner);
            outer.IntersectRay(ray, out float distanceToOuter);

            deltaInner = distanceToInner * direction.normalized;
            deltaOuter = distanceToOuter * direction.normalized;
        }

        /* What's the delta between the AABB and the expanded AAB (with skin width) from center in given direction? */
        private Vector2 ComputeContactOffset(Vector2 direction)
        {
            if (Mathf.Approximately(_skinWidth, 0f))
            {
                return Vector2.zero;
            }

            Vector2 center = Vector2.zero;
            Vector2 size = new(_aabb.bounds.size.x, _aabb.bounds.size.y);
            Vector2 maxOffset = new(_skinWidth, _skinWidth);

            Ray ray = new(center, direction);
            Bounds inner = new(center, size);
            Bounds outer = new(center, size + maxOffset);
            inner.IntersectRay(ray, out float distanceToInner);
            outer.IntersectRay(ray, out float distanceToOuter);
            return (distanceToOuter - distanceToInner) * direction.normalized;
        }

        private static void DrawCastResultAsLineInEditor(RaycastHit2D hit, Vector2 delta, float offset)
        {
            if (!hit)
            {
                // unfortunately we can't reliably find the origin of the cast
                // if there was no hit (as far as I'm aware), so nothing to draw
                return;
            }
            
            float duration  = Time.fixedDeltaTime;
            Vector2 direction = delta.normalized;
            Vector2 start     = hit.point - hit.distance * direction;
            Vector2 origin    = hit.point - (hit.distance - offset) * direction;
            Vector2 hitPoint  = hit.point;
            Vector2 end       = hit.point + (1f - hit.fraction) * (delta.magnitude + offset) * direction;

            Debug.DrawLine(start,    origin,   Color.magenta, duration);
            Debug.DrawLine(origin,   hitPoint, Color.green,   duration);
            Debug.DrawLine(hitPoint, end,      Color.red,     duration);
        }
    }
}
