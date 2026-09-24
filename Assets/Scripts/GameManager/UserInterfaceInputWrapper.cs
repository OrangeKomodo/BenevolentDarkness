using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameManager
{
    public abstract class UserInterfaceInputWrapper : InputModeListener
    {
        public Button[] Buttons;
        public List<int> SpecialButtonIndexes = new List<int>();

        public Color Selected = Color.white;
        public Color NotSelected = Color.gray;
        public Color SpecialSelected = Color.white;
        public Color SpecialNotSelected = Color.gray;

        private int _buttonCount = 0;
        private int _activeButtonIndex = 0;
        private float _nextChangeTime;

        protected virtual void Start()
        {
            // Save the button length on start
            _buttonCount = Buttons.Length;
        }

        // Use Update loop to handle common inputs
        protected virtual void Update()
        {
            if (UsingGamepad)
            {
                HandleGamepadInput();
            }

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

        #region Overridable Functions

        // Called from Update to handle Gamepad input
        protected virtual void HandleGamepadInput()
        {
            // If there are no menu buttons, we don't care
            if (_buttonCount == 0)
            {
                return;
            }

            // Cycle through menu buttons and highlight the currently selected one
            float gamepadYAxis = -Input.GetAxis("Vertical");
            if (_nextChangeTime <= Time.realtimeSinceStartup && Mathf.Abs(gamepadYAxis) > 0.19f)
            {
                PlayChangeSelectionSound();

                _activeButtonIndex = (_activeButtonIndex + (int)(gamepadYAxis / Mathf.Abs(gamepadYAxis)) + _buttonCount) % _buttonCount;
                HighlightButton(_activeButtonIndex);

                _nextChangeTime = Time.realtimeSinceStartup + 0.2f;
            }
        }

        // Select Button overridable function
        protected virtual void SelectButton(int button)
        {
            PlaySelectSound();
        }

        // Cancel overridable function
        protected virtual void Cancel()
        {
            PlaySelectSound();
        }

        // Exit overridable function
        protected virtual void Exit()
        {
            PlaySelectSound();
        }

        #endregion
        
        #region Public Button Functions

        public void LoadMenu(int menuIndex)
        {
            PlaySelectSound();
            MenuSwitcher.Instance.LoadMenu(menuIndex);
        }

        public void LoadLevel(string levelName)
        {
            PlaySelectSound();
            SceneManager.LoadScene(levelName);
        }

        public void RestartLevel()
        {
            PlaySelectSound();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void MainMenu()
        {
            PlaySelectSound();
            PlayerPrefs.SetInt("MainMenuSetting", 1);
            SceneManager.LoadScene("Main Menu");
        }

        public void ToLevelSelect()
        {
            PlaySelectSound();
            PlayerPrefs.SetInt("MainMenuSetting", 2);
            SceneManager.LoadScene("Main Menu");
        }

        public void QuitGame()
        {
            PlaySelectSound();
            Application.Quit();
        }
        
        #endregion

        #region Helper Functions

        // Highlight the given button - use -1 to highlight all of them
        private void HighlightButton(int newButton)
        {
            for (int buttonIndex = 0; buttonIndex < _buttonCount; ++buttonIndex)
            {
                bool buttonIsSpecial = SpecialButtonIndexes.Contains(buttonIndex);
                Color selectedColor = buttonIsSpecial ? SpecialSelected : Selected;
                Color notSelectedColor = buttonIsSpecial ? SpecialNotSelected : NotSelected;

                bool isButton = newButton == -1 || newButton == buttonIndex;
                Buttons[buttonIndex].image.color = isButton ? selectedColor : notSelectedColor;
            }
        }

        private void PlayChangeSelectionSound() => AudioManager.Instance.PlaySound("Swish");

        private void PlaySelectSound() => AudioManager.Instance.PlaySound("Select");

        #endregion

        #region InputModeListener Functions

        // Update the Input Mode for the menus
        protected override void UpdateInputMode(InputManager.InputMode inputMode)
        {
            // If using Keyboard and Mouse, unlock the cursor and re-highlight the menu's buttons
            if (inputMode == InputManager.InputMode.Keyboard)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                HighlightButton(-1);

                return;
            }

            // If using Gamepad, lock the cursor and only highlight the active button
            if (inputMode == InputManager.InputMode.Gamepad)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                HighlightButton(_activeButtonIndex);

                return;
            }
        }

        #endregion
    }
}
