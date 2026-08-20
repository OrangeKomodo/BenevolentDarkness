using GameManager;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class CreditsManagement : MonoBehaviour {

		AudioManager audioManager;
		MenuSwitcher menuSwitcher;

		void Start () {
			audioManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<AudioManager>();
			menuSwitcher = GameObject.FindGameObjectWithTag ("GameController").GetComponent<MenuSwitcher> ();
			if (Input.GetJoystickNames().Length > 0)
			{
				transform.Find("Back Text").GetComponent<Text>().text = "[B] Back";
			}
		}

		void Update () {
			if (Input.GetButtonDown("Exit") || Input.GetKeyDown(KeyCode.Escape)) {
				audioManager.PlaySound("Select");
				menuSwitcher.LoadMenu(1);
			}
		}
	}
}
