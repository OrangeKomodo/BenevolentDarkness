using GameManager;

namespace UI
{
	public class MainMenuManager : UserInterfaceInputWrapper
	{
		// Select Main Menu Buttons
		protected override void SelectButton(int button)
		{
			base.SelectButton(button);

			switch (button)
			{
				case 0:
					LoadMenu(2);
					return;
				case 1:
					LoadMenu(3);
					return;
				case 2:
					QuitGame();
					return;
			}
		}

		// Go back to Title Screen
		protected override void Exit()
		{
			base.Exit();
			
			LoadMenu(0);
		}
	}
}
