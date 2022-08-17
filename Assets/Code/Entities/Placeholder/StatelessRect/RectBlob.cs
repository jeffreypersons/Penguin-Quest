using UnityEngine;


namespace PQ.Entities.Placeholder
{
    [ExecuteAlways]
    [System.Serializable]
    [AddComponentMenu("RectBlob")]
    public class RectBlob : MonoBehaviour
    {
        [Header("Component References")]
        [SerializeField] private Rigidbody2D   _rigidBody;
        [SerializeField] private BoxCollider2D _boundingBoxCollider;

        public Rigidbody2D   Rigidbody           => _rigidBody;
        public BoxCollider2D ColliderBoundingBox => _boundingBoxCollider;
    }
}
