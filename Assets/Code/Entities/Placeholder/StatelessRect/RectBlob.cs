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
        [SerializeField] private RayCasterSettings             _castSettings;
        [SerializeField] private KinematicCharacter2D         _characterController;
        [SerializeField] private KinematicCharacter2DSettings _characterSettings;

        public Transform                     Transform           => _rootTransform;
        public RayCasterSettings             CastSettings        => _castSettings;
        public KinematicCharacter2D         CharacterController => _characterController;
        public KinematicCharacter2DSettings CharacterSettings   => _characterSettings;
    }
}
