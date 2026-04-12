using UnityEngine;
using PQ.Common.Extensions;
using PQ.Common.Physics;
using PQ.Common.Spawning;
using PQ.Game.Entities;
using PQ.Game.Sound;
using PQ.Game.Input;
using PQ.Game.Camera;


// todo: replace config settings with scriptable objects
// todo: replace this placeholder with a game state machine
// todo: separate out blobs from fsm data and game blob and player blob and penguin blob
namespace PQ.Game
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private GameObject            _playerPrefab;
        [SerializeField] private GameplayInputReceiver _gameplayInputController;
        [SerializeField] private CameraController      _cameraController;
        [SerializeField] private SoundEffectController _soundEffectController;
        [SerializeField] private SoundTrackController  _soundTrackController;
        [SerializeField] private WeatherController     _weatherController;

        private GameEventCenter _gameEventCenter;

        private CharacterStatus _characterStatus;
        private SpawnSystem _spawnSystem;

        [SerializeField] private GameObject _playerInstance;

        void Awake()
        {
            if (_playerPrefab == null)
                throw new MissingReferenceException($"Cannot start game - player prefab not set in inspector");
            
            _characterStatus = new CharacterStatus(
                lives:   1,
                stamina: 1.0f,
                health:  1.0f
             );

            _spawnSystem = new SpawnSystem();
            _playerInstance = _spawnSystem.Spawn(prefab: _playerPrefab, tag: "SpawnPoint", new SpawnCollisionOptions(SnapDirection.Down));

            _gameEventCenter = GameEventCenter.Instance;
            _playerInstance.SetActive(true);
            _playerInstance.GetComponent<Entities.Penguin.PenguinEntity>().EventBus = _gameEventCenter;

            _cameraController.FollowTarget = _playerInstance.transform;
            _weatherController.FollowTarget = _playerInstance.transform;
        }

        void Start()
        {
            _soundTrackController.PlayTrack();
        }

        void Update()
        {
            // todo: replace these placeholders with our real state machine 
            if (WinConditionMet())
            {
                _gameEventCenter.levelWon.Raise();
            }
            else if (LoseConditionMet())
            {
                _gameEventCenter.levelLost.Raise();
            }
        }

        void OnEnable()
        {
            _gameEventCenter.startGame .AddHandler(RestartGame);
            _gameEventCenter.endGame   .AddHandler(ResumeGame);
            _gameEventCenter.pauseGame .AddHandler(PauseGame);
            _gameEventCenter.resumeGame.AddHandler(ResumeGame);
        }

        void OnDisable()
        {
            _gameEventCenter.startGame .RemoveHandler(RestartGame);
            _gameEventCenter.endGame   .RemoveHandler(ResumeGame);
            _gameEventCenter.pauseGame .RemoveHandler(PauseGame);
            _gameEventCenter.resumeGame.RemoveHandler(ResumeGame);
        }


        private void PauseGame()        => Time.timeScale = 0f;
        private void ResumeGame()       => Time.timeScale = 1f;
        private void RestartGame()      => SceneExtensions.LoadScene("Main");
        private bool WinConditionMet()  => false;
        private bool LoseConditionMet() => false;
    }
}
