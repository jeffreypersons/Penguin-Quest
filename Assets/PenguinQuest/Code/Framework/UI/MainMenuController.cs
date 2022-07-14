using PenguinQuest.Data;
using PenguinQuest.Extensions;
using UnityEngine;
using UnityEngine.UI;


namespace PenguinQuest.Controllers.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI subtitle = default;
        
        [Header("Scene to load on game start")]
        [SerializeField] private string sceneName = default;
        
        [Header("Menu Core Components")]
        [SerializeField] private GameObject              buttonPanel             = default;
        [SerializeField] private MainMenuPanelController mainMenuPanelController = default;
        
        [Header("Menu Buttons")]
        [SerializeField] private Button startButton    = default;
        [SerializeField] private Button settingsButton = default;
        [SerializeField] private Button aboutButton    = default;
        [SerializeField] private Button quitButton     = default;
        
        void Awake()
        {
            if (!Scenes.IsSceneAbleToLoad(sceneName))
            {
                Debug.LogError($"Scene cannot be loaded, perhaps `{sceneName}` is misspelled?");
            }
            mainMenuPanelController.SetActionOnStartPressed(() => LoadGame());
            mainMenuPanelController.SetActionOnPanelOpen(()    => ToggleMainMenuVisibility(false));
            mainMenuPanelController.SetActionOnPanelClose(()   => ToggleMainMenuVisibility(true));

            #if UNITY_WEBGL
                UiUtils.SetButtonActiveAndEnabled(quitButton, false);
            #endif
        }

        void OnEnable()
        {
            startButton   .onClick.AddListener(mainMenuPanelController.OpenStartPanel);
            settingsButton.onClick.AddListener(mainMenuPanelController.OpenSettingsPanel);
            aboutButton   .onClick.AddListener(mainMenuPanelController.OpenAboutPanel);
            quitButton    .onClick.AddListener(Scenes.QuitGame);
        }
        void OnDisable()
        {
            startButton   .onClick.RemoveListener(mainMenuPanelController.OpenStartPanel);
            settingsButton.onClick.RemoveListener(mainMenuPanelController.OpenSettingsPanel);
            aboutButton   .onClick.RemoveListener(mainMenuPanelController.OpenAboutPanel);
            quitButton    .onClick.RemoveListener(Scenes.QuitGame);
        }

        private void LoadGame()
        {
            Scenes.LoadScene(sceneName, () =>
            {
                GameEventCenter.startNewGame.Trigger(mainMenuPanelController.GetGameSettings());
            });
        }

        private void ToggleMainMenuVisibility(bool isVisible)
        {
            UiExtensions.SetLabelVisibility(subtitle, isVisible);
            buttonPanel.SetActive(isVisible);
            #if UNITY_WEBGL
                UiUtils.SetButtonActiveAndEnabled(quitButton, false);
            #endif
        }
    }
}
