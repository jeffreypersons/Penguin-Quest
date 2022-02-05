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
        [SerializeField] private BoxCollider2D colliderToCastFrom;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] [Range(-10.00f, 25.00f)] private float offsetToCheckFrom           = 0.30f;
        [SerializeField] [Range(  0.25f, 25.00f)] private float toleratedDistanceFromGround = 0.30f;
        
        public bool    IsGrounded    { get; private set; } = false;
        public Vector2 SurfaceNormal { get; private set; } = Vector2.up;


        [SerializeField] private BoxColliderPerimeterCaster _perimeterCaster = default;
        private BoxColliderPerimeterCaster PerimeterCaster
        {
            get
            {
                if (_perimeterCaster == default)
                {
                    _perimeterCaster = new BoxColliderPerimeterCaster(colliderToCastFrom);
                }
                _perimeterCaster.ChecksPerHorizontalSide = 3;
                _perimeterCaster.ChecksPerVerticalSide   = 3;
                _perimeterCaster.CastOffset              = offsetToCheckFrom;
                _perimeterCaster.TargetLayers            = groundMask;
                _perimeterCaster.MaxDistance             = toleratedDistanceFromGround;
                return _perimeterCaster;
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
            PerimeterCaster.Cast();

            if (PerimeterCaster.bottomResults[1].hit.HasValue)
            {
                IsGrounded    = true;
                SurfaceNormal = PerimeterCaster.bottomResults[1].hit.Value.normal;
            }
            else
            {
                IsGrounded    = false;
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

            foreach (BoxColliderPerimeterCaster.Result result in PerimeterCaster.allResults)
            {
                GizmosUtils.DrawLine(from: result.line.start, to: result.line.end, color: Color.red);
                if (result.hit != null)
                {
                    GizmosUtils.DrawLine(from: result.line.start, to: result.hit.Value.point, color: Color.green);
                }
            }
        }
        #endif
    }
}
