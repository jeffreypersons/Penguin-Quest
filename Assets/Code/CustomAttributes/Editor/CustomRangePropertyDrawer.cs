using UnityEngine;
using UnityEditor;
using CustomAttributes;


[CustomPropertyDrawer(typeof(CustomRangeAttribute))]
public class CustomRangePropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

    }
}
