using System;
using UnityEngine;
using PQ.Common.Tuning;


namespace PQ.Game.Entities.Penguin
{
    /*
    Collection of penguin specific tuning data.
    */
    // todo: figure out how to make this work automatically with inheritance
    [CreateAssetMenu(fileName = "PenguinTuningConfig", menuName = "TuningConfigs/PenguinTuningConfig", order = 1)]
    public sealed class PenguinTuningConfig : TuningConfig
    {
        [Header("General")]
        [Tooltip("Terminal speed when falling due to acceleration of gravity")]
        [SerializeField][Range(0, 100)] public float maxVerticalSpeedFalling = 5f;

        [Tooltip("Terminal speed when falling")]
        [SerializeField][Range(0, 100)] public float maxVerticalSpeedJumping = 5f;

        // todo: add jump, sliding 'launch' thresholds, etc


        [Header("Upright")]

        [Tooltip("Terminal speed when walking")]
        [SerializeField][Range(0, 100)] public float maxHorizontalSpeedUpright = 5f;

        [Tooltip("Maximum permissible overlap with other colliders")]
        [SerializeField][Range(0, 1)] public float overlapToleranceUpright = 0.04f;

        [Tooltip("Minimum for bounding box, with coordinates relative to rigidbody position")]
        [SerializeField] public Vector2 boundsMinUpright = new Vector2(-0.25f, 0.00f);

        [Tooltip("Maximum for bounding box, with coordinates relative to rigidbody position")]
        [SerializeField] public Vector2 boundsMaxUpright = new Vector2(0.25f, 1.00f);


        [Header("Prone")]

        [Tooltip("Terminal speed when sliding")]
        [SerializeField][Range(0, 100)] public float maxHorizontalSpeedProne = 20f;

        [Tooltip("Maximum permissible overlap with other colliders")]
        [SerializeField][Range(0, 1)] public float overlapToleranceProne = 0.04f;

        [Tooltip("Minimum for bounding box, with coordinates relative to rigidbody position")]
        [SerializeField] public Vector2 boundsMinProne = new Vector2(-0.50f, 0.00f);

        [Tooltip("Maximum for bounding box, with coordinates relative to rigidbody position")]
        [SerializeField] public Vector2 boundsMaxProne = new Vector2(0.50f, 0.50f);
    }
}
