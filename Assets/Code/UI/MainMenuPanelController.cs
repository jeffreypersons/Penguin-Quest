using System;
using UnityEngine;
using UnityEngine.UI;
using CustomAttributes;
using PQ.Common.Extensions;


namespace PQ.UI
{
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
        public void OpenStartPanel()
        {
            OpenPanel(startPanel);
        }
        public void OpenSettingsPanel()
        {
            OpenPanel(settingsPanel);
        }
        public void OpenAboutPanel()
        {
            OpenPanel(aboutPanel);
        }

        public void SetActionOnStartPressed(Action actionOnStartPress)
        {
            this.actionOnStartPress = actionOnStartPress;
        }
        public void SetActionOnPanelOpen(Action actionOnPanelOpen)
        {
            this.actionOnPanelOpen = actionOnPanelOpen;
        }
        public void SetActionOnPanelClose(Action actionOnPanelClose)
        {
            this.actionOnPanelClose = actionOnPanelClose;
        }
        
        public PlayerSettingsInfo GetGameSettings()
        {
            return new PlayerSettingsInfo(
                numberOfLives:      (int)numberOfLivesSetting.SliderValue,
                difficultyPercent:  (int)difficultySetting.SliderValue,
                soundVolumePercent: (int)soundVolumeSetting.SliderValue,
                musicVolumePercent: (int)musicVolumeSetting.SliderValue
            );
        }
        private void DeactivePanels()
        {
            startPanel   .SetActive(false);
            settingsPanel.SetActive(false);
            aboutPanel   .SetActive(false);
        }
        
        private void OpenPanel(GameObject submenuPanel)
        {
            if (startPanel.activeInHierarchy || settingsPanel.activeInHierarchy || aboutPanel.activeInHierarchy)
            {
                Debug.LogError($"Cannot open {submenuPanel.name}, since only one sub-mainmenu panel can be active at a time.");
            }

            submenuPanel.SetActive(true);
            Button startButton = GetComponentInChildWithTag<Button>(submenuPanel, startButtonTag, true);
            if (startButton)
            {
                UiExtensions.AddAutoUnsubscribeOnClickListenerToButton(startButton, () =>
                {
                    actionOnStartPress();
                });
            }
            Button closeButton = GetComponentInChildWithTag<Button>(submenuPanel, cancelButtonTag, true);
            if (closeButton)
            {
                UiExtensions.AddAutoUnsubscribeOnClickListenerToButton(closeButton, () =>
                {
                    DeactivePanels();
                    actionOnPanelClose();
                });
            }
            actionOnPanelOpen();
        }


        /* Return the first found child matching given tag and component type. */
        private static T GetComponentInChildWithTag<T>(GameObject parent, string tag, bool includeInactive = false)
        {
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                Transform child = parent.transform.GetChild(i);
                if (child == null)
                {
                    continue;
                }
                if (!child.CompareTag(tag))
                {
                    continue;
                }
                if (!includeInactive && !child.gameObject.activeSelf)
                {
                    continue;
                }

                if (child.TryGetComponent(out T component))
                {
                    return component;
                }
            }
            return default;
        }
    }
}
