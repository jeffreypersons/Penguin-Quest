using UnityEngine;
using UnityEngine.UI;
using PQ.Common.Extensions;


namespace PQ.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI _subtitle;
        
        [Header("Scene to load on game start")]
        [SerializeField] private string _sceneName;
        
        [Header("Menu Core Components")]
        [SerializeField] private GameObject _buttonPanel;
        [SerializeField] private MainMenuPanelController _mainMenuPanelController;
        
        [Header("Menu Buttons")]
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _aboutButton;
        [SerializeField] private Button _quitButton;

        private GameEventCenter _eventCenter;

        void Awake()
        {
            _eventCenter = GameEventCenter.Instance;

            if (!SceneExtensions.IsSceneAbleToLoad(_sceneName))
            {
                Debug.LogError($"Scene cannot be loaded, perhaps `{_sceneName}` is misspelled?");
            }
            _mainMenuPanelController.SetActionOnStartPressed(() => LoadGame());
            _mainMenuPanelController.SetActionOnPanelOpen(()    => ToggleMainMenuVisibility(false));
            _mainMenuPanelController.SetActionOnPanelClose(()   => ToggleMainMenuVisibility(true));

            #if UNITY_WEBGL
                UiExtensions.SetButtonActiveAndEnabled(quitButton, false);
            #endif
        }

        void OnEnable()
        {
            _startButton   .onClick.AddListener(_mainMenuPanelController.OpenStartPanel);
            _settingsButton.onClick.AddListener(_mainMenuPanelController.OpenSettingsPanel);
            _aboutButton   .onClick.AddListener(_mainMenuPanelController.OpenAboutPanel);
            _quitButton    .onClick.AddListener(SceneExtensions.QuitGame);
        }
        void OnDisable()
        {
            _startButton   .onClick.RemoveListener(_mainMenuPanelController.OpenStartPanel);
            _settingsButton.onClick.RemoveListener(_mainMenuPanelController.OpenSettingsPanel);
            _aboutButton   .onClick.RemoveListener(_mainMenuPanelController.OpenAboutPanel);
            _quitButton    .onClick.RemoveListener(SceneExtensions.QuitGame);
        }


        private void LoadGame()
        {
            SceneExtensions.LoadScene(_sceneName, () =>
            {
                _eventCenter.startNewGame.Trigger(_mainMenuPanelController.GetGameSettings());
            });
        }

        private void ToggleMainMenuVisibility(bool isVisible)
        {
            UiExtensions.SetLabelVisibility(_subtitle, isVisible);
            _buttonPanel.SetActive(isVisible);
            #if UNITY_WEBGL
                UiExtensions.SetButtonActiveAndEnabled(quitButton, false);
            #endif
        }
    }
}
