using Cinemachine;
using System;
using System.Linq;
using System.Text;
using UnityEngine;


/*
Camera controller for scripting with our game's Cinemachine setup.
*/
[ExecuteAlways]
public class CameraController : MonoBehaviour
{
    [Header("Subject to Follow")]
    [Tooltip("Transform (if any) to follow")]
    [SerializeField] private Transform _followSubject;

    [Tooltip("Base camera for Cinemachine")]
    [SerializeField] private CinemachineComponentBase _mainCam;

    [Tooltip("Configured Cinemachine virtual cameras")]
    [SerializeField] private CinemachineVirtualCamera[] _vCams = Array.Empty<CinemachineVirtualCamera>();

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
            if (ReferenceEquals(_followSubject, value) && ReferenceEquals(_followSubject, _mainCam) && _vCams.All(vCam => ReferenceEquals(vCam.Follow, value)))
            {
                return;
            }

            StringBuilder message = new($"Changed follow object from '{_followSubject?.name}' to '{value?.name}'");
            _followSubject = value;
            for (int i = 0; i < _vCams.Length; i++)
            {
                message.Append($", updated {_vCams[i].name} follow object from '{_vCams[i].Follow?.name}' to '{value?.name}'");
                _vCams[i].Follow = _followSubject;
            }

            Debug.Log(message.ToString());
        }
    }

    #if UNITY_EDITOR
    void OnValidate()
    {
        // cover assignment where changed in editor - force it to update any VCam follows 
        FollowSubject = _followSubject;
    }
    #endif

    void Start()
    {

    }

    void Update()
    {
        
    }
}
