using UnityEngine;


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

        void Awake()
        {
            _eventCenter = GameEventCenter.Instance;

            _audioSource = transform.gameObject.GetComponent<AudioSource>();
            _audioSource.loop        = false;
            _audioSource.playOnAwake = false;
            _audioSource.volume      = DefaultMasterVolume;
        }

        void OnEnable()
        {
            _eventCenter.scoreChange .AddListener(PlaySoundOnPlayerScored);
            _eventCenter.startNewGame.AddListener(SetMasterVolume);
            _eventCenter.pauseGame   .AddListener(PauseAnyActiveSoundEffects);
            _eventCenter.resumeGame  .AddListener(ResumeAnyActiveSoundEffects);
            _eventCenter.gameOver    .AddListener(PlaySoundOnGameOver);
        }
        void OnDisable()
        {
            _eventCenter.scoreChange .RemoveListener(PlaySoundOnPlayerScored);            
            _eventCenter.startNewGame.RemoveListener(SetMasterVolume);
            _eventCenter.pauseGame   .RemoveListener(PauseAnyActiveSoundEffects);
            _eventCenter.resumeGame  .RemoveListener(ResumeAnyActiveSoundEffects);
            _eventCenter.gameOver    .RemoveListener(PlaySoundOnGameOver);
        }

        private void PauseAnyActiveSoundEffects(PlayerProgressionInfo _)
        {
            _audioSource.Pause();
        }
        private void ResumeAnyActiveSoundEffects(string _)
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

        private void PlaySoundOnEnemyHit(string _)
        {
            _audioSource.PlayOneShot(_playerHit, _volumeScalePlayerHit);
        }

        private void PlaySoundOnEnemyKilled(int numPoints)
        {
            _audioSource.PlayOneShot(_enemyHit, _volumeScaleEnemyHit);
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
