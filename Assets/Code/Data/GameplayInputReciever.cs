using UnityEngine;
using UnityEngine.InputSystem;


// receives input for player
//
// assumes player controls is already setup
[System.Serializable]
[AddComponentMenu("GameplayInputReciever")]
public class GameplayInputReciever : MonoBehaviour
{
    private PlayerControls playerControls;
    public Vector2 Axes         { get; private set; }
    public bool IsFireRequested { get; private set; }
    public bool IsUseRequested  { get; private set; }

    [SerializeField] private PlayerInput playerInput;

    private void Init()
    {
        playerControls = new PlayerControls();
        //playerControls.controlSchemes;
        playerInput.defaultControlScheme = playerControls.KeyboardMouseScheme.name;
        playerInput.neverAutoSwitchControlSchemes = false;
    }

    void Awake()
    {
        Init();
    }

    void Update()
    {
        IsFireRequested = playerControls.Gameplay.Fire.triggered;
        IsUseRequested = playerControls.Gameplay.Use.triggered;
        Axes = playerControls.Gameplay.Move.ReadValue<Vector2>();
    }

    #if UNITY_EDITOR
    void OnValidate()
    {
        Init();
    }
    #endif
}
