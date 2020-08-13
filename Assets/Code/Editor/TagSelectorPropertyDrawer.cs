using UnityEngine;
using UnityEditor;
using System;


[CustomPropertyDrawer(typeof(TagSelectorAttribute))]
public class TagSelectorPropertyDrawer : PropertyDrawer
{
    internal static class Tags
    {
        private static string[] _cachedTags;
        public static string[] tags;
        public static GUIContent[] DisplayContents { get; private set; }
        public const string EMPTY_TAG = "";
        public const string EMPTY_TAG_DISPLAY_NAME = "<No Tag>";

        static Tags()
        {
            Update();
        }
        public static string Select(int index)
        {
            return index <= 0 ? EMPTY_TAG : _cachedTags[index];
        }
        public static int IndexOf(string tag)
        {
            return tag == EMPTY_TAG ? 0 : Array.IndexOf(_cachedTags, tag, 1);
        }
        public static void Update()
        {
            if (CollectionUtils.AreElementsEqual(_cachedTags, UnityEditorInternal.InternalEditorUtility.tags))
            {
                return;
            }
            _cachedTags = UnityEditorInternal.InternalEditorUtility.tags;
            tags = CollectionUtils.PrependToArray(EMPTY_TAG_DISPLAY_NAME, UnityEditorInternal.InternalEditorUtility.tags);
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
            Debug.Log(Tags.IndexOf(property.stringValue));
            property.stringValue = Tags.Select(
                EditorGUI.Popup(position, label.text, Tags.IndexOf(property.stringValue), Tags.tags));
        }
    }
}
