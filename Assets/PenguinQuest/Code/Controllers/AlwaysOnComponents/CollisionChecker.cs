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
    public class CollisionChecker : MonoBehaviour
    {
        [Header("Ground Settings")]
        [SerializeField] private Collider2D colliderToCastFrom;
        [SerializeField] private LayerMask groundMask;
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
}
