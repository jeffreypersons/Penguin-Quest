using UnityEngine;
using UnityEngine.InputSystem;


// helpers for wrapping the automatically generated player controls class, for fetching input
//
// todo: look into counting only first press
//
// see the following for ideas on the above:
// https://forum.unity.com/threads/new-input-system-how-to-use-the-hold-interaction.605587/page-2
[System.Serializable]
[AddComponentMenu("GameplayInputReciever")]
public class GameplayInputReciever : MonoBehaviour
{
    private InputAction fireAction;
    private InputAction useAction;
    private InputAction moveAction;
    private PlayerControls playerControls;
    public Vector2 Axes           { get; private set; }
    public bool FireHeldThisFrame { get; private set; }
    public bool UseHeldThisFrame  { get; private set; }
    public bool MoveHeldThisFrame { get; private set; }

    private void Init()
    {
        Axes = Vector2.zero;
        playerControls = new PlayerControls();
        fireAction = playerControls.Gameplay.Fire;
        useAction  = playerControls.Gameplay.Use;
        moveAction = playerControls.Gameplay.Move;
    }

    void OnEnable()
    {
        playerControls.Gameplay.Enable();
    }
    void OnDisable()
    {
        playerControls.Gameplay.Disable();
    }

    void Awake()
    {
        Init();
    }

    void Update()
    {
        Axes = moveAction.ReadValue<Vector2>();
        FireHeldThisFrame = fireAction.triggered;
        UseHeldThisFrame  = useAction.triggered;
        MoveHeldThisFrame = Axes != Vector2.zero;
    }
}
