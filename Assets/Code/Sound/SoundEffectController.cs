using UnityEngine;


namespace PQ.Sound
{
    public class SoundEffectController : MonoBehaviour
    {
        private AudioSource audioSource;
        private static float DEFAULT_MASTER_VOLUME = 0.5f;

        [SerializeField] private float volumeScalePlayerScored;
        [SerializeField] private float volumeScalePlayerHit;
        [SerializeField] private float volumeScaleEnemyHit;
        [SerializeField] private float volumeScaleGameOver;
        
        [SerializeField] private AudioClip playerHit;
        [SerializeField] private AudioClip playerScored;
        [SerializeField] private AudioClip enemyHit;
        [SerializeField] private AudioClip playerWin;
        [SerializeField] private AudioClip playerLose;

        void Awake()
        {
            audioSource = transform.gameObject.GetComponent<AudioSource>();
            audioSource.loop        = false;
            audioSource.playOnAwake = false;
            audioSource.volume      = DEFAULT_MASTER_VOLUME;
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
            audioSource.Pause();
        }
        private void ResumeAnyActiveSoundEffects(string _)
        {
            audioSource.UnPause();
        }
        private void SetMasterVolume(PlayerSettingsInfo gameSettings)
        {
            audioSource.volume = gameSettings.SoundVolume / 100.0f;
        }

        private void PlaySoundOnPlayerScored(PlayerProgressionInfo playerInfo)
        {
            audioSource.PlayOneShot(playerScored, volumeScalePlayerScored);
        }

        private void PlaySoundOnEnemyHit(string _)
        {
            audioSource.PlayOneShot(playerHit, volumeScalePlayerHit);
        }

        private void PlaySoundOnEnemyKilled(int numPoints)
        {
            audioSource.PlayOneShot(enemyHit, volumeScaleEnemyHit);
        }

        private void PlaySoundOnGameOver(PlayerProgressionInfo playerInfo)
        {
            bool placeHolderVictoryCondition = false;
            if (placeHolderVictoryCondition)
            {
                audioSource.PlayOneShot(playerWin, volumeScaleGameOver);
            }
            else
            {
                audioSource.PlayOneShot(playerLose, volumeScaleGameOver);
            }
        }
    }
}
