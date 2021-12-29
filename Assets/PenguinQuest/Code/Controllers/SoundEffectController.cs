using UnityEngine;
using PenguinQuest.Data;


namespace PenguinQuest.Controllers
{
    public class SoundEffectController : MonoBehaviour
    {
        private AudioSource audioSource;
        private static float DEFAULT_MASTER_VOLUME = 0.5f;

        [SerializeField] private float volumeScaleWallHit    = default;
        [SerializeField] private float volumeScalePaddleHit  = default;
        [SerializeField] private float volumeScaleGoalHit    = default;
        [SerializeField] private float volumeScaleGameFinish = default;

        [SerializeField] private AudioClip wallHitSound    = default;
        [SerializeField] private AudioClip paddleHitSound  = default;
        [SerializeField] private AudioClip playerScored    = default;
        [SerializeField] private AudioClip opponentScored  = default;
        [SerializeField] private AudioClip playerWinSound  = default;
        [SerializeField] private AudioClip playerLoseSound = default;

        void Awake()
        {
            audioSource = transform.gameObject.GetComponent<AudioSource>();
            audioSource.loop        = false;
            audioSource.playOnAwake = false;
            audioSource.volume      = DEFAULT_MASTER_VOLUME;
        }

        void OnEnable()
        {
            GlobalGameEventCenter.enemyHit   .AddListener(PlaySoundOnEnemyHit);
            GlobalGameEventCenter.enemyKilled.AddListener(PlaySoundOnEnemyKilled);

            GlobalGameEventCenter.startNewGame.AddListener(SetMasterVolume);
            GlobalGameEventCenter.pauseGame   .AddListener(PauseAnyActiveSoundEffects);
            GlobalGameEventCenter.resumeGame  .AddListener(ResumeAnyActiveSoundEffects);
            GlobalGameEventCenter.gameOver    .AddListener(PlayerSoundOnGameOver);
        }
        void OnDisable()
        {
            GlobalGameEventCenter.enemyHit   .AddListener(PlaySoundOnEnemyHit);
            GlobalGameEventCenter.enemyKilled.AddListener(PlaySoundOnEnemyKilled);

            GlobalGameEventCenter.startNewGame.RemoveListener(SetMasterVolume);
            GlobalGameEventCenter.pauseGame   .RemoveListener(PauseAnyActiveSoundEffects);
            GlobalGameEventCenter.resumeGame  .RemoveListener(ResumeAnyActiveSoundEffects);
            GlobalGameEventCenter.gameOver    .RemoveListener(PlayerSoundOnGameOver);
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

        private void PlaySoundOnEnemyHit(string _)
        {
            audioSource.PlayOneShot(paddleHitSound, volumeScalePaddleHit);
        }

        private void PlaySoundOnEnemyKilled(int numPoints)
        {
            audioSource.PlayOneShot(opponentScored, volumeScaleGoalHit);
        }
        private void PlayerSoundOnGameOver(PlayerStatsInfo playerInfo)
        {
            bool placeHolderVictoryCondition = false;
            if (placeHolderVictoryCondition)
            {
                audioSource.PlayOneShot(playerWinSound, volumeScaleGameFinish);
            }
            else
            {
                audioSource.PlayOneShot(playerLoseSound, volumeScaleGameFinish);
            }
        }
    }
}
