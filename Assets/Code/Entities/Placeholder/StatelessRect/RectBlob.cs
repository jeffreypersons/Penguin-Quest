using PQ.Common.Collisions;
using UnityEngine;


namespace PQ.Entities.Placeholder
{
    [ExecuteAlways]
    [System.Serializable]
    [AddComponentMenu("RectBlob")]
    public class RectBlob : MonoBehaviour
    {
        [Header("Component References")]
        [SerializeField] private Transform         _rootTransform;
        [SerializeField] private Rigidbody2D       _rigidBody;
        [SerializeField] private BoxCollider2D     _boundingBoxCollider;
        [SerializeField] private RayCasterSettings _castSettings;
        
        public Transform         Transform           => _rootTransform;
        public Rigidbody2D       Rigidbody           => _rigidBody;
        public BoxCollider2D     ColliderBoundingBox => _boundingBoxCollider;
        public RayCasterSettings CastSettings        => _castSettings;
    }
}
