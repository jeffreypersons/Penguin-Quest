using System;
using System.Collections.Generic;
using UnityEngine;


namespace PQ._Experimental.Physics.Contact_006
{
    public class Controller : MonoBehaviour
    {
        [SerializeField] private Body _body;
        [SerializeField] [Range(0f, 1f)] private float _contactOffset = 0.05f;

        private IReadOnlyList<Body.ContactSlotInfo> _contactInfos;

        void Awake()
        {
            Application.targetFrameRate = 60;
            _body = new Body(transform);
            _contactInfos = default;
        }

        void FixedUpdate()
        {
            _body.UpdateContactInfo(_contactOffset);
            _contactInfos = _body.GetContactInfo();
        }

        void OnDrawGizmos()
        {
            if (Application.IsPlaying(this))
            {
                GizmoExtensions.DrawText(_body.Position, $"***** Contact Info *****\n{string.Join("\n  ", _contactInfos)}");
            }
        }
    }
}
