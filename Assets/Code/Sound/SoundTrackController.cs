using UnityEngine;


namespace PQ.Sound
{
    public class SoundTrackController : MonoBehaviour
    {
        private static float DefaultMasterVolume = 1.0f;

        private AudioSource _track;
        private GameEventCenter _eventCenter;

        void Awake()
        {
            _eventCenter = GameEventCenter.Instance;

            _track = transform.gameObject.GetComponent<AudioSource>();
            _track.loop        = true;
            _track.playOnAwake = false;
            _track.volume      = DefaultMasterVolume;
        }

        void OnEnable()
        {
            _eventCenter.startNewGame.AddListener(StartTrack);
            _eventCenter.restartGame .AddListener(RestartTrack);
            _eventCenter.pauseGame   .AddListener(PauseTrack);
            _eventCenter.resumeGame  .AddListener(ResumeTrack);
            _eventCenter.gameOver    .AddListener(EndTrack);
        }
        void OnDisable()
        {
            _eventCenter.startNewGame.RemoveListener(StartTrack);
            _eventCenter.restartGame .RemoveListener(RestartTrack);
            _eventCenter.pauseGame   .RemoveListener(PauseTrack);
            _eventCenter.resumeGame  .RemoveListener(ResumeTrack);
            _eventCenter.gameOver    .RemoveListener(EndTrack);
        }

        private void StartTrack(PlayerSettingsInfo gameSettings)
        {
            _track.volume = gameSettings.MusicVolume / 100.0f;
            _track.Play();
        }
        private void RestartTrack(string _)
        {
            _track.Stop();
            _track.Play();
        }
        private void PauseTrack(PlayerProgressionInfo _)
        {
            _track.Pause();
        }
        private void ResumeTrack(string _)
        {
            _track.UnPause();
        }
        private void EndTrack(PlayerProgressionInfo _)
        {
            _track.Stop();
        }
    }
}
