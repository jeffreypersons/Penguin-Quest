using UnityEngine;
using PenguinQuest.Data;


namespace PenguinQuest.Controllers
{
    public class SoundEffectController : MonoBehaviour
    {
        private AudioSource audioSource;
        private static float DEFAULT_MASTER_VOLUME = 0.5f;

        [SerializeField] private float volumeScalePlayerScored = default;
        [SerializeField] private float volumeScalePlayerHit    = default;
        [SerializeField] private float volumeScaleEnemyHit     = default;
        [SerializeField] private float volumeScaleGameOver     = default;
        
        [SerializeField] private AudioClip playerHit    = default;
        [SerializeField] private AudioClip playerScored = default;
        [SerializeField] private AudioClip enemyHit     = default;
        [SerializeField] private AudioClip playerWin    = default;
        [SerializeField] private AudioClip playerLose   = default;

        void Awake()
        {
            audioSource = transform.gameObject.GetComponent<AudioSource>();
            audioSource.loop        = false;
            audioSource.playOnAwake = false;
            audioSource.volume      = DEFAULT_MASTER_VOLUME;
        }

        void OnEnable()
        {
            GameEventCenter.scoreChange.AddListener(PlaySoundOnPlayerScored);
            GameEventCenter.enemyHit   .AddListener(PlaySoundOnEnemyHit);
            GameEventCenter.enemyKilled.AddListener(PlaySoundOnEnemyKilled);

            GameEventCenter.startNewGame.AddListener(SetMasterVolume);
            GameEventCenter.pauseGame   .AddListener(PauseAnyActiveSoundEffects);
            GameEventCenter.resumeGame  .AddListener(ResumeAnyActiveSoundEffects);
            GameEventCenter.gameOver    .AddListener(PlaySoundOnGameOver);
        }
        void OnDisable()
        {
            GameEventCenter.scoreChange.RemoveListener(PlaySoundOnPlayerScored);
            GameEventCenter.enemyHit   .RemoveListener(PlaySoundOnEnemyHit);
            GameEventCenter.enemyKilled.RemoveListener(PlaySoundOnEnemyKilled);

            GameEventCenter.startNewGame.RemoveListener(SetMasterVolume);
            GameEventCenter.pauseGame   .RemoveListener(PauseAnyActiveSoundEffects);
            GameEventCenter.resumeGame  .RemoveListener(ResumeAnyActiveSoundEffects);
            GameEventCenter.gameOver    .RemoveListener(PlaySoundOnGameOver);
        }

        private void PauseAnyActiveSoundEffects(PlayerStatsInfo _)
        {
            audioSource.Pause();
        }
        private void ResumeAnyActiveSoundEffects(string _)
        {
            audioSource.UnPause();
        }
        private void SetMasterVolume(GameSettingsInfo gameSettings)
        {
            audioSource.volume = gameSettings.SoundVolume / 100.0f;
        }

        private void PlaySoundOnPlayerScored(PlayerStatsInfo playerInfo)
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

        private void PlaySoundOnGameOver(PlayerStatsInfo playerInfo)
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
