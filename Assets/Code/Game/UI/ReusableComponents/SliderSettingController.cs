using System.Globalization;
using UnityEngine;
using UnityEngine.UI;


namespace PQ.Game.UI
{
    /*
    Dynamic controller for sliders that update text with configured suffixes and number types (ie float/int).
     */
    [ExecuteAlways]
    public class SliderSettingController : MonoBehaviour
    {
        [SerializeField] private string _description;
        [SerializeField] private string _numberSuffix;
        [SerializeField] private string _initialValue;
        [SerializeField] private string _minValue;
        [SerializeField] private string _maxValue;
        
        [SerializeField] private TMPro.TextMeshProUGUI _label;
        [SerializeField] private Slider _slider;
        
        public float  SliderValue               => _slider.value;
        public float  MinSliderValue            => _slider.minValue;
        public float  MaxSliderValue            => _slider.maxValue;
        public string Description               => _label.text;
        public bool   IsSliderValueAWholeNumber => _slider.wholeNumbers;

        void Awake()
        {
            if (SetSliderValues(_initialValue, _minValue, _maxValue))
            {
                _slider.value = float.Parse(_initialValue);
                UpdateLabel(_slider.value);
            }
        }
        void Update()
        {
            SetSliderValues(_initialValue, _minValue, _maxValue);
            #if UNITY_EDITOR
                UpdateLabel(_slider.value);
            #endif
        }

        void OnEnable()
        {
            _slider.onValueChanged.AddListener(UpdateLabel);
        }
        void OnDisable()
        {
            _slider.onValueChanged.RemoveListener(UpdateLabel);
        }
        void UpdateLabel(float value)
        {
            _label.text = $"{_description}: {value}{_numberSuffix}";
        }

        private bool SetSliderValues(string initial, string min, string max)
        {
            bool isAllInt   = IsAllInteger(initial, min, max);
            bool isAllFloat = IsAllFloat(initial, min, max);

            if (isAllInt || isAllFloat)
            {
                _slider.minValue = float.Parse(min);
                _slider.maxValue = float.Parse(max);
                _slider.wholeNumbers = isAllInt;
                return true;
            }
            else
            {
                Debug.LogError($"Expected min <= default <= max as all floats or all ints, " +
                               $"received {initial}, {min}, {max}` instead");
                return false;
            }
        }

        public static bool IsAllInteger(params string[] values)
        {
            foreach (string value in values)
            {
                if (!int.TryParse(value, out _))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool IsAllFloat(params string[] values)
        {
            foreach (string value in values)
            {
                if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
