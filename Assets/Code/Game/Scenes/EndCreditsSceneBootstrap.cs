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
    Bootstraps the End Credits scene by programmatically creating all required UI elements.

    Attach this to a single empty GameObject in the EndCredits scene. Everything else is
    created at runtime, so the scene file stays minimal.
    */
    public class EndCreditsSceneBootstrap : MonoBehaviour
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
            bgImage.color = new Color(0.02f, 0.02f, 0.08f, 1f);
            var bgRect = bgGo.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // title
            var title = CreateText(canvasGo.transform, "Title", "Thanks for Playing!", 64,
                new Vector2(0.5f, 0.7f));

            // credits
            var credits = CreateText(canvasGo.transform, "Credits",
                "Penguin Quest\n\nA Game by [Your Name]\n\nPlaceholder Credits", 28,
                new Vector2(0.5f, 0.45f));
            credits.color = new Color(0.8f, 0.85f, 1f, 1f);

            // prompt (hidden initially by EndCreditsController)
            var prompt = CreateText(canvasGo.transform, "Prompt", "Press Enter to Continue", 24,
                new Vector2(0.5f, 0.15f));
            prompt.color = new Color(1f, 1f, 1f, 0.7f);

            // controllers
            var controllersGo = new GameObject("Controllers");
            controllersGo.transform.SetParent(canvasGo.transform, false);

            controllersGo.AddComponent<UiInputReceiver>();

            var endCreditsController = controllersGo.AddComponent<EndCreditsController>();
            endCreditsController.Initialize(title, credits, prompt);
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
