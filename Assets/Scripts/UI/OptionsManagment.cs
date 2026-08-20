using System.Collections.Generic;
using GameManager;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace UI
{
	public class OptionsManagment : MonoBehaviour
	{

		public MenuSwitcher menuSwitcher;
		public AudioMixer audioMixer;
		public Slider[] volumeSliders;
		public List<int> screenWidths = new List<int>() { };

		void Start()
		{
			volumeSliders[0].value = PlayerPrefs.GetFloat("MasterVolume", 1);
			volumeSliders[1].value = PlayerPrefs.GetFloat("MusicVolume", 1);
			volumeSliders[2].value = PlayerPrefs.GetFloat("SFXVolume", 1);
			UpdateVolumes();
		}


		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				menuSwitcher.LoadMenu(1);
			}
		}

		public void ToMarbleSelect()
		{
			menuSwitcher.LoadMenu(5);
		}

		public void ScreenResolution(int i)
		{
			float aspectRatio = 16 / 9;
			Screen.SetResolution(screenWidths[i], (int)(screenWidths[i] / aspectRatio), false);
		}

		public void UpdateVolumes()
		{
			audioMixer.SetFloat("MasterVolume", Scale(volumeSliders[0].value));
			audioMixer.SetFloat("MusicVolume", Scale(volumeSliders[1].value));
			audioMixer.SetFloat("SFXVolume", Scale(volumeSliders[2].value));
		}

		float Scale(float volume)
		{
			return volume * 80f - 80f;
		}
	}
}
