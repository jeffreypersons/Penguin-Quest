using System;
using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    public class PenguinStateOnBelly : FsmState<PenguinFsmParams>
    {
        private PenguinBlob _blob;
        private PenguinFsmParams _fsmParams;
        public PenguinStateOnBelly(string name, PenguinBlob blob, PenguinFsmParams fsmParams)
            : base(name) { _blob = blob; _fsmParams = fsmParams; }

        public override void Enter()
        {
            _blob.CharacterController.Settings = _blob.OnBellySettings;
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
