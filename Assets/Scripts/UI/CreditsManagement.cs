using GameManager;

namespace UI
{
	public class CreditsManagement : UserInterfaceInputWrapper
	{
		// Go back to Main Menu
		protected override void Cancel()
		{
			base.Cancel();
			
			MenuSwitcher.Instance.LoadMenu(1);
		}

		// Go back to Main Menu
		protected override void Exit()
		{
			base.Exit();
			
			MenuSwitcher.Instance.LoadMenu(1);
		}
	}
}
