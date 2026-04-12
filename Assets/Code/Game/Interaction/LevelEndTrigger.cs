using UnityEngine;


namespace PQ.Game.Interaction
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class LevelEndTrigger : MonoBehaviour
    {
        private bool _triggered;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!_triggered && collision.CompareTag("Player"))
            {
                _triggered = true;
                GameEventCenter.Instance.endGame.Raise();
            }
        }
    }
}
