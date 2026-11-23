using GameManager;
using UnityEngine.UI;

namespace UI
{
	public class MissionFailedManagement : UserInterfaceInputWrapper
	{
		#region Fields

		public Text CauseText;

		#endregion

		#region Initialization Functions

		// AI - Stop the alarm sound that may be playing on Mission Failure.
		protected override void OnEnable()
		{
			base.OnEnable();
			
			if (AudioManager.Instance != null)
			{
				AudioManager.Instance.StopSound("Alarm");
			}
		}

		#endregion

		#region UI Update Functions

		// AI - Set the cause of Mission Failure text from the Morality System.
		public void SetCause(int causeIndex)
		{
			string causeText = causeIndex switch
			{
				0 => "You Died!",
				_ => "Mission Failed!"
			};

			CauseText.text = causeText;
		}

		#endregion

		#region Button Handling Functions

		// AI - Select Mission Failed Buttons.
		protected override void SelectButton(int button)
		{
			base.SelectButton(button);

			switch (button)
			{
				case 0:
					LoadLastSave();
					return;
				case 1:
					RestartLevel();
					return;
				case 2:
					ToLevelSelect();
					return;
			}
		}

		// AI - Load last Quick Save.
		private void LoadLastSave()
		{
			if (QuickSaveSystem.Instance != null)
			{
				QuickSaveSystem.Instance.QuickLoadAll();
			}
		}

		#endregion
	}
}
