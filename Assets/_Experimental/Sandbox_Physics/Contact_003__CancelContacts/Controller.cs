using UnityEngine;


namespace PQ._Experimental.Physics.Contact_003
{
    public class Controller : MonoBehaviour
    {
        [SerializeField] private Body _body;

        private ContactFlags2D _flags;

        void Awake()
        {
            Application.targetFrameRate = 60;
            _body = new Body(transform);
            _flags = ContactFlags2D.None;
        }

        void FixedUpdate()
        {
            _flags = _body.CheckSides();
            if (_body.IsInsideAnEdgeCollider(out var collider))
            {
                Debug.Log($"isInside={collider.name}");
            }
        }

        void OnDrawGizmos()
        {
            if (Application.IsPlaying(this))
            {
                GizmoExtensions.DrawText(_body.Position, $"flags={_flags}");
            }
        }
    }
}
