﻿using UnityEngine;
using PQ.Common;


namespace PQ.Entities.Penguin
{
    // todo: add some sort of free fall check that forces a respawn/death
    public class PenguinStateMidair : FsmState
    {
        private PenguinStateMachineDriver _driver;
        private PenguinBlob _blob;
        private GameEventCenter _eventCenter;

        public PenguinStateMidair(string name, PenguinStateMachineDriver driver,
            PenguinBlob blob, GameEventCenter eventCenter) : base(name)
        {
            _blob = blob;
            _driver = driver;
            _eventCenter = eventCenter;
        }

        protected override void OnIntialize()
        {

        }

        protected override void OnEnter()
        {
            _blob.CharacterController.GroundContactChanged.AddHandler(HandleGroundContactChanged);

            _blob.Animation.TriggerParamJumpUpParameter();
        }

        protected override void OnExit()
        {
            _blob.CharacterController.GroundContactChanged.RemoveHandler(HandleGroundContactChanged);

            // reset any triggers such that any pending animation events are cleared out to avoid them
            // from firing automatically on landing
            _blob.Animation.ResetAllTriggers();
        }


        private void HandleGroundContactChanged(Character2D.GroundContactInfo groundContactInfo)
        {
            _blob.Animation.SetParamIsGrounded(groundContactInfo.isGrounded);
            if (groundContactInfo.isGrounded)
            {
                _driver.MoveToState(_driver.PreviousState);
            }
        }
    }
}
