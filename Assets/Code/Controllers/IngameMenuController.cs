using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class IngameMenuController : MonoBehaviour
{
    [SerializeField] private GameObject ingameMenu = default;
    [SerializeField] private TMPro.TextMeshProUGUI title    = default;
    [SerializeField] private TMPro.TextMeshProUGUI subtitle = default;
    [SerializeField] private Button resumeButton   = default;
    [SerializeField] private Button mainMenuButton = default;
    [SerializeField] private Button restartButton  = default;
    [SerializeField] private Button quitButton     = default;

    private List<Button> buttonsToHideWhenActive;
    private List<TMPro.TextMeshProUGUI> labelsToHideWhenActive;
    private List<SpriteRenderer> spritesToHideWhenActive;

    private static string subtitleSuffix = "points scored";
    [SerializeField] private string titleOnPause    = default;
    [SerializeField] private string titleOnGameOver = default;
    [TagSelector] [SerializeField] private string[] tagsOfButtonsToHideOnMenuOpen = new string[] { };
    [TagSelector] [SerializeField] private string[] tagsOfLabelsToHideOnMenuOpen  = new string[] { };
    [TagSelector] [SerializeField] private string[] tagsOfSpritesToHideOnMenuOpen = new string[] { };

    void Awake()
    {
        ingameMenu.SetActive(false);  // ensures that the fetched objects are OUTSIDE the ingame menu
        buttonsToHideWhenActive = ObjectUtils.FindAllComponentsInObjectsWithTags<Button>(tagsOfButtonsToHideOnMenuOpen);
        labelsToHideWhenActive  = ObjectUtils.FindAllComponentsInObjectsWithTags<TMPro.TextMeshProUGUI>(tagsOfLabelsToHideOnMenuOpen);
        spritesToHideWhenActive = ObjectUtils.FindAllComponentsInObjectsWithTags<SpriteRenderer>(tagsOfSpritesToHideOnMenuOpen);

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
    private void ToggleMenuVisibility(bool isVisible)
    {
        Time.timeScale = isVisible? 0 : 1;
        ingameMenu.SetActive(isVisible);

        bool hideBackground = !isVisible;
        for (int i = 0; i < buttonsToHideWhenActive.Count; i++)
        {
            UiUtils.SetButtonVisibility(buttonsToHideWhenActive[i], hideBackground);
        }
        for (int i = 0; i < labelsToHideWhenActive.Count; i++)
        {
            UiUtils.SetLabelVisibility(labelsToHideWhenActive[i], hideBackground);
        }
        for (int i = 0; i < spritesToHideWhenActive.Count; i++)
        {
            UiUtils.SetSpriteVisibility(spritesToHideWhenActive[i], hideBackground);
        }
        #if UNITY_WEBGL
            UiUtils.SetButtonActiveAndEnabled(quitButton, false);
        #endif
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
        GameEventCenter.resumeGame.Trigger("Resuming game");
        ToggleMenuVisibility(false);
    }
    private void MoveToMainMenu()
    {
        GameEventCenter.gotoMainMenu.Trigger("Opening main menu");
        Time.timeScale = 1;
        SceneUtils.LoadScene("MainMenu");
    }
    private void TriggerRestartGameEvent()
    {
        GameEventCenter.restartGame.Trigger("Restarting game");
        ToggleMenuVisibility(false);
    }
}
