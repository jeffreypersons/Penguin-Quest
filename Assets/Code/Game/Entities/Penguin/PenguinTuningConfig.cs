using System;
using UnityEngine;
using PQ.Common.Tuning;


namespace PQ.Game.Entities.Penguin
{
    /*
    Collection of penguin specific tuning data.
    */
    public sealed class PenguinTuningConfig : TuningConfig
    {
        [Header("Upright")]

        [SerializeField][Range(0, 100)] public float maxHorizontalSpeedUpright = 5f;

        [Tooltip("Minimum for bounding box, with coordinates relative to rigidbody position")]
        [SerializeField] public Vector2 boundsMinUpright = new Vector2(-0.25f, 0.00f);

        [Tooltip("Maximum for bounding box, with coordinates relative to rigidbody position")]
        [SerializeField] public Vector2 boundsMaxUpright = new Vector2(0.25f, 1.00f);

        [Tooltip("Maximum permissible overlap with other colliders")]
        [SerializeField][Range(0, 1)] public float overlapToleranceUpright = 0.075f;


        [Header("Prone")]

        [SerializeField][Range(0, 100)] public float maxHorizontalSpeedProne = 20f;

        [Tooltip("Minimum for bounding box, with coordinates relative to rigidbody position")]
        [SerializeField] public Vector2 boundsMinProne = new Vector2(-0.50f, 0.00f);

        [Tooltip("Maximum for bounding box, with coordinates relative to rigidbody position")]
        [SerializeField] public Vector2 boundsMaxProne = new Vector2(0.50f, 0.50f);

        [Tooltip("Maximum permissible overlap with other colliders")]
        [SerializeField][Range(0, 1)] public float overlapToleranceProne = 0.075f;
    }
}
