using System;
using UnityEngine;
using PQ.Common;
using PQ.Entities;


namespace PQ.TestScenes.Minimal
{
    public class RectMovementController : MonoBehaviour
    {
        private Collisions2D _collisionChecker;
        private PhysicsBody2D _physicsBody2D;

        public event Action<bool> GroundContactChanged;
        public Character2DSettings Settings { get; set; }

        public Vector2 Position => _physicsBody2D.Position;
        public void PlaceAt(Vector2 position, float rotation)
        {
            _physicsBody2D.MoveTo(position);
            _physicsBody2D.SetLocalOrientation3D(0, 0, rotation);
        }
        public void FaceRight() => _physicsBody2D.SetLocalOrientation3D(0, 0, 0);
        public void FaceLeft() => _physicsBody2D.SetLocalOrientation3D(0, 180, 0);
        public void MoveForward()
        {
            float distanceToMove = Settings.HorizontalMovementPeakSpeed * Time.fixedDeltaTime;
            _physicsBody2D.MoveBy(distanceToMove * _physicsBody2D.Forward);
        }
        public void Jump()
        {
            // todo: replace with actual jump code
        }

        void Awake()
        {
            _physicsBody2D    = gameObject.GetComponent<PhysicsBody2D>();
            _collisionChecker = gameObject.GetComponent<Collisions2D>();
        }

        void Start()
        {

        }

        void FixedUpdate()
        {

        }
    }
}
