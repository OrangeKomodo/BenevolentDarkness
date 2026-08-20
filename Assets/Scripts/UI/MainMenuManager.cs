using GameManager;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class MainMenuManager : MonoBehaviour {

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
			if (usingController) {
				ChangeButton (buttonIndex);
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
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

			if (Input.GetButtonDown("Jump"))
			{
				SelectButton (buttonIndex);
			}

			if (Input.GetButtonDown("Exit"))
			{
				LoadMenu (0);
			}
		}

		public void LoadMenu (int menuIndex) {
			audioManager.PlaySound("Select");
			menuSwitcher.LoadMenu (menuIndex);
		}

		public void QuitGame () {
			audioManager.PlaySound("Select");
			Application.Quit ();
		}

		void ChangeButton (int newButton) {
			for (int i = 0; i < 3; i++)
			{
				transform.GetChild (i + 1).GetComponent<Image> ().color = newButton == i ? selected : notSelected;
			}
		}

		void SelectButton (int button) {
			if (button == 0)
			{
				LoadMenu (2);
			}
			else if (button == 1)
			{
				LoadMenu (3);
			}
			else if (button == 2)
			{
				QuitGame ();
			}
		}
	}
}
