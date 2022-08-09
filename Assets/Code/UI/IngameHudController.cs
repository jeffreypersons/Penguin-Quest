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

        private GameEventCenter _eventCenter;

        void Awake()
        {
            _eventCenter = GameEventCenter.Instance;

            // todo: make this script more interdependent
            _lastRecordedPlayerInfo = new PlayerProgressionInfo(PlayerProgressionInfo.MIN_LIVES_GIVEN);
            _levelLabel.text        = _lastRecordedPlayerInfo.LevelName;
        }

        void OnEnable()
        {
            _eventCenter.scoreChange.AddListener(UpdateScore);
            _pauseButton.onClick    .AddListener(TriggerPauseGameEvent);
        }
        void OnDisable()
        {
            _eventCenter.scoreChange.RemoveListener(UpdateScore);
            _pauseButton.onClick    .RemoveListener(TriggerPauseGameEvent);
        }

        private void UpdateScore(PlayerProgressionInfo playerInfo)
        {
            _lastRecordedPlayerInfo = playerInfo;
            _scoreLabel.text = _scorePrefix + playerInfo.Score.ToString();
            _livesLabel.text = _livesPrefix + playerInfo.Lives.ToString();
        }
        private void TriggerPauseGameEvent()
        {
            _eventCenter.pauseGame.Trigger(_lastRecordedPlayerInfo);
        }
    }
}
