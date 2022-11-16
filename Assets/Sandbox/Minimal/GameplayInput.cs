using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;


namespace PQ.TestScenes.Minimal
{
    public sealed class GameplayInput
    {
        private Vector2 _axis;
        public float Horizontal => _axis.x;
        public float Vertical   => _axis.y;

        private static readonly Key[] leftButtons  = { Key.LeftArrow,  Key.A };
        private static readonly Key[] rightButtons = { Key.RightArrow, Key.D };
        private static readonly Key[] downButtons  = { Key.DownArrow,  Key.S };
        private static readonly Key[] upButtons    = { Key.UpArrow,    Key.W };

        public GameplayInput()
        {
            _axis = Vector2.zero;
        }


        public void ReadInput()
        {
            Vector2 axis = new(
                x: ReadAxis(negativeKeys: leftButtons, positiveKeys: rightButtons),
                y: ReadAxis(negativeKeys: downButtons, positiveKeys: upButtons)
            );
            if (_axis != axis)
            {
                _axis = axis;
            }
        }

        // Return the scalar value (-1, 1) of the winning key(s), if any, otherwise 0
        private float ReadAxis(Key[] negativeKeys, Key[] positiveKeys)
        {
            var negativePressedCount = negativeKeys.Count(key => Keyboard.current[key].isPressed);
            var positivePressedCount = positiveKeys.Count(key => Keyboard.current[key].isPressed);
            return positivePressedCount.CompareTo(negativePressedCount);
        }
    }
}
