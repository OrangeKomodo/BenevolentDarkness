using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

[System.Serializable]
public class Sound{

	public AudioMixerGroup audioMixerGroup;
	private AudioSource source;

	public string clipName;
	public AudioClip clip;

	[Range(0f,1f)]
	public float volume;
	[Range(0f,3f)]
	public float pitch;

	public bool loop = false;
	public bool playOnAwake = false;

	public void SetSource (AudioSource _source){
		source = _source;
		source.clip = clip;
		source.pitch = pitch;
		source.volume = volume;
		source.loop = loop;
		source.playOnAwake = playOnAwake;
		source.outputAudioMixerGroup = audioMixerGroup;
	}

	public void Play (){
		source.Play ();
	}

    public void Stop () {
        source.Stop();
    }

    public void Pause (bool pause) {
        if (pause)
            source.Pause();
        else
            source.UnPause();
    }
}

public class AudioManager : MonoBehaviour {
	
	public static AudioManager instance;

	[SerializeField]
	Sound[] sound;

	void Awake(){
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);
	}

	void Start(){
		for (int x = 0; x < sound.Length; x++) {
			GameObject _go = new GameObject ("Sound_" + x + "_" + sound[x].clipName);
			_go.transform.SetParent (this.transform);
			sound [x].SetSource (_go.AddComponent<AudioSource> ());
            if (sound[x].playOnAwake)
                sound[x].Play();
		}
	}

	public void PlaySound(string _name){
		for (int x = 0; x < sound.Length; x++) {
			if (sound [x].clipName == _name) {
				sound [x].Play();
				return;
			}
		}
		Debug.LogError ("There is no sound called: " + _name);
	}

    public void StopSound(string _name){
        for (int x = 0; x < sound.Length; x++) {
            if (sound[x].clipName == _name) {
                sound[x].Stop();
                return;
            }
        }
        Debug.LogError("There is no sound called: " + _name);
    }

    public void PauseSound(string _name, bool pause) {
        for (int x = 0; x < sound.Length; x++) {
            if (sound[x].clipName == _name) {
                sound[x].Pause(pause);
                return;
            }
        }
        Debug.LogError("There is no sound called: " + _name);
    }
}