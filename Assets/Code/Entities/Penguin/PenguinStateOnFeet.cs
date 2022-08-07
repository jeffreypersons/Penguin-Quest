using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    public class PenguinStateOnFeet : FsmState<PenguinFsmParams>
    {
        private PenguinBlob _blob;
        private PenguinFsmParams _fsmParams;
        public PenguinStateOnFeet(string name, PenguinBlob blob, PenguinFsmParams fsmParams)
            : base(name) { _blob = blob; _fsmParams = fsmParams; }

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
