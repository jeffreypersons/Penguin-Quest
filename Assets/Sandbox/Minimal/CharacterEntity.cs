using UnityEngine;
using PQ.Common.Extensions;


namespace PQ.TestScenes.Minimal
{
    public class CharacterEntity : MonoBehaviour
    {
        [SerializeField] private float           _walkSpeed        = 25.0f;
        [SerializeField] private float           _skinWidth        =  2.5f;
        [SerializeField] private int             _maxMovementSteps =    10;
        [SerializeField] private ContactFilter2D _contactFilter    = default;

        private GameplayInput _input;
        private ICharacterController2D _mover;


        private void Awake()
        {
            _input = new();
            _mover = new SimpleCharacterController2D(gameObject, _contactFilter, _skinWidth, _maxMovementSteps);
        }


        private void Update()
        {
            _input.ReadInput();
        }


        private void FixedUpdate()
        {
            if (Mathf.Approximately(_input.Horizontal, 0f))
            {
                return;
            }

            bool characterMovingLeft = _input.Horizontal < 0;
            bool characterFacingLeft = _mover.Flipped;
            if (characterFacingLeft != characterMovingLeft)
            {
                _mover.Flip();
            }

            _mover.Move(new(_input.Horizontal * _walkSpeed * Time.fixedDeltaTime, 0));
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
            Vector2 center = _mover.Bounds.center;
            Vector2 xAxis  = _mover.Forward * _mover.Bounds.extents.x;
            Vector2 yAxis  = _mover.Up      * _mover.Bounds.extents.y;
            float xOffsetRatio = 1f + _mover.ContactOffset / _mover.Bounds.extents.x;
            float yOffsetRatio = 1f + _mover.ContactOffset / _mover.Bounds.extents.y;

            GizmoExtensions.DrawRect(center, xAxis, yAxis, Color.gray);
            GizmoExtensions.DrawRect(center, xOffsetRatio * xAxis, yOffsetRatio * yAxis, Color.magenta);
            GizmoExtensions.DrawArrow(from: center, to: center + xAxis, color: Color.red);
            GizmoExtensions.DrawArrow(from: center, to: center + yAxis, color: Color.green);
        }
        #endif
    }
}
