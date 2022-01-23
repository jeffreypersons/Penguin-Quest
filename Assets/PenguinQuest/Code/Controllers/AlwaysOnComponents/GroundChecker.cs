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
    public class GroundChecker : MonoBehaviour
    {
        [Header("Ground Settings")]
        [SerializeField] private Collider2D colliderToCastFrom;
        [SerializeField] private LayerMask groundMask = default;
        [SerializeField] [Range(-10.00f, 25.00f)] private float offsetToCheckFrom           = 0.30f;
        [SerializeField] [Range(  0.25f, 25.00f)] private float toleratedDistanceFromGround = 0.30f;

        public bool    IsGrounded    { get; private set; } = false;
        public Vector2 SurfaceNormal { get; private set; } = Vector2.up;

        private LineCaster.Line _lastLine = default;
        private LineCaster.Hit  _lastHit  = default;
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
            if (Caster.CastFromCollider(colliderToCastFrom, downDirection, toleratedDistanceFromGround,
                                        out LineCaster.Line lineResult, out LineCaster.Hit lineHit))
            {
                IsGrounded    = true;
                _lastLine     = lineResult;
                _lastHit      = lineHit;
                SurfaceNormal = lineHit.normal;
            }
            else
            {
                IsGrounded    = false;
                _lastLine     = lineResult;
                _lastHit      = default;
                SurfaceNormal = Vector2.up;
            }
        }
        
        #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!Application.IsPlaying(this) || !enabled)
            {
                CheckForGround();
            }

            GizmosUtils.DrawLine(from: _lastLine.start, to: _lastLine.end, color: Color.red);
            if (IsGrounded)
            {
                GizmosUtils.DrawLine(from: _lastLine.start, to: _lastHit.point, color: Color.green);
            }
        }
        #endif
    }


    // todo: apply axis locking in the other script, and then expand this by adding more linecasts along the collider edges
    /*
    Provides a streamlined interface for casting lines from specific points or colliders.
    */
    public class LineCaster
    {
        public struct Line
        {
            public readonly Vector2 start;
            public readonly Vector2 end;
            public Line(Vector2 start, Vector2 end)
            {
                this.start = start;
                this.end   = end;
            }
        }
        public struct Hit
        {
            public readonly Vector2    point;
            public readonly Vector2    normal;
            public readonly float      distance;
            public readonly Collider2D collider;

            public Hit(Vector2 point, Vector2 normal, float distance, Collider2D collider)
            {
                this.point    = point;
                this.normal   = normal;
                this.distance = distance;
                this.collider = collider;
            }
        }

        public float     DistanceOffset { get; set; } = 0f;
        public LayerMask TargetLayers   { get; set; } = ~0;

        public LineCaster() { }
        
        /* Shoot out a line from point to max distance from that point until a TargetLayer is hit. */
        public bool CastFromPoint(Vector2 point, Vector2 direction, float distance, out Line castedLine, out Hit hit)
        {
            return CastLine(
                from:       point,
                to:         point + (distance * direction),
                castedLine: out castedLine,
                hit:        out hit);
        }

        /* Shoot out a line from edge of collider to distance from that point until a TargetLayer is hit. */
        public bool CastFromCollider(Collider2D collider, Vector2 direction, float distance, out Line castedLine, out Hit hit)
        {
            Vector2 point = FindPositionOnColliderEdgeInGivenDirection(collider, direction);
            return CastLine(
                from:       point,
                to:         point + (distance * direction),
                castedLine: out castedLine,
                hit:        out hit);
        }
        
        /* Shoot out a line between given points, seeing if a TargetLayer is hit. */
        public bool CastBetween(Vector2 from, Vector2 to, out Line castedLine, out Hit hit)
        {
            return CastLine(from, to, out castedLine, out hit);
        }

        private bool CastLine(Vector2 from, Vector2 to, out Line castedLine, out Hit hit)
        {
            Vector2 offset = DistanceOffset * (to - from).normalized;
            Vector2 start  = from + offset;
            Vector2 end    = to   + offset;

            RaycastHit2D rayHit = Physics2D.Linecast(start, end, TargetLayers);
            if (rayHit)
            {
                castedLine = new Line(start, end);
                hit        = new Hit(rayHit.point, rayHit.normal, rayHit.distance, rayHit.collider);
                return true;
            }
            else
            {
                castedLine = new Line(start, end);
                hit        = default;
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
