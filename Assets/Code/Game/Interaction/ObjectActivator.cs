using UnityEngine;


public class ObjectActivator : MonoBehaviour
{
    [SerializeField] string activatorTag = null;
    [SerializeField] bool deactivateOnExit = false;
    [SerializeField] GameObject[] objects = null;

    private BoxCollider2D _collider;
    private ContactFilter2D _filter;
    private Collider2D[] _results;
    private bool _activated;

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
        int count = _collider != null ? _collider.Overlap(_filter, _results) : 0;
        bool hasMatch = false;

        for (int i = 0; i < count; i++)
        {
            if (_results[i].CompareTag(activatorTag))
            {
                hasMatch = true;
                break;
            }
        }

        if (hasMatch && !_activated)
        {
            _activated = true;
            foreach (var obj in objects)
            {
                if (obj != null) obj.SetActive(true);
            }
        }
        else if (!hasMatch && _activated && deactivateOnExit)
        {
            _activated = false;
            foreach (var obj in objects)
            {
                if (obj != null) obj.SetActive(false);
            }
        }
    }
}
