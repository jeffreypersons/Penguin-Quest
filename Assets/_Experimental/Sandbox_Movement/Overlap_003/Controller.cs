using UnityEngine;
using UnityEngine.InputSystem;


namespace PQ._Experimental.Overlap_003
{
    public class Controller : MonoBehaviour
    {
        [SerializeField] private Body _body;
        [SerializeField] private Collider2D _target;
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
                CheckForObstructionAlongPathToTarget(out RaycastHit2D obstruction);
                MoveAwayFromObstruction(obstruction.collider);
                SnapToCollider(obstruction.collider);
            }
        }


        private void CheckForObstructionAlongPathToTarget(out RaycastHit2D obstruction)
        {
            Vector2 targetOffset = _target.bounds.center - _body.Bounds.center;

            float distance = targetOffset.magnitude;
            Vector2 direction = targetOffset.normalized;
            _body.CastCircle(direction, distance, out RaycastHit2D hit, true);
            obstruction = hit;
        }

        private void MoveAwayFromObstruction(Collider2D collider)
        {
            if (collider == null)
            {
                return;
            }

            Vector2 direction = (_target.bounds.center - _body.Bounds.center).normalized;
            float distanceToEdge = _body.ComputeDistanceToEdge();
            Debug.Log($"direction={direction} distance={distanceToEdge}");
            _body.CastRayAt(collider, _body.Bounds.center, direction, distanceToEdge, out RaycastHit2D hit, true, draw: true);
        }

        private void SnapToCollider(Collider2D collider)
        {
            Vector2 startPosition = _body.Bounds.center;
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
            Vector2 endPosition = _body.Bounds.center;

            if (startPosition != endPosition)
            {
                Vector2 markerExtents = 0.075f * Vector2.Perpendicular(endPosition - startPosition);
                Debug.DrawLine(startPosition - markerExtents, startPosition + markerExtents, Color.red, 10f);
                Debug.DrawLine(startPosition, endPosition, Color.white, 10f);
            }
        }
    }
}
