using GameManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
	public class MissionFailedManagement : Singleton<MissionFailedManagement>
	{
		public Text CauseText;
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
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
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
					_buttonIndex = (3 + _buttonIndex + (int)(controllerY / Mathf.Abs(controllerY))) % 3;
					ChangeButton(_buttonIndex);
					_nextChangeTime = Time.realtimeSinceStartup + 0.2f;
				}
			}

			if (Input.GetButtonDown("Jump"))
			{
				SelectButton(_buttonIndex);
			}
		}

		private void OnEnable()
		{
			if (AudioManager.Instance == null)
			{
				return;
			}
			
			AudioManager.Instance.StopSound("Alarm");
		}

		public void SetCause(int causeIndex)
		{
			string causeText = "";

			switch (causeIndex)
			{
				case 0:
				{
					causeText = "You Died!";
					break;
				}
			}

			CauseText.text = causeText;
		}

		public void LoadLastSave()
		{
			QuickSaveSystem.Instance.QuickLoadAll();
		}

		public void RestartLevel()
		{
			AudioManager.Instance.PlaySound("Select");
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}

		public void MainMenu()
		{
			AudioManager.Instance.PlaySound("Select");
			SceneManager.LoadScene("Main Menu");
		}

		public void QuitGame()
		{
			AudioManager.Instance.PlaySound("Select");
			Application.Quit();
		}

		private void ChangeButton(int newButton)
		{
			for (int i = 0; i < Buttons.Length; i++)
			{
				Buttons[i].image.color = newButton == i ? Selected : NotSelected;
			}
		}

		private void SelectButton(int button)
		{
			if (button == 0)
			{
				RestartLevel();
			}
			else if (button == 1)
			{
				MainMenu();
			}
			else if (button == 2)
			{
				QuitGame();
			}
		}
	}
}
