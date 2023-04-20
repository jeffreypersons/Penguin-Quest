using UnityEngine;


namespace PQ.TestScenes.Box
{
    public sealed class Mover
    {
        private Rigidbody2D _body;
        private Collider2D  _collider;

        public override string ToString() =>
            $"Mover{{" +
                $"stub:{true}" +
            $"}}";

        public Mover(Transform transform)
        {
            _body     = transform.GetComponent<Rigidbody2D>();
            _collider = transform.GetComponent<Collider2D>();
        }
        
        public void Flip()
        {

        }

        public void Move(Vector2 deltaPosition)
        {
            // no op
        }
    }
}
