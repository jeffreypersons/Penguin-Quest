using UnityEngine;
using PenguinQuest.Utils;


namespace PenguinQuest.Controllers.AlwaysOnComponents
{
    /*
    Provides functionality for checking if 'ground' is directly below given point.
    */
    [ExecuteAlways]
    [System.Serializable]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class GroundChecker : MonoBehaviour
    {
        [Header("Ground Settings")]
        [SerializeField] private LayerMask groundMask = default;
        [SerializeField] [Range( -1.0f,  1.0f)] private float offsetToCheckFrom           = 0.30f;
        [SerializeField] [Range( 0.05f, 10.0f)] private float toleratedDistanceFromGround = 0.30f;

        private BoxCollider2D boundingBox;

        public bool    IsGrounded    { get; private set; } = false;
        public Vector2 SurfaceNormal { get; private set; } = Vector2.up;

        private LineCaster.Result _lastResult = default;
        private LineCaster _caster = default;
        private LineCaster Caster
        {
            get
            {
                if (_caster == default)
                {
                    _caster = new LineCaster();
                }
                _caster.TargetLayers   = groundMask;
                _caster.DistanceOffset = offsetToCheckFrom;
                return _caster;
            }
        }

        void Awake()
        {
            boundingBox = transform.GetComponent<BoxCollider2D>();
            boundingBox.enabled = true;
        }

        void Start()
        {
            CheckForGround();
        }

        void FixedUpdate()
        {
            CheckForGround();
        }

        /*
        Check for ground below the our source object.
    
        Some extra line height is used as padding to ensure it starts just above our targeted layer if given.
        */
        public void CheckForGround()
        {
            Vector2 downDirection = (-1f * transform.up).normalized;

            if (Caster.CastFromCollider(boundingBox, downDirection, toleratedDistanceFromGround, out LineCaster.Result result))
            {
                IsGrounded = true;
                _lastResult = result;
                SurfaceNormal = result.normal;
            }
            else
            {
                IsGrounded = false;
                _lastResult = result;
                SurfaceNormal = Vector2.up;
            }
        }
        
        #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!Application.IsPlaying(this))
            {
                if (boundingBox == default)
                {
                    boundingBox = transform.GetComponent<BoxCollider2D>();
                }
                CheckForGround();
            }
            GizmosUtils.DrawLine(from: _lastResult.origin, to: _lastResult.point, color: Color.red);
            if (IsGrounded)
            {
                GizmosUtils.DrawLine(from: _lastResult.origin, to: _lastResult.point, color: Color.green);
            }
        }
        #endif
    }


    /*
    Provides a streamlined interface for casting lines from specific points or colliders.
    */
    public class LineCaster
    {
        public struct Result
        {
            public Vector2 origin;
            public Vector2 point;
            public Vector2 normal;
            public float distance;
        }
        
        public float     DistanceOffset { get; set; } = 0f;
        public LayerMask TargetLayers   { get; set; } = ~0;

        public LineCaster() { }
        
        /* Shoot out a line from point to max distance from that point until a TargetLayer is hit. */
        public bool CastFromPoint(Vector2 point, Vector2 direction, float distance, out Result result)
        {
            return CastBetween(
                from:   point,
                to:     point + (distance * direction),
                result: out result);
        }

        /* Shoot out a line from edge of collider to distance from that point until a TargetLayer is hit. */
        public bool CastFromCollider(Collider2D collider, Vector2 direction, float distance, out Result result)
        {
            Vector2 point = FindPositionOnColliderEdgeInGivenDirection(collider, direction);
            return CastBetween(
                from:   point,
                to:     point + (distance * direction),
                result: out result);
        }

        /* Shoot out a line between given points, seeing if a TargetLayer is hit. */
        public bool CastBetween(Vector2 from, Vector2 to, out Result result)
        {
            Vector2 offset = DistanceOffset * (to - from).normalized;
            Vector2 start  = from + offset;
            Vector2 end    = to   + offset;
            float distance = (end - start).magnitude;

            RaycastHit2D hitInfo = Physics2D.Linecast(start, end, TargetLayers);
            if (hitInfo)
            {
                result = new Result()
                {
                    origin = start, point = hitInfo.centroid, normal = hitInfo.normal, distance = hitInfo.distance
                };
                return true;
            }
            else
            {
                result = new Result()
                {
                    origin = start, point = end, normal = Vector2.up, distance = distance
                };
                return false;
            }
        }

        private static Vector2 FindPositionOnColliderEdgeInGivenDirection(Collider2D collider, Vector2 direction)
        {
            Vector2 center = collider.bounds.center;
            collider.bounds.IntersectRay(new Ray(center, direction), out float distanceFromCenterToEdge);
            return center - (distanceFromCenterToEdge * direction);
        }

        private static bool IsInLayerMask(GameObject gameObject, LayerMask mask)
        {
            LayerMask maskForGameObject = 1 << gameObject.layer;
            return (mask & maskForGameObject) != 0;
        }
    }
}
