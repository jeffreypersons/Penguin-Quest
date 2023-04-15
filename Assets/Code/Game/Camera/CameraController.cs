using System;
using System.Linq;
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
        private Transform _followTarget = null;

        [Tooltip("Base camera for Cinemachine")]
        [SerializeField] private UnityEngine.Camera _mainCamera = null;

        [Tooltip("Base brain for Cinemachine (must be attached to main camera")]
        [SerializeField] private CinemachineBrain _brain = null;

        [Tooltip("Configured Cinemachine virtual cameras")]
        [SerializeField] private CinemachineVirtualCamera[] _virtualCameras = Array.Empty<CinemachineVirtualCamera>();


        private string ExtractTargetName(Transform target) =>
            (target == null || string.IsNullOrEmpty(target.name)) ? "<none>" : $"'{target.name}'";

        private bool IsFollowingTarget(Transform target) =>
            ReferenceEquals(_followTarget, target) &&
            _virtualCameras.All(vCam => ReferenceEquals(vCam.Follow, target));


        public override string ToString()
        {
            // todo: add camera stats like viewport, etc
            return $"{GetType()}(gameObject:{base.name})" +
                $"\n  follow: {ExtractTargetName(_followTarget)}";
        }

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

                Debug.LogFormat("Camera.FollowTarget set to {0} (previously {1})",
                    $"{nameof(CameraController)}.{nameof(FollowTarget)}",
                    ExtractTargetName(_followTarget),
                    ExtractTargetName(value));

                _followTarget = value;
                for (int i = 0; i < _virtualCameras.Length; i++)
                {
                    _virtualCameras[i].Follow = _followTarget;
                }
            }
        }


        void Awake()
        {
            EnsureComponentsNotMissing();

            Debug.LogFormat("{0} attached to {1} is awake, with virtual cameras: {2}",
                nameof(CameraController),
                base.name,
                $"[{string.Join(',', _virtualCameras.Select(vCam => vCam.name))}]");
        }

        #if UNITY_EDITOR
        void OnValidate()
        {
            // note: only need to enforce components are not missing _if_ changed in inspector
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                return;
            }
            EnsureComponentsNotMissing();
        }
        #endif


        private void EnsureComponentsNotMissing()
        {
            if (_mainCamera == null)
            {
                throw new MissingComponentException($"Missing component for {base.name} - main camera unassigned");
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
