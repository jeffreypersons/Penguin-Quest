
using UnityEngine;


namespace PQ.Game.Sound
{
    public class SoundTrackController : MonoBehaviour
    {
        private AudioSource _audioSource;

        public void SetMasterVolume(float volume) => _audioSource.volume = volume;
        public void PlayTrack()   { _audioSource.Stop(); _audioSource.Play(); }
        public void PauseTrack()  => _audioSource.Pause();
        public void ResumeTrack() => _audioSource.UnPause();
        public void EndTrack()    => _audioSource.Stop();

        void Awake()
        {
            _audioSource = transform.gameObject.GetComponent<AudioSource>();
            _audioSource.loop        = true;
            _audioSource.playOnAwake = false;
            _audioSource.volume      = 1.0f;
        }
    }
}
