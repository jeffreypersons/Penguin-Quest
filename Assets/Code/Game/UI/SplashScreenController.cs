using PQ.Game.Entities;
using UnityEngine;
using UnityEngine.UI;


namespace PQ.Game.UI
{
    // todo: when incorporating this stuff directly in game state machine, get rid of the events,
    //       and instead expose update stamina/etc directly
    public class SplashScreenController : MonoBehaviour
    {
        [Header("Hud Elements")]
        [SerializeField] private TMPro.TextMeshProUGUI _title;
        [SerializeField] private TMPro.TextMeshProUGUI _subtitle;
        [SerializeField] private TMPro.TextMeshProUGUI _prompt;

        void Awake()
        {
            // TBD
        }

        void OnEnable()
        {
            // TBD
        }

        void OnDisable()
        {
            // TBD
        }

        private void HandleAnyButtonPressed()
        {
            // TBD - load scene?
        }
    }
}
