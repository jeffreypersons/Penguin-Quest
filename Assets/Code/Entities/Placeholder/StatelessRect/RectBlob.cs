using UnityEngine;
using PQ.Common;
using PQ.Common.Collisions;


namespace PQ.Entities.Placeholder
{
    [ExecuteAlways]
    [System.Serializable]
    [AddComponentMenu("RectBlob")]
    public class RectBlob : MonoBehaviour
    {
        [Header("Setting Bundles")]
        [SerializeField] private CharacterController2DSettings _penguinOnFeetSettings;

        [Header("Component References")]
        [SerializeField] private Rigidbody2D           _rigidBody;
        [SerializeField] private CharacterController2D _characterController;
        [SerializeField] private CollisionChecker      _collisionChecker;
        [SerializeField] private Transform             _root;

        [Header("Collider References")]
        [SerializeField] private BoxCollider2D _boundingBoxCollider;

        public CharacterController2D CharacterController  => _characterController;
        public Rigidbody2D           Rigidbody            => _rigidBody;
        public Vector2               RootPosition         => _root.position;
        public BoxCollider2D         ColliderBoundingBox  => _boundingBoxCollider;
    }
}
