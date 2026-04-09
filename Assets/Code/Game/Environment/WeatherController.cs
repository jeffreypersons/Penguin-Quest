using UnityEngine;


// todo: add custom parameter tuning via a scriptable object
public class WeatherController : MonoBehaviour
{
    [SerializeField] private ParticleSystem _snowParticleSystem;

    private Transform _followTarget;
    
    // todo: fix how this setup such that it just occupies the damn screenspace lol
    public Transform FollowTarget
    {
        get => _followTarget;
        set
        {
            Debug.LogFormat("Weather.FollowTarget set to {0} (previously {1})",
                $"{nameof(WeatherController)}.{nameof(FollowTarget)}",
                ExtractTargetName(_followTarget),
                ExtractTargetName(value));
            _followTarget = value;

            var ps = _snowParticleSystem.main;
            ps.simulationSpace = ParticleSystemSimulationSpace.Custom;
            ps.customSimulationSpace = _followTarget;
        }
    }
    
    private string ExtractTargetName(Transform target) =>
        (target == null || string.IsNullOrEmpty(target.name)) ? "<none>" : $"'{target.name}'";
}
