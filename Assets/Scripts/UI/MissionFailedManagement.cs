using GameManager;
using UnityEngine.UI;

namespace UI
{
	public class MissionFailedManagement : UserInterfaceInputWrapper
	{
		public Text CauseText;

		// Stop the alarm sound that may be playing on Mission Failure
		protected override void OnEnable()
		{
			base.OnEnable();
			
			AudioManager.Instance?.StopSound("Alarm");
		}

		// Set the cause of Mission Failure text from the Morality System
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

		// Select Mission Failed Buttons
		protected override void SelectButton(int button)
		{
			switch (button)
			{
				case 0:
					RestartLevel();
					return;
				case 1:
					MainMenu();
					return;
				case 2:
					QuitGame();
					return;
			}
		}

		// Load last Quick Save
		private void LoadLastSave()
		{
			QuickSaveSystem.Instance.QuickLoadAll();
		}
	}
}
