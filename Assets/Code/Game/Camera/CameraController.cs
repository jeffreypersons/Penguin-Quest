using System;
using System.Linq;
using System.Text;
using UnityEngine;
using Cinemachine;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;


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
        private Transform _followTarget = null;

        [Tooltip("Base camera for Cinemachine")]
        [SerializeField] private UnityEngine.Camera _mainCamera = null;

        [Tooltip("Base brain for Cinemachine (must be attached to main camera")]
        [SerializeField] private CinemachineBrain _brain = null;

        [Tooltip("Configured Cinemachine virtual cameras")]
        [SerializeField] private CinemachineVirtualCamera[] _virtualCameras = Array.Empty<CinemachineVirtualCamera>();

        public override string ToString()
        {
            // todo: add camera stats like viewport, etc
            return $"{GetType()}(gameObject:{base.name})" +
                $"\n  follow: {_followTarget?.name ?? "<none>"}";
        }

        private string ExtractTargetName(Transform target) =>
            (target == null || string.IsNullOrEmpty(target.name)) ? null : target.name;

        private bool IsFollowingTarget(Transform target) =>
            ReferenceEquals(_followTarget, target) &&
            _virtualCameras.All(vCam => ReferenceEquals(vCam.Follow, target));


        public Transform FollowTarget
        {
            get
            {
                return _followTarget;
            }
            set
            {
                if (IsFollowingTarget(value))
                {
                    return;
                }

                Debug.LogFormat($"Camera.FollowTarget set to '{0}' (previously '{1}')",
                    ExtractTargetName(_followTarget) ?? "<none>",
                    ExtractTargetName(value)         ?? "<none>");

                _followTarget = value;
                for (int i = 0; i < _virtualCameras.Length; i++)
                {
                    _virtualCameras[i].Follow = _followTarget;
                }
            }
        }

        void Awake()
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

            StringBuilder message = new();
            for (int i = 0; i < _virtualCameras.Length; i++)
            {
                _virtualCameras[i].Follow = _followTarget;
                message.Append($",{_virtualCameras[i].name}");
            }
            Debug.Log($"Camera attached to {base.name} is awake, with virtual cameras: [{0}]" + message.ToString());
        }
    }
}
