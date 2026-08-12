using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSwitcher : MonoBehaviour {

	public Transform canvas;
	public int selectedMenu = 0;

	void Start () {
		Time.timeScale = 1f;
		if (SceneManager.GetActiveScene ().name.Equals ("Main Menu")) {
			LoadMenu (PlayerPrefs.GetInt ("MainMenuSetting", 0));
			PlayerPrefs.DeleteKey ("MainMenuSetting");
		} else
			LoadMenu (selectedMenu);
	}

	public void LoadMenu (int newMenu){
		int x = 0;
		selectedMenu = newMenu;
		foreach (Transform menu in canvas) {
			if (x == selectedMenu) {
				menu.gameObject.SetActive (true);
			} else
				menu.gameObject.SetActive (false);
			x++;
		}
	}
}