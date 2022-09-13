using UnityEngine;
using PQ.Common;


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
            _eventCenter.startNewGame.AddHandler(StartTrack);
            _eventCenter.restartGame .AddHandler(RestartTrack);
            _eventCenter.pauseGame   .AddHandler(PauseTrack);
            _eventCenter.resumeGame  .AddHandler(ResumeTrack);
            _eventCenter.gameOver    .AddHandler(EndTrack);
        }
        void OnDisable()
        {
            _eventCenter.startNewGame.RemoveHandler(StartTrack);
            _eventCenter.restartGame .RemoveHandler(RestartTrack);
            _eventCenter.pauseGame   .RemoveHandler(PauseTrack);
            _eventCenter.resumeGame  .RemoveHandler(ResumeTrack);
            _eventCenter.gameOver    .RemoveHandler(EndTrack);
        }

        private void StartTrack(PlayerSettingsInfo gameSettings)
        {
            _track.volume = gameSettings.MusicVolume / 100.0f;
            _track.Play();
        }
        private void RestartTrack()
        {
            _track.Stop();
            _track.Play();
        }
        private void PauseTrack(PlayerProgressionInfo _)
        {
            _track.Pause();
        }
        private void ResumeTrack()
        {
            _track.UnPause();
        }
        private void EndTrack(PlayerProgressionInfo _)
        {
            _track.Stop();
        }
    }
}
