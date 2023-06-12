using UnityEngine;
using UnityEngine.InputSystem;


namespace PQ._Experimental.Overlap_001
{
    public class Character : MonoBehaviour
    {
        [SerializeField] private Mover _mover;

        private Vector2? _requestedPosition;
        private bool _overlapResolveRequested;
        
        void Start()
        {
            Application.targetFrameRate = 60;
            _requestedPosition = null;
            _overlapResolveRequested = false;
        }

        void Update()
        {
            if (!_requestedPosition.HasValue && Mouse.current.leftButton.wasPressedThisFrame)
            {
                _requestedPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            }
            else
            {
                _requestedPosition = null;
            }

            if (Keyboard.current[Key.Space].isPressed)
            {
                _overlapResolveRequested = true;
            }
        }

        void FixedUpdate()
        {
            if (_requestedPosition.HasValue)
            {
                _mover.MoveTo(_requestedPosition.Value);
            }
            if (_overlapResolveRequested)
            {
                _mover.ResolveDepenetrationAlongLastMove();
            }
        }
    }
}
