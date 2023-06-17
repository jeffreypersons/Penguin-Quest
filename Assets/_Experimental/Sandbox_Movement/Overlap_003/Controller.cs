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
                Vector2 targetOffset = _target.bounds.center - _body.Bounds.center;

                float distance = targetOffset.magnitude;
                Vector2 direction = targetOffset.normalized;
                _body.CastCircle(direction, distance, out RaycastHit2D hit, true);

                Depenetrate(hit.collider, direction);
            }
        }


        private void Depenetrate(Collider2D collider, Vector2 direction)
        {
            MoveAwayFromSurface(collider, direction);
            SnapToCollider(collider);

        }

        private void MoveAwayFromSurface(Collider2D collider, Vector2 direction)
        {
            Vector2 startPosition = _body.Bounds.center;
            float distanceToEdge = _body.ComputeDistanceToEdge(direction);
            Debug.DrawLine(startPosition, startPosition + distanceToEdge * direction, Color.blue, 10f);

            _body.CastRayAt(collider, startPosition, direction, distanceToEdge, out RaycastHit2D hit, true);
            Debug.DrawLine(startPosition, startPosition + distanceToEdge * direction, Color.red, 10f);
            Debug.DrawLine(startPosition, hit.point, Color.green, 10f);

            Debug.Log(hit.collider.name);
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
                Debug.DrawLine(startPosition - markerExtents, startPosition + markerExtents, Color.red, 10f);
                Debug.DrawLine(startPosition, endPosition, Color.white, 10f);
            }
        }
    }
}
