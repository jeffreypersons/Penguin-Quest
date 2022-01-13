using UnityEngine;


namespace PenguinQuest.Controllers
{
    /* 
    Provides functionality for checking if 'ground' is directly below given point.

    */
    [System.Serializable]
    [RequireComponent(typeof(Rigidbody2D))]
    [AddComponentMenu("GroundChecker")]
    public class GroundChecker : MonoBehaviour
    {
        [Tooltip("Surface Contact Detection Settings")]
        [SerializeField] private ContactFilter2D ContactFilter;

        private Rigidbody2D penguinRigidBody;

        // todo: add additional properties for checking contact angle, etc
        public bool IsGrounded => penguinRigidBody.IsTouching(ContactFilter);

        // todo: add proper angle support, but using 90 as a temp is okay for now
        public Vector2 SurfaceNormal => transform.up;
        public float DegreesFromSurfaceNormal => 90.00f;

        void Awake()
        {
            penguinRigidBody = gameObject.GetComponent<Rigidbody2D>();
        }

        void OnDrawGizmos()
        {

        }
    }
}
