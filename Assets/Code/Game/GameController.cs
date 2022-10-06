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

        [SerializeField] private GameObject _playerPenguin;

        void Awake()
        {
            _gameEventCenter = GameEventCenter.Instance;
            _playerPenguin.SetActive(true);
            _gameEventCenter.startNewGame.AddHandler(StartNewGame);

            _playerPenguin.GetComponent<Entities.Penguin.PenguinBlob>().EventBus = _gameEventCenter;
        }

        void OnEnable()
        {
            _gameEventCenter.startNewGame.AddHandler(StartNewGame);
            _gameEventCenter.restartGame.AddHandler(RestartGame);
        }
        void OnDisable()
        {
            _gameEventCenter.restartGame.RemoveHandler(RestartGame);
        }


        private void StartNewGame(PlayerSettingsInfo gameSettings)
        {
            _playerInfo = new PlayerProgressionInfo(gameSettings.NumberOfLives);
            _gameEventCenter.scoreChange.Raise(_playerInfo);
        }

        private void RestartGame()
        {
            ResetMovingObjects();
            _playerInfo = new PlayerProgressionInfo(_playerInfo.Lives);
            _gameEventCenter.scoreChange.Raise(_playerInfo);
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
