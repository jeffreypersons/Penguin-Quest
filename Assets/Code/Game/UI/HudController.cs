using PQ.Game.Entities;
using UnityEngine;
using UnityEngine.UI;


namespace PQ.Game.UI
{
    // todo: when incorporating this stuff directly in game state machine, get rid of the events,
    //       and instead expose update stamina/etc directly
    public class HudController : MonoBehaviour
    {
        [Header("Hud Elements")]
        [SerializeField] private Button                _menuButton;
        [SerializeField] private TMPro.TextMeshProUGUI _staminaLabel;

        private GameEventCenter _eventCenter;

        void Awake()
        {
            _eventCenter = GameEventCenter.Instance;
        }

        void OnEnable()
        {
            //_eventCenter.characterStatusChanged.AddHandler(HandleCharacterStatusChanged);
            //_menuButton.onClick.AddListener(_eventCenter.pauseGame.Raise);
        }

        void OnDisable()
        {
            //_eventCenter.characterStatusChanged.RemoveHandler(HandleCharacterStatusChanged);
            //_menuButton.onClick.RemoveListener(_eventCenter.pauseGame.Raise);
        }

        public void UpdateStaminaBar()
        {

        }

        private void HandleCharacterStatusChanged(CharacterStatus characterStatus)
        {
            //UpdateStamina(characterStatus.Stamina);
        }


        private void UpdateStamina(float staminaRatio)
        {
            // todo: replace with format config object of some kind set in editor
            _staminaLabel.text = $"Health{Mathf.RoundToInt(staminaRatio*100)}%";
        }
    }
}
