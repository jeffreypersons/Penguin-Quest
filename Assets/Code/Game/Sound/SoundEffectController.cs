using UnityEngine;


namespace PQ.Game.Sound
{
    public class SoundEffectController : MonoBehaviour
    {
        private AudioSource _audioSource;

        public void SetMasterVolume(float volume) => _audioSource.volume = volume;
        public void PauseAll()  => _audioSource.Pause();
        public void ResumeAll() => _audioSource.UnPause();

        void Awake()
        {
            _audioSource = transform.gameObject.GetComponent<AudioSource>();
            _audioSource.loop        = false;
            _audioSource.playOnAwake = false;
            _audioSource.volume      = 0.5f;
        }
    }
}
