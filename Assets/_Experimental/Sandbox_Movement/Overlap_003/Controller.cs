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


        private void HandleOverlapCheck(Vector2 castDirection, float castDistance, float drawDuration=10f)
        {
            _body.CastCircle(castDirection, castDistance, out RaycastHit2D hit, true);

            Debug.Log(ComputeOverlapDistance(hit.collider));

            Physics2D.queriesStartInColliders = true;
            ColliderDistance2D minimumSeparation = _body.ComputeMinimumSeparation(hit.collider);
            float distance = minimumSeparation.distance;
            Vector2 pointA = minimumSeparation.pointA;
            Vector2 pointB = minimumSeparation.pointB;
            Vector2 normal = minimumSeparation.normal;
            Debug.DrawLine(pointA, pointB, Color.white, drawDuration);

            Debug.Log(distance);
            if (pointA != pointB)
            {
                Vector2 markerExtents = 0.075f * Vector2.Perpendicular(normal);
                Debug.DrawLine(pointA - markerExtents, pointA + markerExtents, Color.red, drawDuration);
            }
            _body.MoveBy(distance * normal);

            Debug.Log(ComputeOverlapDistance(hit.collider));
        }


        private float ComputeOverlapDistance(Collider2D collider)
        {
            ColliderDistance2D minSeparation = _body.ComputeMinimumSeparation(collider);
            if (minSeparation.distance >= 0f)
            {
                return 0f;
            }

            Vector2 startPosition = _body.Position;
            for (int i = 0; i < _maxMinSeparationSolves; i++)
            {
                Vector2 offset = minSeparation.distance * minSeparation.normal;
                if (offset == Vector2.zero)
                {
                    break;
                }
                _body.MoveBy(offset);
            }
            Vector2 endPosition = _body.Position;

            _body.MoveTo(startPosition);
            return Vector2.Distance(startPosition, endPosition);
        }
    }
}
