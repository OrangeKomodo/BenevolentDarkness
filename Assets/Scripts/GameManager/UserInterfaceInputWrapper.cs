using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameManager
{
	public abstract class UserInterfaceInputWrapper : InputModeListener
	{
		#region Fields

		public Button[] Buttons;
		public List<int> SpecialButtonIndexes = new List<int>();

		public Color Selected = Color.white;
		public Color NotSelected = Color.gray;
		public Color SpecialSelected = Color.white;
		public Color SpecialNotSelected = Color.gray;

		private int _buttonCount = 0;
		private int _activeButtonIndex = 0;
		private float _nextChangeTime;

		private const string MAIN_MENU_SCENE_NAME = "Main Menu";
		private const string MAIN_MENU_SETTING_KEY = "MainMenuSetting";

		#endregion

		#region Initialization Functions

		protected virtual void Start()
		{
			_buttonCount = Buttons.Length;
		}

		#endregion

		#region Update Functions

		// AI - Use Update loop to handle common inputs.
		protected virtual void Update()
		{
			if (UsingGamepad)
			{
				HandleGamepadInput();
			}

			HandleButtonInputs();
		}

		// AI - Handle jump, cancel, and exit button inputs.
		private void HandleButtonInputs()
		{
			if (Input.GetButtonDown("Jump"))
			{
				SelectButton(_activeButtonIndex);
			}

			if (Input.GetButtonDown("Cancel"))
			{
				Cancel();
			}

			if (Input.GetButtonDown("Exit"))
			{
				Exit();
			}
		}

		#endregion

		#region Gamepad Input Functions

		// AI - Handle Gamepad input for cycling through menu buttons.
		protected virtual void HandleGamepadInput()
		{
			if (_buttonCount == 0)
			{
				return;
			}

			// AI - Cycle through menu buttons and highlight the currently selected one.
			float gamepadYAxis = -Input.GetAxis("Vertical");
			if (_nextChangeTime > Time.realtimeSinceStartup || Mathf.Abs(gamepadYAxis) <= 0.19f)
			{
				return;
			}

			PlayChangeSelectionSound();

			int direction = (int)(gamepadYAxis / Mathf.Abs(gamepadYAxis));
			_activeButtonIndex = (_activeButtonIndex + direction + _buttonCount) % _buttonCount;
			HighlightButton(_activeButtonIndex);

			_nextChangeTime = Time.realtimeSinceStartup + 0.2f;
		}

		#endregion

		#region Overridable Functions

		// AI - Select Button overridable function.
		protected virtual void SelectButton(int button)
		{
			PlaySelectSound();
		}

		// AI - Cancel overridable function.
		protected virtual void Cancel()
		{
			PlaySelectSound();
		}

		// AI - Exit overridable function.
		protected virtual void Exit()
		{
			PlaySelectSound();
		}

		#endregion
		
		#region Public Button Functions

		// AI - Switch to a specific menu.
		public void LoadMenu(int menuIndex)
		{
			PlaySelectSound();
			MenuSwitcher.Instance.LoadMenu(menuIndex);
		}

		// AI - Load a specific level by name.
		public void LoadLevel(string levelName)
		{
			PlaySelectSound();
			SceneManager.LoadScene(levelName);
		}

		// AI - Restart the current level.
		public void RestartLevel()
		{
			PlaySelectSound();
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}

		// AI - Return to main menu (credits menu variant).
		public void MainMenu()
		{
			PlaySelectSound();
			PlayerPrefs.SetInt(MAIN_MENU_SETTING_KEY, 1);
			SceneManager.LoadScene(MAIN_MENU_SCENE_NAME);
		}

		// AI - Return to main menu (level select variant).
		public void ToLevelSelect()
		{
			PlaySelectSound();
			PlayerPrefs.SetInt(MAIN_MENU_SETTING_KEY, 2);
			SceneManager.LoadScene(MAIN_MENU_SCENE_NAME);
		}

		// AI - Quit the application.
		public void QuitGame()
		{
			PlaySelectSound();
			Application.Quit();
		}
		
		#endregion

		#region Helper Functions

		// AI - Highlight the given button (use -1 to highlight all of them).
		private void HighlightButton(int newButton)
		{
			for (int buttonIndex = 0; buttonIndex < _buttonCount; ++buttonIndex)
			{
				bool buttonIsSpecial = SpecialButtonIndexes.Contains(buttonIndex);
				Color selectedColor = buttonIsSpecial ? SpecialSelected : Selected;
				Color notSelectedColor = buttonIsSpecial ? SpecialNotSelected : NotSelected;

				bool isActiveButton = newButton == -1 || newButton == buttonIndex;
				Buttons[buttonIndex].image.color = isActiveButton ? selectedColor : notSelectedColor;
			}
		}

		private void PlayChangeSelectionSound() => AudioManager.Instance.PlaySound("Swish");

		private void PlaySelectSound() => AudioManager.Instance.PlaySound("Select");

		#endregion

		#region InputModeListener Functions

		// AI - Update the Input Mode for the menus.
		protected override void UpdateInputMode(InputManager.InputMode inputMode)
		{
			// AI - If using Keyboard and Mouse, unlock the cursor and re-highlight the menu's buttons.
			if (inputMode == InputManager.InputMode.Keyboard)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				HighlightButton(-1);
				return;
			}

			// AI - If using Gamepad, lock the cursor and only highlight the active button.
			if (inputMode == InputManager.InputMode.Gamepad)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
				HighlightButton(_activeButtonIndex);
			}
		}

		#endregion
	}
}
