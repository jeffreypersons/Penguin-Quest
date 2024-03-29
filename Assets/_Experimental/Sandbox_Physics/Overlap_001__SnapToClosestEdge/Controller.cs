using UnityEngine;
using UnityEngine.InputSystem;


namespace PQ._Experimental.Physics.Overlap_001
{
    public class Controller : MonoBehaviour
    {
        [SerializeField] private Body _body;
        //[SerializeField][Range(0, 100)] private int _maxMinSeparationSolves = 10; // need many if surfaces are complex, concave shapes

        private bool _nextButtonPressed;

        void Awake()
        {
            Application.targetFrameRate = 60;
            Physics2D.queriesStartInColliders = true;

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
            _body.CastAABB(castDirection, castDistance, out RaycastHit2D hit);

            for (int i = 0; i < 2; i++)
            {
                ColliderDistance2D minimumSeparation = _body.ComputeMinimumSeparation(hit.collider);
                float distance = minimumSeparation.distance;
                Vector2 pointA = minimumSeparation.pointA;
                Vector2 pointB = minimumSeparation.pointB;
                Vector2 normal = minimumSeparation.normal;

                Debug.DrawLine(pointA, pointB, Color.white, drawDuration);
                if (pointA != pointB)
                {
                    Vector2 markerExtents = 0.075f * Vector2.Perpendicular(normal);
                    Debug.DrawLine(pointA - markerExtents, pointA + markerExtents, Color.red, drawDuration);
                }

                Vector2 offset;
                if (minimumSeparation.isOverlapped)
                {
                    offset = minimumSeparation.distance * minimumSeparation.normal;
                }
                else
                {
                    offset = -minimumSeparation.distance * minimumSeparation.normal;
                }

                if (offset == Vector2.zero)
                {
                    break;
                }
                _body.MoveBy(offset);
            }
        }


        void OnDrawGizmos()
        {
            
        }
    }
}
