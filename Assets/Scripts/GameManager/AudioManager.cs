using UnityEngine;
using UnityEngine.Audio;

namespace GameManager
{
	public class AudioManager : Singleton<AudioManager>
	{
		[Header("SFX Assets")]
		public AudioMixerGroup SfxMixerGroup;
		[SerializeField] private Sound[] SfxClips;
		[Header("Music Assets")]
		public AudioMixerGroup MusicMixerGroup;
		[SerializeField] private Sound[] MusicClips;

		private void Start()
		{
			// Set up the SFX GameObject and create an AudioSource for each sound
			// Only these sounds may be played from within the game
			GameObject sfxGameObject = new GameObject("Audio_SFX_Clips");
			sfxGameObject.transform.SetParent(this.transform);
			
			for (int clipIndex = 0; clipIndex < SfxClips.Length; ++clipIndex)
			{
				AudioSource audioSource = sfxGameObject.AddComponent<AudioSource>();
				Sound sfxClip = SfxClips[clipIndex];
				
				sfxClip.SetSource(SfxMixerGroup, audioSource);
				if (sfxClip.PlayOnAwake)
				{
					sfxClip.Play();
				}
			}
			
			// Set up the Music GameObject and create an AudioSource for each sound
			// These sounds may only be started by setting "PlayOnAwake" to True
			// TODO: Add this to a DontDestroyOnLoad scene and implement a method that changes the music when the scene changes
			GameObject musicGameObject = new GameObject("Audio_Music_Clips");
			musicGameObject.transform.SetParent(this.transform);
			
			for (int clipIndex = 0; clipIndex < MusicClips.Length; ++clipIndex)
			{
				AudioSource audioSource = musicGameObject.AddComponent<AudioSource>();
				Sound musicClip = MusicClips[clipIndex];
				
				musicClip.SetSource(MusicMixerGroup, audioSource);
				if (musicClip.PlayOnAwake)
				{
					musicClip.Play();
				}
			}
		}

		public void PlaySound(string clipName)
		{
			Sound sfxSound = GetSfxSound(clipName);
			if (sfxSound == null)
			{
				return;
			}
			
			sfxSound.Play();
		}

		public void StopSound(string clipName)
		{
			Sound sfxSound = GetSfxSound(clipName);
			if (sfxSound == null)
			{
				return;
			}
			
			sfxSound.Stop();
		}

		public void PauseSound(string clipName, bool pause)
		{
			Sound sfxSound = GetSfxSound(clipName);
			if (sfxSound == null)
			{
				return;
			}
			
			sfxSound.Pause(pause);
		}

		private Sound GetSfxSound(string clipName)
		{
			if (string.IsNullOrWhiteSpace(clipName))
			{
				return null;
			}
			
			for (int clipIndex = 0; clipIndex < SfxClips.Length; ++clipIndex)
			{
				if (SfxClips[clipIndex].ClipName == clipName)
				{
					return SfxClips[clipIndex];
				}
			}

			//Debug.LogError("There is no SFX sound called: " + clipName);
			return null;
		}
	}
}