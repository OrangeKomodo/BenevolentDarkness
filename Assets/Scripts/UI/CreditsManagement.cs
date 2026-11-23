using GameManager;

namespace UI
{
	// AI - Note: This is a small UI class and doesn't require regions.
	public class CreditsManagement : UserInterfaceInputWrapper
	{
		// AI - Go back to Main Menu.
		protected override void Cancel()
		{
			base.Cancel();
			MenuSwitcher.Instance.LoadMenu(1);
		}

		// AI - Go back to Main Menu.
		protected override void Exit()
		{
			base.Exit();
			MenuSwitcher.Instance.LoadMenu(1);
		}
	}
}
