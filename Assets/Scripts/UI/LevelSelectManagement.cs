using GameManager;

namespace UI
{
	public class LevelSelectManagement : UserInterfaceInputWrapper
	{
		// Select Levels or go back to Main Menu
		protected override void SelectButton(int button)
		{
			base.SelectButton(button);

			switch (button)
			{
				case 0:
					LoadLevel("Level 1");
					return;
				case 1:
					LoadLevel("Level 2");
					return;
				case 2:
					LoadLevel("Level 3");
					return;
				case 3:
					LoadMenu(1);
					return;
			}
		}

		// Go back to Main Menu
		protected override void Exit()
		{
			base.Exit();
			
			LoadMenu(1);
		}
	}
}
