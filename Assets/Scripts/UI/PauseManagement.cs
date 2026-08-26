using GameManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
	public class PauseManagement : MonoBehaviour
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
					_buttonIndex = (4 + _buttonIndex + (int)(controllerY / Mathf.Abs(controllerY))) % 4;
					ChangeButton(_buttonIndex);
					_nextChangeTime = Time.realtimeSinceStartup + 0.2f;
				}
			}

			if (Input.GetButtonDown("Jump"))
			{
				SelectButton(_buttonIndex);
			}

			if (Input.GetButtonDown("Cancel"))
			{
				Resume();
			}
		}

		public void Resume()
		{
			AudioManager.Instance.PlaySound("Select");
			AudioManager.Instance.PauseSound("Alarm", false);
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			Time.timeScale = 1;
			MenuSwitcher.Instance.LoadMenu(0);
		}

		public void Restart()
		{
			AudioManager.Instance.PlaySound("Select");
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			Time.timeScale = 1;
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}

		public void ToMainMenu()
		{
			AudioManager.Instance.PlaySound("Select");
			PlayerPrefs.SetInt("MainMenuSetting", 1);
			SceneManager.LoadScene("Main Menu");
		}

		public void ToLevelSelect()
		{
			AudioManager.Instance.PlaySound("Select");
			PlayerPrefs.SetInt("MainMenuSetting", 2);
			SceneManager.LoadScene("Main Menu");
		}

		void ChangeButton(int newButton)
		{
			for (int i = 0; i < 4; i++)
			{
				Buttons[i].image.color = newButton == i ? Selected : NotSelected;
			}
		}

		void SelectButton(int button)
		{
			if (button == 0)
			{
				Resume();
			}
			else if (button == 1)
			{
				Restart();
			}
			else if (button == 2)
			{
				ToLevelSelect();
			}
			else if (button == 3)
			{
				ToMainMenu();
			}
		}
	}
}
