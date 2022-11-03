using UnityEngine;


namespace PQ.TestScenes.Minimal
{
    public class CharacterEntity : MonoBehaviour
    {
        [SerializeField] [Range(0, 1000f)] private float _horizontalSpeed;

        private GameplayInput _input;
        private ICharacterController2D _mover;

        private void Awake()
        {
            _input = new();
            _mover = new SimpleCharacterController2D(gameObject);
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            _input.ReadInput();
        }


        private void FixedUpdate()
        {
            _mover.Move(
                deltaX: _input.Horizontal * _horizontalSpeed * Time.fixedDeltaTime,
                deltaY: 0
            );
        }
    }
}
