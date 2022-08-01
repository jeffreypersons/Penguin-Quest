using UnityEngine;
using UnityEngine.UI;


namespace PQ.UI
{
    public class IngameHudController : MonoBehaviour
    {
        [Header("Visible Ui Elements")]
        [SerializeField] private Button _pauseButton;
        [SerializeField] private TMPro.TextMeshProUGUI _levelLabel;
        [SerializeField] private TMPro.TextMeshProUGUI _scoreLabel;
        [SerializeField] private TMPro.TextMeshProUGUI _livesLabel;

        [Header("Menu Text")]
        [SerializeField] private string _scorePrefix;
        [SerializeField] private string _livesPrefix;

        private PlayerProgressionInfo _lastRecordedPlayerInfo;

        void Awake()
        {
            // todo: make this script more interdependent
            _lastRecordedPlayerInfo = new PlayerProgressionInfo(PlayerProgressionInfo.MIN_LIVES_GIVEN);
            _levelLabel.text        = _lastRecordedPlayerInfo.LevelName;
        }
        void OnEnable()
        {
            GameEventCenter.scoreChange.AddListener(UpdateScore);
            _pauseButton    .onClick    .AddListener(TriggerPauseGameEvent);
        }
        void OnDisable()
        {
            GameEventCenter.scoreChange.RemoveListener(UpdateScore);
            _pauseButton    .onClick    .RemoveListener(TriggerPauseGameEvent);
        }

        private void UpdateScore(PlayerProgressionInfo playerInfo)
        {
            _lastRecordedPlayerInfo = playerInfo;
            _scoreLabel.text = _scorePrefix + playerInfo.Score.ToString();
            _livesLabel.text = _livesPrefix + playerInfo.Lives.ToString();
        }
        private void TriggerPauseGameEvent()
        {
            GameEventCenter.pauseGame.Trigger(_lastRecordedPlayerInfo);
        }
    }
}
