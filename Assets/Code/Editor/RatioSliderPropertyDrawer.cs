using UnityEngine;
using UnityEditor;


[CustomPropertyDrawer(typeof(RatioSliderAttribute))]
public class CustomRatioSliderPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        RatioSliderAttribute ratio = attribute as RatioSliderAttribute;

        if (property.propertyType == SerializedPropertyType.Float)
        {
            EditorGUI.Slider(position, property, ratio.Min, ratio.Max, label);
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Ratio slider works only with floats.");
        }

        if (fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), true).Length == 0)
        {
            EditorGUI.LabelField(position, $"Float {ratio.Value}", $"Ratio clamped within range [{ratio.Min}, {ratio.Min}]");
        }
    }
}
