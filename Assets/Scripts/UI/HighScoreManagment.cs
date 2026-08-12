using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class HighScoreManagment : MonoBehaviour {
	public MenuSwitcher menuSwitcher;

	public Text[] parTexts;
	public Text[] highTexts;

	float[] parTimes = new float[]{ 5f, 20f, 30f, 30f };

	// Use this for initialization
	void Start () {
		for (int x = 0; x < 4; x++) {
			parTexts [x].text = Convert (parTimes [x]);
			float highTime = PlayerPrefs.GetFloat ("BestTime" + (x + 1), 6000);
			if (highTime == 6000)
				highTime = 0;
			highTexts [x].text = Convert (highTime);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			menuSwitcher.LoadMenu (1);
		}
	}

	string Convert (float t) {
		float rawMinutes = (int)t / 60;
		string minutes;
		if (rawMinutes < 10)
			minutes = "0" + rawMinutes.ToString ();
		else
			minutes = rawMinutes.ToString ();

		float rawSeconds = t % 60;
		string seconds;
		if (rawSeconds < 10)
			seconds = "0" + rawSeconds.ToString ("f3");
		else
			seconds = rawSeconds.ToString ("f3");

		return minutes + ":" + seconds;
	}
}
