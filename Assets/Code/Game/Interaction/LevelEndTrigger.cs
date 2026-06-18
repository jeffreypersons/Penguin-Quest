using UnityEngine;


namespace PQ.Game.Interaction
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class LevelEndTrigger : MonoBehaviour
    {
        private bool _triggered;
        private BoxCollider2D _collider;
        private ContactFilter2D _filter;
        private Collider2D[] _results;

        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("Triggers");
            _collider = GetComponent<BoxCollider2D>();
            _results = new Collider2D[4];
            _filter = new ContactFilter2D();
            _filter.useTriggers = false;
            _filter.SetLayerMask(LayerMask.GetMask("Actors"));
        }

        private void FixedUpdate()
        {
            if (_triggered)
            {
                return;
            }

            int count = _collider.Overlap(_filter, _results);
            for (int i = 0; i < count; i++)
            {
                if (_results[i].CompareTag("Player"))
                {
                    _triggered = true;
                    Debug.Log("LevelEndTrigger: Player entered, raising endGame");
                    GameEventCenter.Instance.endGame.Raise();
                    return;
                }
            }
        }
    }
}
