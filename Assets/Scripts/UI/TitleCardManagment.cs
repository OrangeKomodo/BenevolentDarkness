using GameManager;
using UnityEngine;

namespace UI
{
	public class TitleCardManagment : MonoBehaviour
	{

		AudioManager audioManager;
		MenuSwitcher menuSwitcher;

		void Start()
		{
			audioManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<AudioManager>();
			menuSwitcher = GameObject.FindGameObjectWithTag("GameController").GetComponent<MenuSwitcher>();
			bool usingController = Input.GetJoystickNames().Length > 0;
			if (usingController)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
		}

		void Update()
		{
			if (Input.anyKeyDown)
			{
				menuSwitcher.LoadMenu(1);
				audioManager.PlaySound("Select");
			}
		}
	}
}
