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
        [Header("Component References")]
        [SerializeField] private Transform                     _rootTransform;
        [SerializeField] private Rigidbody2D                   _rigidBody;
        [SerializeField] private BoxCollider2D                 _boundingBoxCollider;
        [SerializeField] private RayCasterSettings             _castSettings;
        [SerializeField] private CharacterController2D_v2      _characterController;
        [SerializeField] private CharacterController2DSettings _characterSettings;

        public Transform                     Transform           => _rootTransform;
        public Rigidbody2D                   Rigidbody           => _rigidBody;
        public BoxCollider2D                 ColliderBoundingBox => _boundingBoxCollider;
        public RayCasterSettings             CastSettings        => _castSettings;
        public CharacterController2D_v2      CharacterController => _characterController;
        public CharacterController2DSettings CharacterSettings   => _characterSettings;
    }
}
