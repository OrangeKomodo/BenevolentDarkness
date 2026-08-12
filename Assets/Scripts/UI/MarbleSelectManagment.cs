using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MarbleSelectManagment : MonoBehaviour {
	public MenuSwitcher menuSwitcher;

	public Image ballGraphic;
	public Sprite[] skins;
	public int maxSkins;

	int onBall = 0;

	// Use this for initialization
	void Start () {
		onBall = PlayerPrefs.GetInt ("PlayerSkins", 0);
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			PlayerPrefs.SetInt ("PlayerSkin", onBall);
			menuSwitcher.LoadMenu (4);
		}

		if (onBall == 0) {
			ballGraphic.sprite = skins [0];
		} else if (onBall == 1) {
			ballGraphic.sprite = skins [1];
		} else if (onBall == 2) {
			ballGraphic.sprite = skins [2];
		} else if (onBall == 3) {
			ballGraphic.sprite = skins [3];
		} else if (onBall == 4) {
			ballGraphic.sprite = skins [4];
		}
		
	}

	public void LeftArrow () {
		onBall --;
		if (onBall < 0) {
			onBall = 4;
		}
	}

	public void RightArrow () {
		onBall ++;
		if (onBall > 4) {
			onBall = 0;
		}
	}
}
