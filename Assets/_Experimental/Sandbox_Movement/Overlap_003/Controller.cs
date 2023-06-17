using UnityEngine;
using UnityEngine.InputSystem;


namespace PQ._Experimental.Overlap_003
{
    public class Controller : MonoBehaviour
    {
        [SerializeField] private Body _body;

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
            Physics2D.queriesStartInColliders = true;
            _body.CastCapsule(castDirection, castDistance, out RaycastHit2D hit);

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
        }


        void OnDrawGizmos()
        {
            
        }
    }
}
