using System.Diagnostics.Contracts;
using UnityEngine;
using PQ.Common;
using PQ.Common.Collisions;


namespace PQ.Entities.Placeholder
{
    public class RectMovement
    {
        public Vector2 Up       => _transform.up.normalized;
        public Vector2 Forward  => _transform.forward.normalized;
        public Vector2 Position => _rigidBody.position;
        public float   Rotation => _transform.eulerAngles.z;

        public float GroundDistanceThreshold { get; set; }
        public bool  MaintainGroundAlignment { get; set; }
        public float HorizontalSpeed         { get; set; }
        public float AngularSpeed            { get; set; }
        public float DistanceThreshold       { get; set; }
        public float AngularThreshold        { get; set; }

        private Transform     _transform;
        private Rigidbody2D   _rigidBody;
        private BoxCollider2D _boundingBox;
        private LineCaster    _caster;

        private static readonly Quaternion RightFacing = Quaternion.Euler(0,   0, 0);
        private static readonly Quaternion LeftFacing  = Quaternion.Euler(0, 180, 0);

        public RectMovement(Transform transform, RayCasterSettings casterSettings)
        {
            _transform   = transform;
            _rigidBody   = transform.GetComponent<Rigidbody2D>();
            _boundingBox = transform.GetComponent<BoxCollider2D>();
            _caster      = new LineCaster(casterSettings);

            MaintainGroundAlignment = false;
            HorizontalSpeed         = 10.0f;
            AngularSpeed            = 10.0f;
            DistanceThreshold       =  5.0f;
            AngularThreshold        =  5.0f;
            GroundDistanceThreshold =  0.3f;
        }

        public void PlaceAt(Vector2 position, float angle) =>
            _transform.SetPositionAndRotation(position, Quaternion.Euler(0, 0, angle));

        public void MoveForwardForTime(float time) =>
            MoveTowards(ComputeTargetPosition(Position, Forward, HorizontalSpeed, time), HorizontalSpeed, time);

        public void MoveTowardsPoint(Vector2 target, float time) =>
            MoveTowards(target, HorizontalSpeed, time);

        public void FaceRight() => _transform.localRotation = RightFacing;
        public void FaceLeft()  => _transform.localRotation = LeftFacing;


        private void MoveTowards(Vector2 target, float speed, float time)
        {
            Vector2 origin    = Position;
            Vector2 direction = Forward;
            float   distance  = speed * time;

            _transform.localRotation = IsTargetBehind(origin, direction, target) ?
                LeftFacing : RightFacing;


            Vector2 newPosition = Vector2.MoveTowards(origin, target, distance);
            if (ArePointsWithinDistance(origin, newPosition, DistanceThreshold))
            {
                return;
            }

            if (_rigidBody == null)
            {
                _transform.position = newPosition;
            }
            else
            {
                _rigidBody.MovePosition(newPosition);
            }
        }

        
        [Pure] private static bool IsTargetBehind(Vector2 origin, Vector2 direction, Vector2 target) =>
            Vector2.Dot(direction, target - origin) < 0;

        [Pure] private static Vector2 ComputeTargetPosition(Vector2 position, Vector2 direction, float speed, float time) =>
            position + ((speed * time) * direction);

        [Pure] private static bool ArePointsWithinDistance(Vector2 pointA, Vector2 pointB, float distance) =>
            (pointB - pointA).sqrMagnitude <= distance * distance;
    }
}
