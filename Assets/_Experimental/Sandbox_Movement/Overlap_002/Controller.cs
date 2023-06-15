using UnityEngine;
using UnityEngine.InputSystem;


namespace PQ._Experimental.Overlap_002
{
    public class Controller : MonoBehaviour
    {
        [SerializeField] private Body _body;

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
            Vector2 origin = _body.Position;

            Debug.DrawLine(origin, origin + castDistance * castDirection, Color.red, drawDuration);
            if (_body.CastAABB(castDirection, castDistance, out RaycastHit2D hit))
            {
                Debug.DrawLine(origin, origin + hit.distance * castDirection, Color.green, drawDuration);
            }

            ColliderDistance2D minimumSeparation = _body.ComputeMinimumSeparation(hit.collider);

            Vector2 pointA        = minimumSeparation.pointA;
            Vector2 pointB        = minimumSeparation.pointB;
            Vector2 offset        = minimumSeparation.distance * minimumSeparation.normal;
            Vector2 markerExtents = 0.05f * Vector2.Perpendicular(offset);

            Debug.DrawLine(pointB, pointA, Color.black, drawDuration);
            Debug.DrawLine(pointA - markerExtents, pointA + markerExtents, Color.black, drawDuration);

            Debug.DrawLine(origin, origin + offset, Color.blue, drawDuration);
            _body.MoveBy(offset);
        }
    }
}
