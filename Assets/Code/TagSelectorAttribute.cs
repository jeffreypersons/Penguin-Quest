// note: deliberately kept OUTSIDE the editor folder, where implementations can be found,
//       so just think of this file as our `api` to our custom built attributes
using UnityEngine;


// example usage:
//     [TagSelector] [SerializeField] private string[] tagsOfButtonsToHideOnMenuOpen = new string[] { };
//
// note: see `Editor/TagSelectorPropertyDrawer` for implementation
public class TagSelectorAttribute : PropertyAttribute
{
    public bool UseDefaultTagFieldDrawer = false;
}
