using UnityEngine;
using UnityEditor;
using System;


[CustomPropertyDrawer(typeof(TagSelectorAttribute))]
public class TagSelectorPropertyDrawer : PropertyDrawer
{
    internal static class Tags
    {
        public static string[] DisplayNames { get; private set; }
        public const string EMPTY_TAG = "";
        public const string EMPTY_TAG_DISPLAY_NAME = "<No Tag>";

        static Tags()
        {
            Update();
        }
        public static string Select(int index)
        {
            return index <= 0 ? EMPTY_TAG : DisplayNames[index];
        }
        public static int IndexOf(string tag)
        {
            return tag == EMPTY_TAG ? 0 : Array.IndexOf(DisplayNames, tag, 1);
        }
        public static void Update()
        {
            var newTags = UnityEditorInternal.InternalEditorUtility.tags;
            if (!CollectionUtils.AreArraySegmentsEqual(newTags, DisplayNames, start1: 0, start2: 1))
            {
                DisplayNames = CollectionUtils.PrependToArray(EMPTY_TAG_DISPLAY_NAME, newTags);
            }
        }
    }

    // get the latest tags from editor and display them in a dropdown with our drawer getting the selected tag
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        using (var scope = new EditorGUI.PropertyScope(position, label, property))
        {
            Tags.Update();
            property.stringValue = Tags.Select(
                EditorGUI.Popup(position, label.text, Tags.IndexOf(property.stringValue), Tags.DisplayNames));
        }
    }
}
