using UnityEngine;
using UnityEngine.Audio;

namespace GameManager
{
    [System.Serializable]
    public class Sound
    {
        #region Fields

        private AudioSource _source;

        public string ClipName;
        public AudioClip Clip;

        [Range(0f, 1f)] public float Volume = 1f;
        [Range(0f, 3f)] public float Pitch = 1f;

        public bool Loop = false;
        public bool PlayOnAwake = false;

        #endregion

        #region Setup Functions

        // AI - Configure the AudioSource with the specified settings and mixer group.
        public void SetSource(AudioMixerGroup group, AudioSource source)
        {
            _source = source;
            _source.clip = Clip;
            _source.pitch = Pitch;
            _source.volume = Volume;
            _source.loop = Loop;
            _source.playOnAwake = PlayOnAwake;
            _source.outputAudioMixerGroup = group;
        }

        #endregion

        #region Playback Functions

        // AI - Begin playing the audio clip through the configured AudioSource.
        public void Play()
        {
            if (_source == null)
            {
                return;
            }
            
            _source.Play();
        }

        // AI - Stop playing the audio clip.
        public void Stop()
        {
            if (_source == null)
            {
                return;
            }

            _source.Stop();
        }

        // AI - Pause or unpause the audio clip based on the provided boolean.
        public void Pause(bool pause)
        {
            if (_source == null)
            {
                return;
            }

            if (pause)
            {
                _source.Pause();
                return;
            }
            
            _source.UnPause();
        }

        #endregion
    }
}
