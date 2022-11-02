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
        private bool             _flippedHorizontal;
        private bool             _flippedVertical;
        private float            _castOffset;
        private RayCaster        _caster;
        private Rigidbody2D      _rigidBody;
        private Collider2D       _collider;
        private OrientedBounds2D _currentBounds;
        private OrientedBounds2D _extrapolatedBounds;

        public OrientedBounds2D Current      => _currentBounds;
        public OrientedBounds2D Extrapolated => _extrapolatedBounds;
        public bool FlippedHorizontal        => _flippedHorizontal;
        public bool FlippedVertical          => _flippedVertical;

        public float CastOffset       { get => _castOffset;              set => _castOffset = value;              }
        public bool  DrawCastInEditor { get => _caster.DrawCastInEditor; set => _caster.DrawCastInEditor = value; }

        public override string ToString() =>
            $"{GetType().Name}(" +
                $"current:{Current}," +
                $"extrapolated:{Extrapolated})";

        /* Force a move to current position and rotation, discarding any extrapolated movement. */
        public void PlaceAt(Vector2 position, float rotation)
        {
            _rigidBody.transform.position = position;
            _rigidBody.transform.localEulerAngles = new Vector3(0, 0, rotation);
            _currentBounds.SetFromCollider(_collider);
            _extrapolatedBounds.SetFromCollider(_collider);
        }

        /* Force a move to current position and rotation, discarding any extrapolated movement. */
        public void Flip(bool horizontal, bool vertical)
        {
            Vector3 orientation = _rigidBody.transform.localEulerAngles;
            _rigidBody.transform.localEulerAngles = new Vector3(
                vertical?   -180 : 0,
                horizontal? -180 : 0,
                orientation.z
            );

            _currentBounds     .SetFromCollider(_collider);
            _extrapolatedBounds.SetFromCollider(_collider);
            _flippedHorizontal = horizontal;
            _flippedVertical   = vertical;
        }


        public void MoveBy(Vector2 amount)  => _extrapolatedBounds.MoveBy(amount);
        public void RotateBy(float degrees) => _extrapolatedBounds.RotateBy(degrees);
        public RayHit CastBehind(float yOffset, LayerMask mask, float distance) => Cast(-1f, yOffset, _extrapolatedBounds.Back,    mask, distance);
        public RayHit CastFront(float yOffset,  LayerMask mask, float distance) => Cast( 1f, yOffset, _extrapolatedBounds.Forward, mask, distance);
        public RayHit CastBelow(float xOffset,  LayerMask mask, float distance) => Cast(xOffset, -1f, _extrapolatedBounds.Below,   mask, distance);
        public RayHit CastAbove(float xOffset,  LayerMask mask, float distance) => Cast(xOffset,  1f, _extrapolatedBounds.Above,   mask, distance);

        // more expensive cast in arbitrary direction as it has to account for any angle of intersection with bounds
        public RayHit Cast(Vector2 direction, LayerMask layerMask, float distance)
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
        private RayHit Cast(float tXAxis, float tYAxis, Vector2 direction, LayerMask layerMask, float distance)
        {
            return _caster.CastFromPoint(
                point:     _extrapolatedBounds.InterpolateAlongAxes(tXAxis, tYAxis),
                direction: direction,
                layerMask: layerMask,
                distance:  distance,
                offset:    _castOffset);
        }


        void Awake()
        {
            _caster = new();
            _currentBounds = new();
            _extrapolatedBounds = new();
            _collider = gameObject.GetComponent<Collider2D>();
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
            _caster.DrawCastInEditor = true;
            _rigidBody.isKinematic = true;
            _currentBounds.SetFromCollider(_collider);
            _extrapolatedBounds.SetFromCollider(_collider);
        }

        void FixedUpdate()
        {
            ApplyAnyAccumulatedChanges();
        }


        public bool ApplyAnyAccumulatedChanges()
        {
            if (_currentBounds == _extrapolatedBounds)
            {
                return false;
            }

            var timeDelta     = Time.fixedDeltaTime;
            var rotationDelta = _currentBounds.ComputeRotationDelta(_extrapolatedBounds.Forward);
            var positionDelta = _currentBounds.ComputePositionDelta(_extrapolatedBounds.Center);
            if (!Mathf.Approximately(rotationDelta, 0f))
            {
                Vector2 referenceAxis = _flippedHorizontal? Vector2.left : Vector2.right;
                float currentRotation = _extrapolatedBounds.ComputeRotationDelta(referenceAxis);
                _rigidBody.MoveRotation(currentRotation + (rotationDelta * timeDelta));
            }
            if (!Mathf.Approximately(positionDelta.x, 0f) || !Mathf.Approximately(positionDelta.y, 0f))
            {
                _rigidBody.MovePosition(_extrapolatedBounds.Center + (positionDelta * timeDelta));
            }
            return _currentBounds.SetFrom(_extrapolatedBounds);
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
