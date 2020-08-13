﻿// note: deliberately kept OUTSIDE the editor folder, where implementations can be found,
//       so just think of this file as our `api` to our custom built attributes
using UnityEngine;


// example usage:
//     [TagSelector] [SerializeField] private string[] tagsOfButtonsToHideOnMenuOpen = new string[] { };
//
// Notes
// * see `Editor/TagSelectorPropertyDrawer` for implementation
// * unlike the built in default tag selector in which a custom editor has to be written and the field is then
// tagged like `EditorGUI.TagField(position, label, property.stringValue)`, this attribute works 'out of the box'
public class TagSelectorAttribute : PropertyAttribute
{
}
