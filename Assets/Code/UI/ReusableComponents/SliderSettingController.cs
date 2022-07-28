using System.Globalization;
using UnityEngine;
using UnityEngine.UI;


namespace PQ.UI
{
    /*
    Dynamic controller for sliders that update text with configured suffixes and number types (ie float/int).
     */
    [ExecuteAlways]
    public class SliderSettingController : MonoBehaviour
    {
        [SerializeField] private string description;
        [SerializeField] private string numberSuffix;
        [SerializeField] private string initialValue;
        [SerializeField] private string minValue;
        [SerializeField] private string maxValue;
        
        [SerializeField] private TMPro.TextMeshProUGUI label;
        [SerializeField] private Slider slider;
        
        public float  SliderValue => slider.value;
        public float  MinSliderValue => slider.minValue;
        public float  MaxSliderValue => slider.maxValue;
        public string Description => label.text;
        public bool   IsSliderValueAWholeNumber => slider.wholeNumbers;

        void Awake()
        {
            if (SetSliderValues(initialValue, minValue, maxValue))
            {
                slider.value = float.Parse(initialValue);
                UpdateLabel(slider.value);
            }
        }
        void Update()
        {
            SetSliderValues(initialValue, minValue, maxValue);
            #if UNITY_EDITOR
                UpdateLabel(slider.value);
            #endif
        }

        void OnEnable()
        {
            slider.onValueChanged.AddListener(UpdateLabel);
        }
        void OnDisable()
        {
            slider.onValueChanged.RemoveListener(UpdateLabel);
        }
        void UpdateLabel(float value)
        {
            label.text = $"{description}: {value}{numberSuffix}";
        }

        private bool SetSliderValues(string initial, string min, string max)
        {
            bool isAllInt   = IsAllInteger(initial, min, max);
            bool isAllFloat = IsAllFloat(initial, min, max);

            if (isAllInt || isAllFloat)
            {
                slider.minValue = float.Parse(min);
                slider.maxValue = float.Parse(max);
                slider.wholeNumbers = isAllInt;
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
