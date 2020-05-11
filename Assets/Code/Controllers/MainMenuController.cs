using UnityEngine;
using UnityEngine.UI;


public class MainMenuController : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI subtitle = default;

    [SerializeField] private GameObject buttonPanel = default;
    [SerializeField] private MainMenuPanelController mainMenuPanelController = default;

    [Header("Menu Buttons")]
    [SerializeField] private Button startButton    = default;
    [SerializeField] private Button settingsButton = default;
    [SerializeField] private Button aboutButton    = default;
    [SerializeField] private Button quitButton     = default;

    void Awake()
    {
        mainMenuPanelController.SetActionOnStartPressed(() => LoadGame());
        mainMenuPanelController.SetActionOnPanelOpen(()    => ToggleMainMenuVisibility(false));
        mainMenuPanelController.SetActionOnPanelClose(()   => ToggleMainMenuVisibility(true));

        #if UNITY_WEBGL
            UiUtils.SetButtonActiveAndEnabled(quitButton, false);
        #endif
    }

    void OnEnable()
    {
        startButton.onClick.AddListener(mainMenuPanelController.OpenStartPanel);
        settingsButton.onClick.AddListener(mainMenuPanelController.OpenSettingsPanel);
        aboutButton.onClick.AddListener(mainMenuPanelController.OpenAboutPanel);
        quitButton.onClick.AddListener(SceneUtils.QuitGame);
    }
    void OnDisable()
    {
        startButton.onClick.RemoveListener(mainMenuPanelController.OpenStartPanel);
        settingsButton.onClick.RemoveListener(mainMenuPanelController.OpenSettingsPanel);
        aboutButton.onClick.RemoveListener(mainMenuPanelController.OpenAboutPanel);
        quitButton.onClick.RemoveListener(SceneUtils.QuitGame);
    }

    private void LoadGame()
    {
        SceneUtils.LoadScene("Game", () =>
        {
            GameEventCenter.startNewGame.Trigger(mainMenuPanelController.GetGameSettings());
        });
    }

    private void ToggleMainMenuVisibility(bool isVisible)
    {
        UiUtils.SetLabelVisibility(subtitle, isVisible);
        buttonPanel.SetActive(isVisible);
        #if UNITY_WEBGL
            UiUtils.SetButtonActiveAndEnabled(quitButton, false);
        #endif
    }
}
