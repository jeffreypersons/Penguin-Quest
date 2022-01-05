﻿using UnityEngine;
using PenguinQuest.Data;


namespace PenguinQuest.Controllers
{
    /*
    Handles game level logic/event handling.
    
    Notes
    - relies on startNewGameEvent being triggered (from main menu), so if running in editor
    - expect null parameters, and just run from main menu instead
    */
    public class GameController : MonoBehaviour
    {
        private PlayerStatsInfo playerInfo;

        // todo: replace with MoveController interfaces, and use like `MoveController.Reset()`
        [SerializeField] private GameObject playerPenguin = default;

        void Awake()
        {
            GameEventCenter.startNewGame.AddAutoUnsubscribeListener(StartNewGame);
        }

        void OnEnable()
        {
            GameEventCenter.enemyKilled.AddListener(UpdateScore);
            GameEventCenter.restartGame.AddListener(RestartGame);
        }
        void OnDisable()
        {
            GameEventCenter.enemyKilled.RemoveListener(UpdateScore);
            GameEventCenter.restartGame.RemoveListener(RestartGame);
        }

        private void StartNewGame(GameSettingsInfo gameSettings)
        {
            playerInfo = new PlayerStatsInfo(gameSettings.NumberOfLives);
            //enemy.foreach().GetComponent<AiController>().SetDifficultyLevel(gameSettings.DifficultyLevel);
            GameEventCenter.scoreChange.Trigger(playerInfo);
        }
        private void RestartGame(string status)
        {
            ResetMovingObjects();
            playerInfo = new PlayerStatsInfo(playerInfo.Lives);
            GameEventCenter.scoreChange.Trigger(playerInfo);
        }
        private void UpdateScore(int points)
        {
            if (playerInfo == null)
            {
                Debug.LogError($"RecordedScore that is set upon starting a new game {GetType().Name} is missing, " +
                               $"perhaps the event wasn't fired or listened to? " +
                               $"...If running from game scene in play mode, try starting from main menu instead");
            }

            playerInfo.AddToScore(points);
            GameEventCenter.scoreChange.Trigger(playerInfo);
            if (LoseConditionMet() || WinConditionMet())
            {
                GameEventCenter.gameOver.Trigger(playerInfo);
            }
        }

        // placeholders for gameover conditions (will be based on level progression, score etc in future)
        private bool LoseConditionMet()
        {
            return false;
        }
        private bool WinConditionMet()
        {
            return false;
        }

        private void ResetMovingObjects()
        {

        }
    }
}