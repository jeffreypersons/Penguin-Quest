using UnityEngine;
using PQ.Common.Extensions;
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
        [SerializeField] private GameObject            _playerPenguin;
        [SerializeField] private GameplayInputReceiver _gameplayInputController;
        [SerializeField] private CameraController      _cameraController;
        [SerializeField] private SoundEffectController _soundEffectController;
        [SerializeField] private SoundTrackController  _soundTrackController;

        private GameEventCenter _gameEventCenter;

        private CharacterStatus _characterStatus;


        void Awake()
        {
            _characterStatus = new CharacterStatus(
                lives:   1,
                stamina: 1.0f,
                health:  1.0f
             );

            _gameEventCenter = GameEventCenter.Instance;
            _playerPenguin.SetActive(true);
            _playerPenguin.GetComponent<Entities.Penguin.PenguinEntity>().EventBus = _gameEventCenter;

            _cameraController.FollowTarget = _playerPenguin.transform;
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
