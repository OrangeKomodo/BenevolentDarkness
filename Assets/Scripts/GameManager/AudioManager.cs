using UnityEngine;

namespace GameManager
{
	public class AudioManager : MonoBehaviour
	{

		public static AudioManager instance;

		[SerializeField] Sound[] sound;

		void Awake()
		{
			if (instance == null)
			{
				instance = this;
			}
			else if (instance != this)
			{
				Destroy(gameObject);
			}
		}

		void Start()
		{
			for (int x = 0; x < sound.Length; x++)
			{
				GameObject _go = new GameObject("Sound_" + x + "_" + sound[x].clipName);
				_go.transform.SetParent(this.transform);
				sound[x].SetSource(_go.AddComponent<AudioSource>());
				if (sound[x].playOnAwake)
					sound[x].Play();
			}
		}

		public void PlaySound(string _name)
		{
			for (int x = 0; x < sound.Length; x++)
			{
				if (sound[x].clipName == _name)
				{
					sound[x].Play();
					return;
				}
			}

			//Debug.LogError("There is no sound called: " + _name);
		}

		public void StopSound(string _name)
		{
			for (int x = 0; x < sound.Length; x++)
			{
				if (sound[x].clipName == _name)
				{
					sound[x].Stop();
					return;
				}
			}

			//Debug.LogError("There is no sound called: " + _name);
		}

		public void PauseSound(string _name, bool pause)
		{
			for (int x = 0; x < sound.Length; x++)
			{
				if (sound[x].clipName == _name)
				{
					sound[x].Pause(pause);
					return;
				}
			}

			//Debug.LogError("There is no sound called: " + _name);
		}
	}
}