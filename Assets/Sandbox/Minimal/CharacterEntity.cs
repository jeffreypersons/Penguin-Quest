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
            if (Mathf.Approximately(_input.Horizontal, 0f))
            {
                return;
            }

            bool characterMovingLeft = _input.Horizontal < 0;
            bool characterFacingLeft = _mover.Flipped;
            if (characterFacingLeft != characterMovingLeft)
            {
                _mover.Flip();
            }

            float distance = Mathf.Abs(_input.Horizontal * _horizontalSpeed * Time.fixedDeltaTime);
            if (!Mathf.Approximately(distance, 0f))
            {
                _mover.Move(distance * _mover.Forward);
            }
        }
    }
}
