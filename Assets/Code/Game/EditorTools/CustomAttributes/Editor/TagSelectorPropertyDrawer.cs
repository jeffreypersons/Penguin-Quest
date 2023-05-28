#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using CustomAttributes;


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
            UpdateDisplayedTagNames();
        }

        public static string Select(int index)
        {
            return index <= 0 ? EMPTY_TAG : DisplayNames[index];
        }

        public static int IndexOf(string tag)
        {
            return tag == EMPTY_TAG ? 0 : Array.IndexOf(DisplayNames, tag, 1);
        }

        public static void UpdateDisplayedTagNames()
        {
            var newTags = UnityEditorInternal.InternalEditorUtility.tags;

            if (DisplayNames == null || HasTagsChanged(newTags))
            {
                string[] newNames = new string[newTags.Length + 1];
                newNames[0] = EMPTY_TAG_DISPLAY_NAME;
                Array.Copy(newNames, 0, DisplayNames, 1, newNames.Length);
            }
        }

        private static bool HasTagsChanged(string[] tags)
        {
            if (DisplayNames.Length-1 != tags.Length)
            {
                return true;
            }
            for (int i = 1; i < DisplayNames.Length; i++)
            {
                if (DisplayNames[i] != tags[i-1])
                {
                    return true;
                }
            }
            return false;
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
            Tags.UpdateDisplayedTagNames();
            property.stringValue = Tags.Select(
                EditorGUI.Popup(position, label.text, Tags.IndexOf(property.stringValue), Tags.DisplayNames));
        }
    }
}
#endif