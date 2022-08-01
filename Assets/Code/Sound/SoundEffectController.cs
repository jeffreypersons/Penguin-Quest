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

        void Awake()
        {
            _audioSource = transform.gameObject.GetComponent<AudioSource>();
            _audioSource.loop        = false;
            _audioSource.playOnAwake = false;
            _audioSource.volume      = DefaultMasterVolume;
        }

        void OnEnable()
        {
            GameEventCenter.scoreChange .AddListener(PlaySoundOnPlayerScored);
            GameEventCenter.enemyHit    .AddListener(PlaySoundOnEnemyHit);
            GameEventCenter.enemyKilled .AddListener(PlaySoundOnEnemyKilled);

            GameEventCenter.startNewGame.AddListener(SetMasterVolume);
            GameEventCenter.pauseGame   .AddListener(PauseAnyActiveSoundEffects);
            GameEventCenter.resumeGame  .AddListener(ResumeAnyActiveSoundEffects);
            GameEventCenter.gameOver    .AddListener(PlaySoundOnGameOver);
        }
        void OnDisable()
        {
            GameEventCenter.scoreChange .RemoveListener(PlaySoundOnPlayerScored);
            GameEventCenter.enemyHit    .RemoveListener(PlaySoundOnEnemyHit);
            GameEventCenter.enemyKilled .RemoveListener(PlaySoundOnEnemyKilled);

            GameEventCenter.startNewGame.RemoveListener(SetMasterVolume);
            GameEventCenter.pauseGame   .RemoveListener(PauseAnyActiveSoundEffects);
            GameEventCenter.resumeGame  .RemoveListener(ResumeAnyActiveSoundEffects);
            GameEventCenter.gameOver    .RemoveListener(PlaySoundOnGameOver);
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
