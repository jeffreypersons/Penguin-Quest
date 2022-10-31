using UnityEngine;
using PQ.Common.Extensions;
using PQ.Common.Casts;


// ----- todo -----
// - figure out a way to sync any changes to transform/rigidbody/collider made in editor or elsewhere..
//   maybe try just applying it as just another force?? Or skipping updating for the frame that occurs..or just ignore it?
//
namespace PQ.Common.Physics
{
    /*
    Provides functionality querying the surrounding of a bounding box, and moving it then applying changes in next update.

    For example, is there something X distance in front of me?
    What about the front half of the box's bottom side?
    */
    public sealed class KinematicMover2D : MonoBehaviour
    {
        private float            _castOffset;
        private RayCaster        _caster;
        private Rigidbody2D      _rigidBody;
        private Collider2D       _collider;
        private OrientedBounds2D _currentBounds;
        private OrientedBounds2D _extrapolatedBounds;

        public OrientedBounds2D Current      => _currentBounds;
        public OrientedBounds2D Extrapolated => _extrapolatedBounds;

        public float CastOffset       { get => _castOffset;              set => _castOffset = value;              }
        public bool  DrawCastInEditor { get => _caster.DrawCastInEditor; set => _caster.DrawCastInEditor = value; }

        public override string ToString() =>
            $"{GetType().Name}(" +
                $"current:{Current}," +
                $"extrapolated:{Extrapolated})";

        public void PlaceAt(Vector2 position, float rotation)
        {
            _rigidBody.transform.position = position;
            _rigidBody.transform.localEulerAngles = new Vector3(0, 0, rotation);
        }

        public void SetLocalOrientation3D(float xDegrees, float yDegrees, float zDegrees)
        {
            _rigidBody.transform.localEulerAngles = new Vector3(xDegrees, yDegrees, zDegrees);
        }

        public void MoveBy(Vector2 amount)  => _extrapolatedBounds.MoveBy(amount);
        public void RotateBy(float degrees) => _extrapolatedBounds.RotateBy(degrees);
        public RayHit CastBehind(float t, in LayerMask mask, float distance) => CastFromSideAt(_extrapolatedBounds.Back,   t, mask, distance);
        public RayHit CastFront(float t,  in LayerMask mask, float distance) => CastFromSideAt(_extrapolatedBounds.Front,  t, mask, distance);
        public RayHit CastBelow(float t,  in LayerMask mask, float distance) => CastFromSideAt(_extrapolatedBounds.Bottom, t, mask, distance);
        public RayHit CastAbove(float t,  in LayerMask mask, float distance) => CastFromSideAt(_extrapolatedBounds.Top,    t, mask, distance);

        // more expensive cast in arbitrary direction as it has to account for any angle of intersection with bounds
        public RayHit Cast(Vector2 direction, in LayerMask layerMask, float distance)
        {
            return _caster.CastFromColliderBounds(
                bounds:    _collider.bounds,
                direction: direction,
                layerMask: layerMask,
                distance:  distance,
                offset:    _castOffset);
        }

        // cheap cast in a direction perpendicular to bounds, with t clamped to [0,1], and mapped to relative minimum on that side
        // for example, front side with t = 1 is the top of the forward facing side, aka top right corner relative to orientation
        private RayHit CastFromSideAt(OrientedBounds2D.Side side, float t, in LayerMask layerMask, float distance)
        {
            return _caster.CastFromPoint(
                point:     _extrapolatedBounds.Back.PointAt(t),
                direction: side.normal,
                layerMask: layerMask,
                distance:  distance,
                offset:    _castOffset);
        }


        void Awake()
        {
            _caster    = new RayCaster { DrawCastInEditor = true };
            _collider  = gameObject.GetComponent<Collider2D>();
            _rigidBody = gameObject.GetComponent<Rigidbody2D>();
            if (_collider == null)
            {
                throw new MissingComponentException($"Expected attached collider2D - not found on {gameObject}");
            }
            if (_rigidBody == null)
            {
                throw new MissingComponentException($"Expected attached rigidbody2D - not found on {gameObject}");
            }

            _castOffset = 0.0f;
            _rigidBody.isKinematic = true;
            _currentBounds.SetFromCollider(_collider);
            _extrapolatedBounds.SetFromCollider(_collider);
        }

        void FixedUpdate()
        {
            ApplyAnyAccumulatedChanges(_rigidBody, _currentBounds, _extrapolatedBounds);
        }


        private static bool ApplyAnyAccumulatedChanges(Rigidbody2D rigidBody,
            OrientedBounds2D current, OrientedBounds2D extrapolated)
        {
            if (current == extrapolated)
            {
                return false;
            }

            var timeDelta     = Time.fixedDeltaTime;
            var rotationDelta = OrientedBounds2D.ComputeRotationDelta(current, extrapolated);
            var positionDelta = OrientedBounds2D.ComputePositionDelta(current, extrapolated);

            if (!Mathf.Approximately(rotationDelta, 0f))
            {
                rigidBody.MoveRotation(extrapolated.Rotation + (rotationDelta * timeDelta));
            }
            if (!Mathf.Approximately(positionDelta.x, 0f) || !Mathf.Approximately(positionDelta.y, 0f))
            {
                rigidBody.MovePosition(extrapolated.Center + (positionDelta * timeDelta));
            }
            return current.SetFrom(extrapolated);
        }
        

        #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!Application.IsPlaying(this) || !enabled)
            {
                return;
            }

            // draw a bounding box that should be identical to the BoxCollider2D bounds in the editor window,
            // then draw a pair of arrows from the that should be identical to the transform's axes in the editor window
            Vector2 center = Current.Center;
            Vector2 xAxis  = Current.XAxis;
            Vector2 yAxis  = Current.YAxis;
            GizmoExtensions.DrawRect(center, xAxis, yAxis, Color.gray);
            GizmoExtensions.DrawArrow(from: center, to: center + xAxis, color: Color.red);
            GizmoExtensions.DrawArrow(from: center, to: center + yAxis, color: Color.red);
        }
        #endif
    }
}
