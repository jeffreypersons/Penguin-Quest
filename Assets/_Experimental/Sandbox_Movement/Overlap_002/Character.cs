using UnityEngine;
using UnityEngine.InputSystem;


namespace PQ._Experimental.Overlap_002
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
            _overlapResolveRequested = Keyboard.current[Key.Space].wasPressedThisFrame;
            if (!_requestedPosition.HasValue && Mouse.current.leftButton.wasPressedThisFrame)
            {
                _requestedPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            }
            else
            {
                _requestedPosition = null;
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
                _mover.DepenetrateAlongLastMove();
            }
        }
    }
}
