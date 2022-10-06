using System;
using UnityEngine;
using UnityEngine.UI;
using CustomAttributes;
using PQ.Common.Extensions;


namespace PQ.Game.UI
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
        [SerializeField] private GameObject _startPanel;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _aboutPanel;

        [Header("Setting Controllers")]
        [SerializeField] private SliderSettingController _difficultySetting;
        [SerializeField] private SliderSettingController _numberOfLivesSetting;
        [SerializeField] private SliderSettingController _soundVolumeSetting;
        [SerializeField] private SliderSettingController _musicVolumeSetting;

        [SerializeField] [TagSelector] private string _startButtonTag;
        [SerializeField] [TagSelector] private string _cancelButtonTag;

        private event Action _actionOnStartPress;
        private event Action _actionOnPanelOpen;
        private event Action _actionOnPanelClose;

        void OnEnable()
        {
            DeactivePanels();
        }
        public void OpenStartPanel()
        {
            OpenPanel(_startPanel);
        }
        public void OpenSettingsPanel()
        {
            OpenPanel(_settingsPanel);
        }
        public void OpenAboutPanel()
        {
            OpenPanel(_aboutPanel);
        }

        public void SetActionOnStartPressed(Action actionOnStartPress)
        {
            this._actionOnStartPress = actionOnStartPress;
        }
        public void SetActionOnPanelOpen(Action actionOnPanelOpen)
        {
            this._actionOnPanelOpen = actionOnPanelOpen;
        }
        public void SetActionOnPanelClose(Action actionOnPanelClose)
        {
            this._actionOnPanelClose = actionOnPanelClose;
        }
        
        public PlayerSettingsInfo GetGameSettings()
        {
            return new PlayerSettingsInfo(
                numberOfLives:      (int)_numberOfLivesSetting.SliderValue,
                difficultyPercent:  (int)_difficultySetting.SliderValue,
                soundVolumePercent: (int)_soundVolumeSetting.SliderValue,
                musicVolumePercent: (int)_musicVolumeSetting.SliderValue
            );
        }
        private void DeactivePanels()
        {
            _startPanel   .SetActive(false);
            _settingsPanel.SetActive(false);
            _aboutPanel   .SetActive(false);
        }
        
        private void OpenPanel(GameObject submenuPanel)
        {
            if (_startPanel.activeInHierarchy || _settingsPanel.activeInHierarchy || _aboutPanel.activeInHierarchy)
            {
                Debug.LogError($"Cannot open {submenuPanel.name}, since only one sub-mainmenu panel can be active at a time.");
            }

            submenuPanel.SetActive(true);
            Button startButton = GetComponentInChildWithTag<Button>(submenuPanel, _startButtonTag, true);
            if (startButton)
            {
                UiExtensions.AddAutoUnsubscribeOnClickListenerToButton(startButton, () =>
                {
                    _actionOnStartPress();
                });
            }
            Button closeButton = GetComponentInChildWithTag<Button>(submenuPanel, _cancelButtonTag, true);
            if (closeButton)
            {
                UiExtensions.AddAutoUnsubscribeOnClickListenerToButton(closeButton, () =>
                {
                    DeactivePanels();
                    _actionOnPanelClose();
                });
            }
            _actionOnPanelOpen();
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
