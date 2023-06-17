using UnityEngine;
using UnityEngine.InputSystem;


namespace PQ._Experimental.Overlap_003
{
    public class Controller : MonoBehaviour
    {
        [SerializeField] private Body _body;
        [SerializeField] [Range(0, 100)] private int _maxMinSeparationSolves = 10;

        private bool _nextButtonPressed;

        void Awake()
        {
            Application.targetFrameRate = 60;
            _nextButtonPressed = false;
        }

        void Update()
        {
            _nextButtonPressed = Keyboard.current[Key.Space].wasPressedThisFrame;
        }

        void FixedUpdate()
        {
            if (_nextButtonPressed)
            {
                HandleOverlapCheck(castDirection: Vector2.down, castDistance: 10f);
            }
        }


        private void HandleOverlapCheck(Vector2 castDirection, float castDistance)
        {
            _body.CastCircle(castDirection, castDistance, out RaycastHit2D hit, true);
            SnapToCollider(hit.collider);
            Debug.Log(_body.IsTouching(hit.collider));
        }


        private void SnapToCollider(Collider2D collider)
        {
            Vector2 startPosition = _body.Position;
            for (int i = 0; i < _maxMinSeparationSolves; i++)
            {
                ColliderDistance2D minSeparation = _body.ComputeMinimumSeparation(collider);
                Vector2 offset = minSeparation.distance * minSeparation.normal;
                if (offset == Vector2.zero)
                {
                    break;
                }
                _body.MoveBy(offset);
            }
            Vector2 endPosition = _body.Position;

            if (startPosition != endPosition)
            {
                Vector2 markerExtents = 0.075f * Vector2.Perpendicular(endPosition - startPosition);
                Debug.DrawLine(startPosition - markerExtents, startPosition + markerExtents, Color.red,   10f);
                Debug.DrawLine(startPosition, endPosition, Color.white, 10f);
            }
        }
    }
}
