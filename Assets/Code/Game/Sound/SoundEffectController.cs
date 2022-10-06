using UnityEngine;
using PQ.Common.Events;


namespace PQ.Sound
{
    public class SoundEffectController : MonoBehaviour
    {
        private static float DefaultMasterVolume = 0.5f;

        [SerializeField] private float _volumeScalePlayerScored;
        [SerializeField] private float _volumeScalePlayerHit;
        [SerializeField] private float _volumeScaleEnemyHit;
        [SerializeField] private float _volumeScaleGameOver;
        
        [SerializeField] private AudioClip _playerHit;
        [SerializeField] private AudioClip _playerScored;
        [SerializeField] private AudioClip _enemyHit;
        [SerializeField] private AudioClip _playerWin;
        [SerializeField] private AudioClip _playerLose;

        private AudioSource _audioSource;
        private GameEventCenter _eventCenter;
        private PqEventRegistry _soundEffectEventRegistry;

        void Awake()
        {
            _eventCenter = GameEventCenter.Instance;

            _audioSource = transform.gameObject.GetComponent<AudioSource>();
            _audioSource.loop        = false;
            _audioSource.playOnAwake = false;
            _audioSource.volume      = DefaultMasterVolume;

            _soundEffectEventRegistry = new PqEventRegistry();
            _soundEffectEventRegistry.Add(_eventCenter.scoreChange,  PlaySoundOnPlayerScored);
            _soundEffectEventRegistry.Add(_eventCenter.startNewGame, SetMasterVolume);
            _soundEffectEventRegistry.Add(_eventCenter.pauseGame,    PauseAnyActiveSoundEffects);
            _soundEffectEventRegistry.Add(_eventCenter.resumeGame,   ResumeAnyActiveSoundEffects);
            _soundEffectEventRegistry.Add(_eventCenter.gameOver,     PlaySoundOnGameOver);
        }

        void OnEnable()
        {
            _soundEffectEventRegistry.SubscribeToAllRegisteredEvents();
        }
        void OnDisable()
        {
            _soundEffectEventRegistry.UnsubscribeToAllRegisteredEvents();
        }


        private void PauseAnyActiveSoundEffects(PlayerProgressionInfo _)
        {
            _audioSource.Pause();
        }
        private void ResumeAnyActiveSoundEffects()
        {
            _audioSource.UnPause();
        }
        private void SetMasterVolume(PlayerSettingsInfo gameSettings)
        {
            _audioSource.volume = gameSettings.SoundVolume / 100.0f;
        }

        private void PlaySoundOnPlayerScored(PlayerProgressionInfo playerInfo)
        {
            _audioSource.PlayOneShot(_playerScored, _volumeScalePlayerScored);
        }

        private void PlaySoundOnGameOver(PlayerProgressionInfo playerInfo)
        {
            bool placeHolderVictoryCondition = false;
            if (placeHolderVictoryCondition)
            {
                _audioSource.PlayOneShot(_playerWin, _volumeScaleGameOver);
            }
            else
            {
                _audioSource.PlayOneShot(_playerLose, _volumeScaleGameOver);
            }
        }
    }
}
