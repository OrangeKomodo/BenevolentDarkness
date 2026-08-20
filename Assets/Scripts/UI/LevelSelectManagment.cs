using GameManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
	public class LevelSelectManagment : MonoBehaviour {

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
					buttonIndex = (4 + (buttonIndex + (int)(controllerY / Mathf.Abs(controllerY)))) % 4;
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
				LoadMenu (1);
			}
		}

		public void LoadMenu (int menuIndex) {
			audioManager.PlaySound("Select");
			menuSwitcher.LoadMenu (menuIndex);
		}

		public void LoadLevel (string levelName){
			audioManager.PlaySound("Select");
			SceneManager.LoadScene (levelName);
		}
		
		void ChangeButton (int newButton) {
			for (int i = 0; i < 4; i++)
			{
				transform.GetChild (i + 1).GetComponent<Image> ().color = newButton == i ? selected : notSelected;
			}
		}

		void SelectButton (int button) {
			if (button == 0)
			{
				LoadLevel ("Level 1");
			}
			else if (button == 1)
			{
				LoadLevel ("Level 2");
			}
			else if (button == 2)
			{
				LoadLevel ("Level 3");
			}
			else if (button == 3)
			{
				LoadMenu (1);
			}
		}
	}
}
