using UnityEngine;
using UnityEngine.InputSystem;

namespace PQ._Experimental.Overlap_001
{
    public class Character : MonoBehaviour
    {
        private Mover _mover;
        private Vector2? _requestedPosition;

        
        void Awake()
        {
            Application.targetFrameRate = 60;
            _mover = new Mover(gameObject.transform);
            _requestedPosition = null;
        }

        void Update()
        {
            if (!_requestedPosition.HasValue && Mouse.current.leftButton.wasPressedThisFrame)
            {
                _requestedPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                Debug.Log(_requestedPosition);
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
        }
    }
}
