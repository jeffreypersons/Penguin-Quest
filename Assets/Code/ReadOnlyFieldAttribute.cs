// note: deliberately kept OUTSIDE the editor folder, where implementations can be found,
//       so just think of this file as our `api` to our custom built attributes
using UnityEngine;


// example usage:
//     [ReadOnlyFieldAttribute] [SerializeField] private string myStatus;
//
// note: see `Editor/TagSelectorPropertyDrawer` for implementation
public class ReadOnlyFieldAttribute : PropertyAttribute
{
}
