using UnityEngine;


// example usage:
//     [TagSelector] [SerializeField] private string[] tagsOfButtonsToHideOnMenuOpen = new string[] { };
//
// note: see `Editor/CustomRatioSliderPropertyDrawer` for implementation
public class RatioSliderAttribute : PropertyAttribute
{
    public float Min;
    public float Max;
    private float val;

    public float Value { get => val; set => Mathf.Clamp(val, Min, Max); }

    public RatioSliderAttribute(float min, float max)
    {
        Min = min;
        Max = max;
    }
}
