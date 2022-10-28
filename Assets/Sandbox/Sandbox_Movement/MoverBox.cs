using UnityEngine;
using PQ.Common.Extensions;
using PQ.Common.Casts;


namespace PQ.Common.Physics
{
    /*
    Provides functionality querying the surrounding of a bounding box, and moving it then applying changes in next update.

    For example, is there something X distance in front of me?
    What about the front half of the box's bottom side?
    */
    public sealed class MoverBox : MonoBehaviour
    {
        private float   _degreesDelta;
        private Vector2 _positionDelta;

        private float            _castOffset;
        private RayCaster        _caster;
        private Rigidbody2D      _rigidBody;
        private Collider2D       _collider;
        private OrientedBounds2D _boundsLastFixedUpdate;
        private OrientedBounds2D _bounds;

        public OrientedBounds2D Current      => _boundsLastFixedUpdate;
        public OrientedBounds2D Extrapolated => _bounds;

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

        
        public void MoveBy(Vector2 delta)   => _positionDelta += delta;
        public void RotateBy(float degrees) => _degreesDelta  += degrees;

        public RayHit CastBehind(float t, in LayerMask mask, float distance) => CastFromSideAt(_bounds.Back,   t, mask, distance);
        public RayHit CastFront(float t,  in LayerMask mask, float distance) => CastFromSideAt(_bounds.Front,  t, mask, distance);
        public RayHit CastBelow(float t,  in LayerMask mask, float distance) => CastFromSideAt(_bounds.Bottom, t, mask, distance);
        public RayHit CastAbove(float t,  in LayerMask mask, float distance) => CastFromSideAt(_bounds.Top,    t, mask, distance);

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
                point:     _bounds.Back.PointAt(t),
                direction: side.normal,
                layerMask: layerMask,
                distance:  distance,
                offset:    _castOffset);
        }


        void Awake()
        {
            _castOffset = 0.0f;
            _collider   = gameObject.GetComponent<Collider2D>();
            _rigidBody  = gameObject.GetComponent<Rigidbody2D>();

            if (_collider == null)
            {
                throw new MissingComponentException($"Expected attached collider2D - not found on {gameObject}");
            }
            if (_rigidBody == null)
            {
                throw new MissingComponentException($"Expected attached rigidbody2D - not found on {gameObject}");
            }

            _caster = new();

            _rigidBody.isKinematic = true;
        }

        void FixedUpdate()
        {
            if (!Mathf.Approximately(_degreesDelta, 0f))
            {
                _rigidBody.MoveRotation(_rigidBody.rotation + _degreesDelta);
                _degreesDelta = 0f;
            }
            if (!Mathf.Approximately(_positionDelta.x, 0f) || !Mathf.Approximately(_positionDelta.x, 0f))
            {
                _rigidBody.MovePosition(_rigidBody.position + _positionDelta);
                _positionDelta = Vector2.zero;
            }
            _boundsLastFixedUpdate = _bounds;
        }



        #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!Application.IsPlaying(this) || !enabled)
            {
                return;
            }

            // draw a bounding box that should be identical to the BoxCollider2D bounds in the editor window
            // draw a pair of arrows from the that should be identical to the transform's axes in the editor window
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
