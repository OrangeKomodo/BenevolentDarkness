using UnityEngine;

namespace GameManager
{
	public class AudioManager : Singleton<AudioManager>
	{
		[SerializeField] Sound[] sound;

		private void Start()
		{
			for (int x = 0; x < sound.Length; x++)
			{
				GameObject audioGameObject = new GameObject("Sound_" + x + "_" + sound[x].clipName);
				audioGameObject.transform.SetParent(this.transform);
				sound[x].SetSource(audioGameObject.AddComponent<AudioSource>());
				if (sound[x].playOnAwake)
					sound[x].Play();
			}
		}

		public void PlaySound(string clipName)
		{
			for (int x = 0; x < sound.Length; x++)
			{
				if (sound[x].clipName == clipName)
				{
					sound[x].Play();
					return;
				}
			}

			//Debug.LogError("There is no sound called: " + _name);
		}

		public void StopSound(string clipName)
		{
			for (int x = 0; x < sound.Length; x++)
			{
				if (sound[x].clipName == clipName)
				{
					sound[x].Stop();
					return;
				}
			}

			//Debug.LogError("There is no sound called: " + _name);
		}

		public void PauseSound(string clipName, bool pause)
		{
			for (int x = 0; x < sound.Length; x++)
			{
				if (sound[x].clipName == clipName)
				{
					sound[x].Pause(pause);
					return;
				}
			}

			//Debug.LogError("There is no sound called: " + _name);
		}
	}
}