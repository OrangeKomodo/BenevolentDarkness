using UnityEngine;
using UnityEngine.Audio;

namespace GameManager
{
    [System.Serializable]
    public class Sound
    {
        public AudioMixerGroup audioMixerGroup;
        private AudioSource source;

        public string clipName;
        public AudioClip clip;

        [Range(0f, 1f)] public float volume;
        [Range(0f, 3f)] public float pitch;

        public bool loop = false;
        public bool playOnAwake = false;

        public void SetSource(AudioSource _source)
        {
            source = _source;
            source.clip = clip;
            source.pitch = pitch;
            source.volume = volume;
            source.loop = loop;
            source.playOnAwake = playOnAwake;
            source.outputAudioMixerGroup = audioMixerGroup;
        }

        public void Play()
        {
            source.Play();
        }

        public void Stop()
        {
            source.Stop();
        }

        public void Pause(bool pause)
        {
            if (pause)
                source.Pause();
            else
                source.UnPause();
        }
    }
}