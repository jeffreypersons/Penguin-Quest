// note: deliberately kept OUTSIDE the editor folder, where implementations can be found,
//       so just think of this file as our `api` to our custom built attributes
using UnityEngine;


namespace CustomAttributes
{
// example usage:  [ReadOnlyFieldAttribute] [SerializeField] private string myStatus;
// implementation: `Editor/ReadOnlyPropertyDrawer`
//
// Summary
// * Allows fields to be disabled in inspector with editing of the value disabled,
//   providing an in-editor way of showing useful data without resorting to debug or print statements
public class ReadOnlyFieldAttribute : PropertyAttribute
{
}
}
