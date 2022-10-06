using UnityEngine;
using PQ.Common.Events;


namespace PQ.Sound
{
    public class SoundTrackController : MonoBehaviour
    {
        private static float DefaultMasterVolume = 1.0f;

        private AudioSource _track;
        private GameEventCenter _eventCenter;
        private PqEventRegistry _soundTrackEventRegistry;

        void Awake()
        {
            _eventCenter = GameEventCenter.Instance;

            _track = transform.gameObject.GetComponent<AudioSource>();
            _track.loop        = true;
            _track.playOnAwake = false;
            _track.volume      = DefaultMasterVolume;
            
            _soundTrackEventRegistry = new PqEventRegistry();
            _soundTrackEventRegistry.Add(_eventCenter.startNewGame, StartTrack);
            _soundTrackEventRegistry.Add(_eventCenter.restartGame,  RestartTrack);
            _soundTrackEventRegistry.Add(_eventCenter.pauseGame,    PauseTrack);
            _soundTrackEventRegistry.Add(_eventCenter.resumeGame,   ResumeTrack);
            _soundTrackEventRegistry.Add(_eventCenter.gameOver,     EndTrack);
        }

        void OnEnable()
        {
            _soundTrackEventRegistry.SubscribeToAllRegisteredEvents();
        }

        void OnDisable()
        {
            _soundTrackEventRegistry.UnsubscribeToAllRegisteredEvents();
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
        private void PauseTrack(PlayerProgressionInfo _) => _track.Pause();
        private void ResumeTrack()                       => _track.UnPause();
        private void EndTrack(PlayerProgressionInfo _)   => _track.Stop();
    }
}
