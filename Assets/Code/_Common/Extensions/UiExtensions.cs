using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace PQ.Common.Extensions
{
    /*
    Note that these methods use old style for loops so that the found value CAN be modified after return.
    */
    public static class UiExtensions
    {
        /* Register a onetime use callback with given button that automatically unsubscribes itself after first click. */
        public static void AddAutoUnsubscribeOnClickListenerToButton(Button button, System.Action onButtonClicked)
        {
            // note null initialization is required to force nonlocal scope of the handler, see https://stackoverflow.com/a/1362244
            UnityAction handler = null;
            handler = () =>
            {
                button.onClick.RemoveListener(handler);
                onButtonClicked.Invoke();
            };
            button.onClick.AddListener(handler);
        }


        /* Toggle visibility of given sprite renderer. */
        public static void SetSpriteVisibility(SpriteRenderer spriteRenderer, bool isVisible)
        {
            spriteRenderer.enabled = isVisible;
        }

        /* Toggle visibility of given label. */
        public static void SetLabelVisibility(TMPro.TextMeshProUGUI label, bool isVisible)
        {
            label.enabled = isVisible;
        }

        /*
        Hide button without the overhead or implication of `SetActive`.
    
        Assumes active button with or without a textmeshpro child-label.
        */
        public static void SetButtonVisibility(Button button, bool isVisible)
        {
            button.enabled       = isVisible;
            button.image.enabled = isVisible;

            TMPro.TextMeshProUGUI buttonText = button.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (buttonText)
            {
                buttonText.enabled = isVisible;
            }
        }

        /* Fully enable/disable the button, affecting any attached scripts/listeners (unlike just changing visibility). */
        public static void SetButtonActiveAndEnabled(Button button, bool isActiveAndEnabled)
        {
            button.gameObject.SetActive(isActiveAndEnabled);
            button.enabled       = isActiveAndEnabled;
            button.image.enabled = isActiveAndEnabled;
        }
    }
}
