using UnityEngine;


namespace PQ
{
    /*
    Handles game level logic/event handling.
    
    Notes
    - relies on startNewGameEvent being triggered (from main menu), so if running in editor
    - expect null parameters, and just run from main menu instead
    */
    public class GameController : MonoBehaviour
    {
        private GameEventCenter _gameEventCenter;
        private PlayerProgressionInfo _playerInfo;

        // todo: use spawner
        [SerializeField] private GameObject _playerPenguin;

        void Awake()
        {
            _gameEventCenter = GameEventCenter.Instance;
            _playerPenguin.SetActive(true);
            _gameEventCenter.startNewGame.AddAutoUnsubscribeListener(StartNewGame);
        }

        void OnEnable()
        {
            _gameEventCenter.enemyKilled.AddListener(UpdateScore);
            _gameEventCenter.restartGame.AddListener(RestartGame);
        }
        void OnDisable()
        {
            _gameEventCenter.enemyKilled.RemoveListener(UpdateScore);
            _gameEventCenter.restartGame.RemoveListener(RestartGame);
        }


        private void StartNewGame(PlayerSettingsInfo gameSettings)
        {
            _playerInfo = new PlayerProgressionInfo(gameSettings.NumberOfLives);
            _gameEventCenter.scoreChange.Trigger(_playerInfo);
        }

        private void RestartGame(string status)
        {
            ResetMovingObjects();
            _playerInfo = new PlayerProgressionInfo(_playerInfo.Lives);
            _gameEventCenter.scoreChange.Trigger(_playerInfo);
        }

        private void UpdateScore(int points)
        {
            if (_playerInfo == null)
            {
                Debug.LogError($"RecordedScore that is set upon starting a new game {GetType().Name} is missing, " +
                               $"perhaps the event wasn't fired or listened to? " +
                               $"...If running from game scene in play mode, try starting from main menu instead");
            }

            _playerInfo.AddToScore(points);
            _gameEventCenter.scoreChange.Trigger(_playerInfo);
            if (LoseConditionMet() || WinConditionMet())
            {
                _gameEventCenter.gameOver.Trigger(_playerInfo);
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
            // no op placeholder
        }
    }
}
