using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    public class PenguinStateOnFeet : FsmState
    {
        private PenguinBlob _blob;
        public PenguinStateOnFeet(string name, PenguinBlob blob): base(name) { _blob = blob; }

        public override void Enter()
        {
            _blob.CharacterController.Settings = _blob.OnFeetSettings;
        }

        public override void Exit()
        {

        }

        public override void FixedUpdate()
        {

        }

        public override void LateUpdate()
        {

        }

        public override void Update()
        {

        }
    }
}
