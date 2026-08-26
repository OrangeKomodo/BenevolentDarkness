using GameManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
	public class LevelSelectManagement : Singleton<LevelSelectManagement>
	{
		public Button[] Buttons;
		
		public Color Selected = Color.white;
		public Color NotSelected = Color.gray;

		private bool _usingController;

		private int _buttonIndex = 0;
		private float _nextChangeTime;

		void Start()
		{
			_usingController = Input.GetJoystickNames().Length > 0;
			if (_usingController)
			{
				ChangeButton(_buttonIndex);
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
		}

		void Update()
		{
			if (_usingController)
			{
				float controllerY = -Input.GetAxis("Vertical");
				if (_nextChangeTime <= Time.realtimeSinceStartup && Mathf.Abs(controllerY) > 0.19f)
				{
					AudioManager.Instance.PlaySound("Swish");
					_buttonIndex = (4 + (_buttonIndex + (int)(controllerY / Mathf.Abs(controllerY)))) % 4;
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
				LoadMenu(1);
			}
		}

		public void LoadMenu(int menuIndex)
		{
			AudioManager.Instance.PlaySound("Select");
			MenuSwitcher.Instance.LoadMenu(menuIndex);
		}

		public void LoadLevel(string levelName)
		{
			AudioManager.Instance.PlaySound("Select");
			SceneManager.LoadScene(levelName);
		}

		void ChangeButton(int newButton)
		{
			for (int i = 0; i < Buttons.Length; i++)
			{
				Buttons[i].image.color = newButton == i ? Selected : NotSelected;
			}
		}

		void SelectButton(int button)
		{
			if (button == 0)
			{
				LoadLevel("Level 1");
			}
			else if (button == 1)
			{
				LoadLevel("Level 2");
			}
			else if (button == 2)
			{
				LoadLevel("Level 3");
			}
			else if (button == 3)
			{
				LoadMenu(1);
			}
		}
	}
}
