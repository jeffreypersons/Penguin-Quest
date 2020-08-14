using UnityEngine;
using UnityEditor;


[CustomPropertyDrawer(typeof(ReadOnlyFieldAttribute))]
public class ReadOnlyFieldPropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        using (var scope = new EditorGUI.DisabledGroupScope(true))
        {
            EditorGUI.PropertyField(position, property, label, includeChildren: true);
        }
    }
}
