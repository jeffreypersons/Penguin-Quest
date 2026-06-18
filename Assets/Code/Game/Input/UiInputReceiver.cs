using UnityEngine;
using UnityEngine.InputSystem;
using PQ.Game.Input.Generated;


namespace PQ.Game.Input
{
    /*
    Input receiver that maps UI control input to the game event system.

    Mirrors GameplayInputReceiver but for UI-context inputs (splash screens, menus, etc).
    Uses the UI action map from the generated player controls.
    */
    [System.Serializable]
    [AddComponentMenu("UiInputReceiver")]
    public class UiInputReceiver : MonoBehaviour
    {
        private UnityPlayerControls _generatedPlayerControls;
        private GameEventCenter _eventCenter;

        private InputAction _submit;

        void Awake()
        {
            _eventCenter = GameEventCenter.Instance;

            UnityPlayerControls controls = new UnityPlayerControls();

            _generatedPlayerControls = controls;
            _submit = controls.UI.Submit;
        }

        void OnEnable()
        {
            _generatedPlayerControls.UI.Enable();
            _submit.performed += OnSubmit;
        }

        void OnDisable()
        {
            _generatedPlayerControls.UI.Disable();
            _submit.performed -= OnSubmit;
        }

        private void OnSubmit(InputAction.CallbackContext _) => _eventCenter.anyKeyPressed.Raise();
    }
}
