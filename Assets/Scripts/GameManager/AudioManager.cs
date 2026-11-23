using UnityEngine;
using UnityEngine.Audio;

namespace GameManager
{
	public class AudioManager : Singleton<AudioManager>
	{
		#region Fields

		[Header("SFX Assets")]
		public AudioMixerGroup SfxMixerGroup;
		[SerializeField] private Sound[] SfxClips;
		
		[Header("Music Assets")]
		public AudioMixerGroup MusicMixerGroup;
		[SerializeField] private Sound[] MusicClips;

		#endregion

		#region Initialization Functions

		private void Start()
		{
			InitializeSfxClips();
			InitializeMusicClips();
		}

		// AI - Set up the SFX GameObject and create an AudioSource for each sound.
		private void InitializeSfxClips()
		{
			GameObject sfxGameObject = new GameObject("Audio_SFX_Clips");
			sfxGameObject.transform.SetParent(transform);
			
			for (int clipIndex = 0; clipIndex < SfxClips.Length; ++clipIndex)
			{
				AudioSource audioSource = sfxGameObject.AddComponent<AudioSource>();
				Sound sfxClip = SfxClips[clipIndex];
				
				if (sfxClip == null)
				{
					continue;
				}

				sfxClip.SetSource(SfxMixerGroup, audioSource);
				if (sfxClip.PlayOnAwake)
				{
					sfxClip.Play();
				}
			}
		}

		// AI - Set up the Music GameObject and create an AudioSource for each sound.
		// AI - TODO: Add this to a DontDestroyOnLoad scene and implement a method that changes the music when the scene changes.
		private void InitializeMusicClips()
		{
			GameObject musicGameObject = new GameObject("Audio_Music_Clips");
			musicGameObject.transform.SetParent(transform);
			
			for (int clipIndex = 0; clipIndex < MusicClips.Length; ++clipIndex)
			{
				AudioSource audioSource = musicGameObject.AddComponent<AudioSource>();
				Sound musicClip = MusicClips[clipIndex];
				
				if (musicClip == null)
				{
					continue;
				}

				musicClip.SetSource(MusicMixerGroup, audioSource);
				if (musicClip.PlayOnAwake)
				{
					musicClip.Play();
				}
			}
		}

		#endregion

		#region Sound Playback Functions

		// AI - Play a sound effect by name.
		public void PlaySound(string clipName)
		{
			Sound sfxSound = GetSfxSound(clipName);
			if (sfxSound == null)
			{
				return;
			}
			
			sfxSound.Play();
		}

		// AI - Stop a sound effect by name.
		public void StopSound(string clipName)
		{
			Sound sfxSound = GetSfxSound(clipName);
			if (sfxSound == null)
			{
				return;
			}
			
			sfxSound.Stop();
		}

		// AI - Pause or unpause a sound effect by name.
		public void PauseSound(string clipName, bool pause)
		{
			Sound sfxSound = GetSfxSound(clipName);
			if (sfxSound == null)
			{
				return;
			}
			
			sfxSound.Pause(pause);
		}

		#endregion

		#region Sound Lookup Functions

		// AI - Find a sound effect by name.
		private Sound GetSfxSound(string clipName)
		{
			if (string.IsNullOrWhiteSpace(clipName))
			{
				return null;
			}
			
			for (int clipIndex = 0; clipIndex < SfxClips.Length; ++clipIndex)
			{
				if (SfxClips[clipIndex] != null && SfxClips[clipIndex].ClipName == clipName)
				{
					return SfxClips[clipIndex];
				}
			}

			// AI - Sound not found, could log error here if desired.
			return null;
		}

		#endregion
	}
}
