using GameManager;
using UnityEngine;

namespace UI
{
	public class PauseManagement : UserInterfaceInputWrapper
	{
		// Select Pause Menu Buttons
		protected override void SelectButton(int button)
		{
			base.SelectButton(button);
			
			switch (button)
			{
				case 0:
					Resume();
					return;
				case 1:
					RestartLevel();
					return;
				case 2:
					ToLevelSelect();
					return;
				case 3:
					MainMenu();
					return;
			}
		}

		// Use Cancel function to Resume
		protected override void Cancel()
		{
			// Don't call base version for the sake of the Select Sound in this case
			Resume();
		}

		// Use Exit function to Resume
		protected override void Exit()
		{
			// Don't call base version for the sake of the Select Sound in this case
			Resume();
		}

		// Unpause the game
		public void Resume()
		{
			AudioManager.Instance.PlaySound("Select");
			AudioManager.Instance.PauseSound("Alarm", false);
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			Time.timeScale = 1;
			MenuSwitcher.Instance.LoadMenu(0);
		}
	}
}
