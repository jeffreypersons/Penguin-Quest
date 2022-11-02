using UnityEngine;


namespace PQ.TestScenes.Minimal
{
    public class CharacterEntity : MonoBehaviour
    {
        [SerializeField] private float _speed;

        private PlayerInput _input;

        private void Awake()
        {
            _input = new();
        }


        private void Update()
        {
            Vector2 a = _speed * _input.Horizontal * Vector2.right;
        }
    }
}
