using UnityEngine;
using PenguinQuest.Utils;
using PenguinQuest.Data;


namespace PenguinQuest.Controllers.AlwaysOnComponents
{
    /*
    Provides functionality querying the surrounding of a bounding box.

    For example, is there something X distance in front of me?
    What about the front half of the box's bottom side?
    */
    public class CollisionChecker : MonoBehaviour
    {
        // todo: look into integrating with box perimeter caster? Or at least putting things in different places
        [Header("Ground Settings")]
        [SerializeField] private BoxCollider2D colliderToCastFrom;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] [Range(  0.25f, 25.00f)] private float toleratedDistanceFromGround = 0.30f;
        [SerializeField] private RayCasterSettings perimeterCasterSettings = default;

        private BoxPerimeterRayCaster _perimeterCaster;
        public bool    IsGrounded    { get; private set; } = false;
        public Vector2 SurfaceNormal { get; private set; } = Vector2.up;

        void Start()
        {
            _perimeterCaster = new BoxPerimeterRayCaster(colliderToCastFrom, perimeterCasterSettings);
            CheckForGround();
        }

        void FixedUpdate()
        {
            CheckForGround();
        }

        public void CheckForGround()
        {
            _perimeterCaster.CastAll();

            CastHit? hit = _perimeterCaster.BottomResults[1].hit;
            if (hit.HasValue && hit.Value.distance <= toleratedDistanceFromGround)
            {
                IsGrounded    = true;
                SurfaceNormal = _perimeterCaster.BottomResults[1].hit.Value.normal;
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
                return;
            }

            foreach (CastResult result in _perimeterCaster.AllResults)
            {
                GizmosUtils.DrawLine(from: result.origin, to: result.terminal, color: Color.red);
                if (result.hit != null)
                {
                    GizmosUtils.DrawLine(from: result.origin, to: result.hit.Value.point, color: Color.green);
                }
            }
        }
        #endif
    }
}
