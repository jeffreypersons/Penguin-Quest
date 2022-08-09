using UnityEngine;
using UnityEngine.UI;
using PQ.Common.Extensions;


namespace PQ.UI
{
    public class IngameMenuController : MonoBehaviour
    {
        [Header("Scene to Load on Return to Main Menu")]
        [SerializeField] private string _mainMenuSceneName;

        [Header("Scene Objects to Hide on Menu Open")]
        [SerializeField] private GameObject _topBanner;

        [Header("Menu Buttons")]
        [SerializeField] private GameObject _ingameMenu;
        [SerializeField] private TMPro.TextMeshProUGUI _title;
        [SerializeField] private TMPro.TextMeshProUGUI _subtitle;

        [Header("Menu Text")]
        [SerializeField] private string _titleOnPause;
        [SerializeField] private string _titleOnGameOver;
        [SerializeField] private string _subtitleSuffix;

        [Header("Menu Buttons")]
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _quitButton;

        private GameEventCenter _eventCenter;

        private void ToggleMenuVisibility(bool isVisible)
        {
            if (isVisible)
            {
                Time.timeScale = 0;
                _topBanner.SetActive(false);
                _ingameMenu.SetActive(true);
            }
            else
            {
                Time.timeScale = 1;
                _topBanner.SetActive(true);
                _ingameMenu.SetActive(false);
            }
        }

        void Awake()
        {
            _eventCenter = GameEventCenter.Instance;

            _ingameMenu.SetActive(false);
            _eventCenter.pauseGame.AddListener(OpenAsPauseMenu);
            _eventCenter.gameOver .AddListener(OpenAsEndGameMenu);

            #if UNITY_WEBGL
                UiExtensions.SetButtonActiveAndEnabled(quitButton, false);
            #endif
        }
        void OnDestroy()
        {
            _eventCenter.pauseGame.RemoveListener(OpenAsPauseMenu);
            _eventCenter.gameOver .RemoveListener(OpenAsEndGameMenu);
        }

        void OnEnable()
        {
            _resumeButton  .onClick.AddListener(ResumeGame);
            _mainMenuButton.onClick.AddListener(MoveToMainMenu);
            _restartButton .onClick.AddListener(TriggerRestartGameEvent);
            _quitButton    .onClick.AddListener(SceneExtensions.QuitGame);
        }

        void OnDisable()
        {
            _resumeButton  .onClick.RemoveListener(ResumeGame);
            _mainMenuButton.onClick.RemoveListener(MoveToMainMenu);
            _restartButton .onClick.RemoveListener(TriggerRestartGameEvent);
            _quitButton    .onClick.RemoveListener(SceneExtensions.QuitGame);
        }


        private void OpenAsPauseMenu(PlayerProgressionInfo playerInfo)
        {
            _title.text = _titleOnPause;
            _subtitle.text = playerInfo.Score.ToString() + _subtitleSuffix;
            UiExtensions.SetButtonActiveAndEnabled(_resumeButton, true);
            ToggleMenuVisibility(true);
        }

        private void OpenAsEndGameMenu(PlayerProgressionInfo playerInfo)
        {
            _title.text = _titleOnGameOver;
            _subtitle.text = playerInfo.Score.ToString() + _subtitleSuffix;
            UiExtensions.SetButtonActiveAndEnabled(_resumeButton, false);
            ToggleMenuVisibility(true);
        }

        private void ResumeGame()
        {
            ToggleMenuVisibility(false);
            _eventCenter.resumeGame.Trigger("Resuming game");
        }

        private void MoveToMainMenu()
        {
            Time.timeScale = 1;
            _eventCenter.gotoMainMenu.Trigger("Opening main menu");
            SceneExtensions.LoadScene(_mainMenuSceneName);
        }

        private void TriggerRestartGameEvent()
        {
            ToggleMenuVisibility(false);
            _eventCenter.restartGame.Trigger("Restarting game");
        }
    }
}
