using GameManager;
using UnityEngine;

namespace UI
{
	// AI - Note: This is a small UI class and doesn't require extensive regions.
	public class PauseManagement : UserInterfaceInputWrapper
	{
		// AI - Select Pause Menu Buttons.
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

		// AI - Use Cancel function to Resume.
		protected override void Cancel()
		{
			// AI - Don't call base version for the sake of the Select Sound in this case.
			Resume();
		}

		// AI - Use Exit function to Resume.
		protected override void Exit()
		{
			// AI - Don't call base version for the sake of the Select Sound in this case.
			Resume();
		}

		// AI - Unpause the game and return to gameplay.
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
