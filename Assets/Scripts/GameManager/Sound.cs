using UnityEngine;
using UnityEngine.Audio;

namespace GameManager
{
    [System.Serializable]
    public class Sound
    {
        private AudioSource _source;

        public string ClipName;
        public AudioClip Clip;

        [Range(0f, 1f)] public float Volume = 1f;
        [Range(0f, 3f)] public float Pitch = 1f;

        public bool Loop = false;
        public bool PlayOnAwake = false;

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

        public void Play()
        {
            if (_source == null)
            {
                return;
            }
            
            _source.Play();
        }

        public void Stop()
        {
            if (_source == null)
            {
                return;
            }

            _source.Stop();
        }

        public void Pause(bool pause)
        {
            if (_source == null)
            {
                return;
            }

            if (pause)
            {
                _source.Pause();
            }
            else
            {
                _source.UnPause();
            }
        }
    }
}