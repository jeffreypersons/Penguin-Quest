using System;
using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    public class PenguinStateOnBelly : FsmState
    {
        private PenguinBlob _blob;
        public PenguinStateOnBelly(string name, PenguinBlob blob) : base(name) { _blob = blob; }


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
