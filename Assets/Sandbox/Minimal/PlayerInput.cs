using System.Linq;
using UnityEngine.InputSystem;


namespace PQ.TestScenes.Minimal
{
    public class PlayerInput
    {
        public float Horizontal { get; private set; }
        public float Vertical   { get; private set; }

        private static readonly Key[] leftButtons  = { Key.LeftArrow,  Key.A };
        private static readonly Key[] rightButtons = { Key.RightArrow, Key.D };
        private static readonly Key[] downButtons  = { Key.DownArrow,  Key.S };
        private static readonly Key[] upButtons    = { Key.UpArrow,    Key.W };

        private bool WasPressedThisFrame(Key key) => Keyboard.current[key].wasPressedThisFrame;

        public PlayerInput()
        {
            Horizontal = 0f;
            Vertical   = 0f;
        }


        public void Update()
        {
            Horizontal = ReadAxis(negativeKeys: leftButtons, positiveKeys: rightButtons);
            Vertical   = ReadAxis(negativeKeys: downButtons, positiveKeys: upButtons);
        }

        // Return the scalar value (-1, 1) of the winning key(s), if any, otherwise 0
        private float ReadAxis(Key[] negativeKeys, Key[] positiveKeys)
        {
            var negativePressedCount = negativeKeys.Count(WasPressedThisFrame);
            var positivePressedCount = positiveKeys.Count(WasPressedThisFrame);
            return negativePressedCount.CompareTo(positivePressedCount);
        }
    }
}
