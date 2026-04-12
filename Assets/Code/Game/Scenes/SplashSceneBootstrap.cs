using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;
using PQ.Game.Input;
using PQ.Game.UI;


namespace PQ.Game.Scenes
{
    /*
    Bootstraps the Splash scene by programmatically creating all required UI elements.

    Attach this to a single empty GameObject in the Splash scene. Everything else is
    created at runtime, so the scene file stays minimal.
    */
    public class SplashSceneBootstrap : MonoBehaviour
    {
        void Awake()
        {
            CreateEventSystem();
            CreateUI();
        }

        private void CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }

        private void CreateUI()
        {
            // canvas
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // background
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(canvasGo.transform, false);
            var bgImage = bgGo.AddComponent<Image>();
            bgImage.color = new Color(0.05f, 0.05f, 0.15f, 1f);
            var bgRect = bgGo.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // title
            var title = CreateText(canvasGo.transform, "Title", "Penguin Quest", 72,
                new Vector2(0.5f, 0.65f));

            // subtitle
            var subtitle = CreateText(canvasGo.transform, "Subtitle", "A Penguin Adventure", 32,
                new Vector2(0.5f, 0.5f));
            subtitle.color = new Color(0.7f, 0.8f, 1f, 1f);

            // prompt (hidden initially by SplashScreenController)
            var prompt = CreateText(canvasGo.transform, "Prompt", "Press Enter to Start", 24,
                new Vector2(0.5f, 0.25f));
            prompt.color = new Color(1f, 1f, 1f, 0.7f);

            // controllers
            var controllersGo = new GameObject("Controllers");
            controllersGo.transform.SetParent(canvasGo.transform, false);

            controllersGo.AddComponent<UiInputReceiver>();

            var splashController = controllersGo.AddComponent<SplashScreenController>();
            splashController.Initialize(title, subtitle, prompt);
        }

        private TextMeshProUGUI CreateText(Transform parent, string name, string text,
            float fontSize, Vector2 anchorPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, anchorPosition.y);
            rect.anchorMax = new Vector2(1, anchorPosition.y);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0, fontSize * 1.5f);

            return tmp;
        }
    }
}
