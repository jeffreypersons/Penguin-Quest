using UnityEngine;
using UnityEngine.UI;


namespace PQ.UI
{
    public class IngameHudController : MonoBehaviour
    {
        [Header("Visible Ui Elements")]
        [SerializeField] private Button pauseButton = default;
        [SerializeField] private TMPro.TextMeshProUGUI levelLabel = default;
        [SerializeField] private TMPro.TextMeshProUGUI scoreLabel = default;
        [SerializeField] private TMPro.TextMeshProUGUI livesLabel = default;

        [Header("Menu Text")]
        [SerializeField] private string scorePrefix = default;
        [SerializeField] private string livesPrefix = default;

        private PlayerProgressionInfo lastRecordedPlayerInfo;

        void Awake()
        {
            // todo: make this script more interdependent
            lastRecordedPlayerInfo = new PlayerProgressionInfo(PlayerProgressionInfo.MIN_LIVES_GIVEN);
            levelLabel.text        = lastRecordedPlayerInfo.LevelName;
        }
        void OnEnable()
        {
            GameEventCenter.scoreChange.AddListener(UpdateScore);
            pauseButton.onClick.AddListener(TriggerPauseGameEvent);
        }
        void OnDisable()
        {
            GameEventCenter.scoreChange.RemoveListener(UpdateScore);
            pauseButton.onClick.RemoveListener(TriggerPauseGameEvent);
        }

        private void UpdateScore(PlayerProgressionInfo playerInfo)
        {
            lastRecordedPlayerInfo = playerInfo;
            scoreLabel.text = scorePrefix + playerInfo.Score.ToString();
            livesLabel.text = livesPrefix + playerInfo.Lives.ToString();
        }
        private void TriggerPauseGameEvent()
        {
            GameEventCenter.pauseGame.Trigger(lastRecordedPlayerInfo);
        }
    }
}
