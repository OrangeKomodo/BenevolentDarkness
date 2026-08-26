using GameManager;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class MainMenuManager : MonoBehaviour
	{
		public Button[] Buttons;

		public Color Selected = Color.white;
		public Color NotSelected = Color.gray;

		private bool _usingController;

		private int _buttonIndex = 0;
		private float _nextChangeTime;

		private void Start()
		{
			_usingController = Input.GetJoystickNames().Length > 0;
			if (_usingController)
			{
				ChangeButton(_buttonIndex);
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
		}

		private void Update()
		{
			if (_usingController)
			{
				float controllerY = -Input.GetAxis("Vertical");
				if (_nextChangeTime <= Time.realtimeSinceStartup && Mathf.Abs(controllerY) > 0.19f)
				{
					AudioManager.Instance.PlaySound("Swish");
					_buttonIndex = (3 + (_buttonIndex + (int)(controllerY / Mathf.Abs(controllerY)))) % 3;
					ChangeButton(_buttonIndex);
					_nextChangeTime = Time.realtimeSinceStartup + 0.2f;
				}
			}

			if (Input.GetButtonDown("Jump"))
			{
				SelectButton(_buttonIndex);
			}

			if (Input.GetButtonDown("Exit"))
			{
				LoadMenu(0);
			}
		}

		public void LoadMenu(int menuIndex)
		{
			AudioManager.Instance.PlaySound("Select");
			MenuSwitcher.Instance.LoadMenu(menuIndex);
		}

		public void QuitGame()
		{
			AudioManager.Instance.PlaySound("Select");
			Application.Quit();
		}

		private void ChangeButton(int newButton)
		{
			for (int i = 0; i < 3; i++)
			{
				Buttons[i].image.color = newButton == i ? Selected : NotSelected;
			}
		}

		private void SelectButton(int button)
		{
			if (button == 0)
			{
				LoadMenu(2);
			}
			else if (button == 1)
			{
				LoadMenu(3);
			}
			else if (button == 2)
			{
				QuitGame();
			}
		}
	}
}
