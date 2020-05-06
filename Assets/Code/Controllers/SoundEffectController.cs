using UnityEngine;


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
        GameEventCenter.enemyHit.AddListener(PlaySoundOnEnemyHit);
        GameEventCenter.enemyKilled.AddListener(PlaySoundOnEnemyKilled);

        GameEventCenter.startNewGame.AddListener(SetMasterVolume);
        GameEventCenter.pauseGame.AddListener(PauseAnyActiveSoundEffects);
        GameEventCenter.resumeGame.AddListener(ResumeAnyActiveSoundEffects);
        GameEventCenter.gameOver.AddListener(PlayerSoundOnGameOver);
    }
    void OnDisable()
    {
        GameEventCenter.enemyHit.AddListener(PlaySoundOnEnemyHit);
        GameEventCenter.enemyKilled.AddListener(PlaySoundOnEnemyKilled);

        GameEventCenter.startNewGame.RemoveListener(SetMasterVolume);
        GameEventCenter.pauseGame.RemoveListener(PauseAnyActiveSoundEffects);
        GameEventCenter.resumeGame.RemoveListener(ResumeAnyActiveSoundEffects);
        GameEventCenter.gameOver.RemoveListener(PlayerSoundOnGameOver);
    }

    private void PauseAnyActiveSoundEffects(PlayerInfo _)
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
    private void PlayerSoundOnGameOver(PlayerInfo playerInfo)
    {
        // todo: add some sort of extra field for victory/failure conditions
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
