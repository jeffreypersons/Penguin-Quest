using UnityEngine;


namespace PQ._Experimental.Physics.Contact_006
{
    public class Controller : MonoBehaviour
    {
        [SerializeField] private Body _body;
        [SerializeField] [Range(0f, 1f)] private float _contactOffset = 0.05f;

        private ContactFlags2D _flags;

        void Awake()
        {
            Application.targetFrameRate = 60;
            _body = new Body(transform);
            _flags = ContactFlags2D.None;
        }

        void FixedUpdate()
        {
            _flags = _body.CheckForOverlappingContacts(skinWidth: _contactOffset);
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
