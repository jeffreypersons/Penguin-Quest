using UnityEngine;
using PenguinQuest.Data;


namespace PenguinQuest.Controllers
{
    public class SoundTrackController : MonoBehaviour
    {
        private AudioSource track;
        private static float DEFAULT_MASTER_VOLUME = 1.0f;

        void Awake()
        {
            track = transform.gameObject.GetComponent<AudioSource>();
            track.loop        = true;
            track.playOnAwake = false;
            track.volume      = DEFAULT_MASTER_VOLUME;
        }

        void OnEnable()
        {
            GlobalGameEventCenter.startNewGame.AddListener(StartTrack);
            GlobalGameEventCenter.restartGame .AddListener(RestartTrack);
            GlobalGameEventCenter.pauseGame   .AddListener(PauseTrack);
            GlobalGameEventCenter.resumeGame  .AddListener(ResumeTrack);
            GlobalGameEventCenter.gameOver    .AddListener(EndTrack);
        }
        void OnDisable()
        {
            GlobalGameEventCenter.startNewGame.RemoveListener(StartTrack);
            GlobalGameEventCenter.restartGame .RemoveListener(RestartTrack);
            GlobalGameEventCenter.pauseGame   .RemoveListener(PauseTrack);
            GlobalGameEventCenter.resumeGame  .RemoveListener(ResumeTrack);
            GlobalGameEventCenter.gameOver    .RemoveListener(EndTrack);
        }

        private void StartTrack(GameSettingsInfo gameSettings)
        {
            track.volume = gameSettings.MusicVolume / 100.0f;
            track.Play();
        }
        private void RestartTrack(string _)
        {
            track.Stop();
            track.Play();
        }
        private void PauseTrack(PlayerStatsInfo _)
        {
            track.Pause();
        }
        private void ResumeTrack(string _)
        {
            track.UnPause();
        }
        private void EndTrack(PlayerStatsInfo _)
        {
            track.Stop();
        }
    }
}
