using UnityEngine;


namespace PQ.Sound
{
    public class SoundTrackController : MonoBehaviour
    {
        private static float DefaultMasterVolume = 1.0f;

        private AudioSource track;

        void Awake()
        {
            track = transform.gameObject.GetComponent<AudioSource>();
            track.loop        = true;
            track.playOnAwake = false;
            track.volume      = DefaultMasterVolume;
        }

        void OnEnable()
        {
            GameEventCenter.startNewGame.AddListener(StartTrack);
            GameEventCenter.restartGame .AddListener(RestartTrack);
            GameEventCenter.pauseGame   .AddListener(PauseTrack);
            GameEventCenter.resumeGame  .AddListener(ResumeTrack);
            GameEventCenter.gameOver    .AddListener(EndTrack);
        }
        void OnDisable()
        {
            GameEventCenter.startNewGame.RemoveListener(StartTrack);
            GameEventCenter.restartGame .RemoveListener(RestartTrack);
            GameEventCenter.pauseGame   .RemoveListener(PauseTrack);
            GameEventCenter.resumeGame  .RemoveListener(ResumeTrack);
            GameEventCenter.gameOver    .RemoveListener(EndTrack);
        }

        private void StartTrack(PlayerSettingsInfo gameSettings)
        {
            track.volume = gameSettings.MusicVolume / 100.0f;
            track.Play();
        }
        private void RestartTrack(string _)
        {
            track.Stop();
            track.Play();
        }
        private void PauseTrack(PlayerProgressionInfo _)
        {
            track.Pause();
        }
        private void ResumeTrack(string _)
        {
            track.UnPause();
        }
        private void EndTrack(PlayerProgressionInfo _)
        {
            track.Stop();
        }
    }
}
