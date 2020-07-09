using UnityEngine;
using UnityEngine.UI;


public class IngameMenuController : MonoBehaviour
{
    [Header("Scene to Load on Return to Main Menu")]
    [SerializeField] private string mainMenuSceneName = default;

    [Header("Scene Objects to Hide on Menu Open")]
    [SerializeField] private GameObject topBanner = default;

    [Header("Menu Buttons")]
    [SerializeField] private GameObject ingameMenu = default;
    [SerializeField] private TMPro.TextMeshProUGUI title    = default;
    [SerializeField] private TMPro.TextMeshProUGUI subtitle = default;

    [Header("Menu Text")]
    [SerializeField] private string titleOnPause    = default;
    [SerializeField] private string titleOnGameOver = default;
    [SerializeField] private string subtitleSuffix  = default;

    [Header("Menu Buttons")]
    [SerializeField] private Button resumeButton   = default;
    [SerializeField] private Button mainMenuButton = default;
    [SerializeField] private Button restartButton  = default;
    [SerializeField] private Button quitButton     = default;

    private void ToggleMenuVisibility(bool isVisible)
    {
        if (isVisible)
        {
            Time.timeScale = 0;
            topBanner.SetActive(false);
            ingameMenu.SetActive(true);
        }
        else
        {
            Time.timeScale = 1;
            topBanner.SetActive(true);
            ingameMenu.SetActive(false);
        }
    }

    void Awake()
    {
        if (!SceneUtils.IsSceneAbleToLoad(mainMenuSceneName))
        {
            Debug.LogError($"Scene cannot be loaded, perhaps `{mainMenuSceneName}` is misspelled?");
        }
        ingameMenu.SetActive(false);
        GameEventCenter.pauseGame.AddListener(OpenAsPauseMenu);
        GameEventCenter.gameOver.AddListener(OpenAsEndGameMenu);

        #if UNITY_WEBGL
            UiUtils.SetButtonActiveAndEnabled(quitButton, false);
        #endif
    }
    void OnDestroy()
    {
        GameEventCenter.pauseGame.RemoveListener(OpenAsPauseMenu);
        GameEventCenter.gameOver.RemoveListener(OpenAsEndGameMenu);
    }

    void OnEnable()
    {
        resumeButton.onClick.AddListener(ResumeGame);
        mainMenuButton.onClick.AddListener(MoveToMainMenu);
        restartButton.onClick.AddListener(TriggerRestartGameEvent);
        quitButton.onClick.AddListener(SceneUtils.QuitGame);
    }
    void OnDisable()
    {
        resumeButton.onClick.RemoveListener(ResumeGame);
        mainMenuButton.onClick.RemoveListener(MoveToMainMenu);
        restartButton.onClick.RemoveListener(TriggerRestartGameEvent);
        quitButton.onClick.RemoveListener(SceneUtils.QuitGame);
    }

    private void OpenAsPauseMenu(PlayerInfo playerInfo)
    {
        title.text    = titleOnPause;
        subtitle.text = playerInfo.Score.ToString() + subtitleSuffix;
        UiUtils.SetButtonActiveAndEnabled(resumeButton, true);
        ToggleMenuVisibility(true);
    }
    private void OpenAsEndGameMenu(PlayerInfo playerInfo)
    {
        title.text    = titleOnGameOver;
        subtitle.text = playerInfo.Score.ToString() + subtitleSuffix;
        UiUtils.SetButtonActiveAndEnabled(resumeButton, false);
        ToggleMenuVisibility(true);
    }

    private void ResumeGame()
    {
        ToggleMenuVisibility(false);
        GameEventCenter.resumeGame.Trigger("Resuming game");
    }
    private void MoveToMainMenu()
    {
        Time.timeScale = 1;
        GameEventCenter.gotoMainMenu.Trigger("Opening main menu");
        SceneUtils.LoadScene(mainMenuSceneName);
    }
    private void TriggerRestartGameEvent()
    {
        ToggleMenuVisibility(false);
        GameEventCenter.restartGame.Trigger("Restarting game");
    }
}
