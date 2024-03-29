using UnityEngine;


namespace PQ._Experimental.Physics.Contact_001
{
    public class Controller : MonoBehaviour
    {
        [SerializeField] private Body _body;

        private CollisionFlags2D _flags;

        void Awake()
        {
            Application.targetFrameRate = 60;
            _body = new Body(transform);
            _flags = CollisionFlags2D.None;
        }

        void FixedUpdate()
        {
            _flags = _body.CheckSides();
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
