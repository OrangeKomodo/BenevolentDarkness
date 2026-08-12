using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManagment : MonoBehaviour {

    AudioManager audioManager;
	MenuSwitcher menuSwitcher;

    Color selected = Color.white;
    Color notSelected = Color.gray;

    bool usingController;

	int buttonIndex = 0;
	float nextChangeTime;

	void Start () {
        audioManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<AudioManager>();
        menuSwitcher = GameObject.FindGameObjectWithTag ("GameController").GetComponent<MenuSwitcher> ();
		usingController = Input.GetJoystickNames ().Length > 0;
		if (usingController)
			ChangeButton (buttonIndex);
	}

	void Update () {
        if (usingController) {
            float controllerY = -Input.GetAxis("Vertical");
            if (nextChangeTime <= Time.realtimeSinceStartup && Mathf.Abs(controllerY) > 0.19f) {
                audioManager.PlaySound("Swish");
                buttonIndex = (4 + (buttonIndex + (int)(controllerY / Mathf.Abs(controllerY)))) % 4;
                ChangeButton(buttonIndex);
                nextChangeTime = Time.realtimeSinceStartup + 0.2f;
            }
        }

		if (Input.GetButtonDown ("Jump"))
			SelectButton (buttonIndex);

		if (Input.GetButtonDown ("Cancel"))
			Resume ();
	}

	public void Resume () {
        audioManager.PlaySound("Select");
        audioManager.PauseSound("Alarm", false);
        Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		Time.timeScale = 1;
		menuSwitcher.LoadMenu (0);
	}

	public void Restart () {
        audioManager.PlaySound("Select");
        Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		Time.timeScale = 1;
		SceneManager.LoadScene (SceneManager.GetActiveScene().name);
	}
		
	public void ToMainMenu () {
        audioManager.PlaySound("Select");
        PlayerPrefs.SetInt ("MainMenuSetting", 1);
		SceneManager.LoadScene ("Main Menu");
	}
		
	public void ToLevelSelect () {
        audioManager.PlaySound("Select");
        PlayerPrefs.SetInt ("MainMenuSetting", 2);
		SceneManager.LoadScene ("Main Menu");
	}

	void ChangeButton (int newButton) {
		for (int i = 0; i < 4; i++)
			transform.GetChild (i + 1).GetComponent<Image> ().color = newButton == i ? selected : notSelected;
	}

	void SelectButton (int button) {
		if (button == 0)
			Resume ();
		else if (button == 1)
			Restart ();
		else if (button == 2)
			ToLevelSelect ();
		else if (button == 3)
			ToMainMenu ();
	}
}
