using System;
using System.Linq;
using System.Text;
using UnityEngine;
using Cinemachine;


namespace PQ.Game.Camera
{
    /*
    Camera controller for scripting with our game's Cinemachine setup.


    Notes
    - Subjects are assigned at game start, rather than in inspector, to simplify serialization
      (otherwise we have to account for property being reassigned via inspector during runtime, etc)
    */
    public class CameraController : MonoBehaviour
    {
        private Transform _followSubject = null;

        [Tooltip("Base camera for Cinemachine")]
        [SerializeField] private UnityEngine.Camera _mainCamera = null;

        [Tooltip("Base brain for Cinemachine (must be attached to main camera")]
        [SerializeField] private CinemachineBrain _brain = null;

        [Tooltip("Configured Cinemachine virtual cameras")]
        [SerializeField] private CinemachineVirtualCamera[] _virtualCameras = Array.Empty<CinemachineVirtualCamera>();

        public override string ToString()
        {
            return $"{GetType()}(gameObject:{base.name})" +
                $"\n  follow: {_followSubject?.name ?? "<none>"}";
        }

        public Transform FollowSubject
        {
            get
            {
                return _followSubject;
            }
            set
            {
                if (ReferenceEquals(_followSubject, value) && ReferenceEquals(_followSubject, _mainCamera) && _virtualCameras.All(vCam => ReferenceEquals(vCam.Follow, value)))
                {
                    return;
                }

                StringBuilder message = new($"Set follow target to '{value?.name}' (was previously '{_followSubject?.name?? "<none>"}')");
                _followSubject = value;
                for (int i = 0; i < _virtualCameras.Length; i++)
                {
                    _virtualCameras[i].Follow = _followSubject;
                    message.Append($", set {_virtualCameras[i].name} follow object to '{value?.name}'");
                }
                Debug.Log(message.ToString());
            }
        }


        void OnEnable()
        {
            if (_mainCamera == null)
            {
                throw new MissingComponentException($"Missing component for {base.name} - Main camera unassigned");
            }
            if (_brain == null)
            {
                throw new MissingComponentException($"Missing component for {base.name} - brain unassigned");
            }
            if (_virtualCameras == null || Array.Exists(_virtualCameras, vCam => vCam == null))
            {
                throw new MissingComponentException($"Missing component(s) for {base.name} - virtual camera(s) unassigned");
            }
        }
    }
}
