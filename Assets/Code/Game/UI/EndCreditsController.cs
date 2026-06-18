using UnityEngine;
using PQ.Common.Extensions;


namespace PQ.Game.UI
{
    public class EndCreditsController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TMPro.TextMeshProUGUI _title;
        [SerializeField] private TMPro.TextMeshProUGUI _credits;
        [SerializeField] private TMPro.TextMeshProUGUI _prompt;

        [Header("Settings")]
        [SerializeField] private float  _minimumDisplaySeconds = 3f;
        [SerializeField] private string _nextSceneName = "Splash";

        private GameEventCenter _eventCenter;
        private float _elapsedTime;
        private bool  _ready;
        private bool  _transitioning;

        public void Initialize(TMPro.TextMeshProUGUI title, TMPro.TextMeshProUGUI credits, TMPro.TextMeshProUGUI prompt)
        {
            _title = title;
            _credits = credits;
            _prompt = prompt;
        }

        void Awake()
        {
            _eventCenter = GameEventCenter.Instance;
            _elapsedTime = 0f;
            _ready = false;
            _transitioning = false;

            if (_prompt != null)
            {
                _prompt.enabled = false;
            }
        }

        void OnEnable()
        {
            _eventCenter.anyKeyPressed.AddHandler(HandleAnyKeyPressed);
        }

        void OnDisable()
        {
            _eventCenter.anyKeyPressed.RemoveHandler(HandleAnyKeyPressed);
        }

        void Update()
        {
            if (_ready || _transitioning)
            {
                return;
            }

            _elapsedTime += Time.unscaledDeltaTime;

            if (_elapsedTime >= _minimumDisplaySeconds)
            {
                _ready = true;
                if (_prompt != null)
                {
                    _prompt.enabled = true;
                }
            }
        }

        private void HandleAnyKeyPressed()
        {
            if (!_ready || _transitioning)
            {
                return;
            }

            _transitioning = true;
            SceneExtensions.LoadScene(_nextSceneName);
        }
    }
}
