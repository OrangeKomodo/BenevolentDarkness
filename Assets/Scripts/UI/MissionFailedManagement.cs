using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MissionFailedManagement : MonoBehaviour {

    AudioManager audioManager;

    Color selected = Color.white;
    Color notSelected = Color.gray;

    bool usingController;

	int buttonIndex = 0;
	float nextChangeTime;

	void Start () {
        audioManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<AudioManager>();
        audioManager.StopSound("Alarm");
        usingController = Input.GetJoystickNames ().Length > 0;
        if (usingController) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            ChangeButton(buttonIndex);
        }
	}

	void Update () {
        if (usingController) {
            float controllerY = -Input.GetAxis("Vertical");
            if (nextChangeTime <= Time.realtimeSinceStartup && Mathf.Abs(controllerY) > 0.19f) {
                audioManager.PlaySound("Swish");
                buttonIndex = (3 + (buttonIndex + (int)(controllerY / Mathf.Abs(controllerY)))) % 3;
                ChangeButton(buttonIndex);
                nextChangeTime = Time.realtimeSinceStartup + 0.2f;
            }
        }

		if (Input.GetButtonDown ("Jump"))
			SelectButton (buttonIndex);
	}

	public void SetCause (int causeIndex) {
		string causeText = "";

		switch (causeIndex) {
		case 0:{
				causeText = "You Died!";
				break;
			}
		}

		transform.GetChild (2).GetComponent<Text> ().text = causeText;
	}

	public void LoadLastSave () {
		//GameObject.FindGameObjectWithTag ("GameController").GetComponent<MenuSwitcher> ().LoadMenu (0);
		GameObject.FindGameObjectWithTag ("GameController").GetComponent<QuickSave> ().QuickLoadAll ();
	}

	public void RestartLevel () {
        audioManager.PlaySound("Select");
		SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
	}

	public void MainMenu () {
        audioManager.PlaySound("Select");
        SceneManager.LoadScene ("Main Menu");
	}

	public void QuitGame () {
        audioManager.PlaySound("Select");
        Application.Quit ();
	}

	void ChangeButton (int newButton) {
		for (int i = 0; i < 3; i++)
			transform.GetChild (i + 4).GetComponent<Image> ().color = newButton == i ? selected : notSelected;
	}

	void SelectButton (int button) {
		//if (button == 0)
		//	LoadLastSave ();
		//else 
		if (button == 0)
			RestartLevel ();
		else if (button == 1)
			MainMenu ();
		else if (button == 2)
			QuitGame ();
	}
}
