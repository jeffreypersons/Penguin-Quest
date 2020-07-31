using UnityEngine;
using UnityEngine.UI;


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

    private PlayerStatsInfo lastRecordedPlayerInfo;

    void Awake()
    {
        lastRecordedPlayerInfo = new PlayerStatsInfo(PlayerStatsInfo.MIN_LIVES_GIVEN);
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

    private void UpdateScore(PlayerStatsInfo playerInfo)
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
