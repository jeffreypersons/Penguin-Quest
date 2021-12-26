using System;
using UnityEngine;
using UnityEngine.UI;
using CustomAttributes;


/*
Controller for opening sub panels of the main menu.


Notes
- Only one panel can be open at a time
- All panels have a back button, and some may have a continue button
*/
public class MainMenuPanelController : MonoBehaviour
{
    [Header("Sub-panels")]
    [SerializeField] private GameObject startPanel    = default;
    [SerializeField] private GameObject settingsPanel = default;
    [SerializeField] private GameObject aboutPanel    = default;

    [Header("Setting Controllers")]
    [SerializeField] private SliderSettingController difficultySetting    = default;
    [SerializeField] private SliderSettingController numberOfLivesSetting = default;
    [SerializeField] private SliderSettingController soundVolumeSetting   = default;
    [SerializeField] private SliderSettingController musicVolumeSetting   = default;

    [SerializeField] [TagSelector] private string startButtonTag  = default;
    [SerializeField] [TagSelector] private string cancelButtonTag = default;

    private Action actionOnStartPress;
    private Action actionOnPanelOpen;
    private Action actionOnPanelClose;

    void OnEnable()
    {
        DeactivePanels();
    }
    public void OpenStartPanel()    { OpenPanel(startPanel);    }
    public void OpenSettingsPanel() { OpenPanel(settingsPanel); }
    public void OpenAboutPanel()    { OpenPanel(aboutPanel);    }

    public void SetActionOnStartPressed(Action actionOnStartPress) { this.actionOnStartPress = actionOnStartPress; }
    public void SetActionOnPanelOpen(Action actionOnPanelOpen)     { this.actionOnPanelOpen  = actionOnPanelOpen;  }
    public void SetActionOnPanelClose(Action actionOnPanelClose)   { this.actionOnPanelClose = actionOnPanelClose; }

    public GameSettingsInfo GetGameSettings()
    {
        return new GameSettingsInfo(
            numberOfLives:      (int)numberOfLivesSetting.SliderValue,
            difficultyPercent:  (int)difficultySetting.SliderValue,
            soundVolumePercent: (int)soundVolumeSetting.SliderValue,
            musicVolumePercent: (int)musicVolumeSetting.SliderValue
        );
    }
    private void DeactivePanels()
    {
        startPanel.SetActive(false);
        settingsPanel.SetActive(false);
        aboutPanel.SetActive(false);
    }

    private void OpenPanel(GameObject submenuPanel)
    {
        if (startPanel.activeInHierarchy || settingsPanel.activeInHierarchy || aboutPanel.activeInHierarchy)
        {
            Debug.LogError($"Cannot open {submenuPanel.name}, since only one sub-mainmenu panel can be active at a time.");
        }

        submenuPanel.SetActive(true);
        Button startButton = ObjectUtils.GetComponentInChildWithTag<Button>(submenuPanel, startButtonTag,  true);
        if (startButton)
        {
            UiUtils.AddAutoUnsubscribeOnClickListenerToButton(startButton, () =>
            {
                actionOnStartPress();
            });
        }
        Button closeButton = ObjectUtils.GetComponentInChildWithTag<Button>(submenuPanel, cancelButtonTag, true);
        if (closeButton)
        {
            UiUtils.AddAutoUnsubscribeOnClickListenerToButton(closeButton, () =>
            {
                DeactivePanels();
                actionOnPanelClose();
            });
        }
        actionOnPanelOpen();
    }
}
