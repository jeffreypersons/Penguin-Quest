using System;
using System.Text;
using UnityEngine;


namespace PQ._Experimental.Physics.Contact_006
{
    public class Controller : MonoBehaviour
    {
        [SerializeField] private Body _body;
        [SerializeField] [Range(0f, 1f)] private float _contactOffset = 0.05f;

        private StringBuilder summary;

        void Awake()
        {
            Application.targetFrameRate = 60;
            _body = new Body(transform);
            summary = new StringBuilder();
        }

        void FixedUpdate()
        {
            _body.FireAllContactSensors(_contactOffset, out var slots);

            summary = summary.Clear();
            summary.AppendLine("***** Contact Info *****");
            foreach (var slot in slots)
            {
                summary.AppendLine($"{slot}");
            }
        }

        void OnDrawGizmos()
        {
            if (Application.IsPlaying(this))
            {
                GizmoExtensions.DrawText(_body.Position, summary.ToString());
            }
        }
    }
}
