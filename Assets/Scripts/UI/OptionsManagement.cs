using System.Collections.Generic;
using GameManager;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace UI
{
	public class OptionsManagement : MonoBehaviour
	{

		public MenuSwitcher MenuSwitcher;
		public AudioMixer AudioMixer;
		public Slider[] VolumeSliders;
		public List<int> ScreenWidths = new List<int>() { };

		private const float AspectRatio = 16f / 9f;

		void Start()
		{
			VolumeSliders[0].value = PlayerPrefs.GetFloat("MasterVolume", 1);
			VolumeSliders[1].value = PlayerPrefs.GetFloat("MusicVolume", 1);
			VolumeSliders[2].value = PlayerPrefs.GetFloat("SFXVolume", 1);
			UpdateVolumes();
		}


		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				MenuSwitcher.LoadMenu(1);
			}
		}

		public void ToMarbleSelect()
		{
			MenuSwitcher.LoadMenu(5);
		}

		public void ScreenResolution(int i)
		{
			Screen.SetResolution(ScreenWidths[i], (int)(ScreenWidths[i] / AspectRatio), false);
		}

		public void UpdateVolumes()
		{
			AudioMixer.SetFloat("MasterVolume", Scale(VolumeSliders[0].value));
			AudioMixer.SetFloat("MusicVolume", Scale(VolumeSliders[1].value));
			AudioMixer.SetFloat("SFXVolume", Scale(VolumeSliders[2].value));
		}

		float Scale(float volume)
		{
			return volume * 80f - 80f;
		}
	}
}
