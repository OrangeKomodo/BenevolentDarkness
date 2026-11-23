using GameManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
	public class MissionCompleteManagement : UserInterfaceInputWrapper
	{
		#region Fields

		public Text EnemiesKilledText;
		public Text TimesSpottedText;
		public Toggle PacifistToggle;
		public Toggle ShadowToggle;
		public Text WantedLevelText;
		
		public int NumberOfGuards;

		private int _enemiesKilled;
		private int _timesSpotted;

		#endregion

		#region Initialization Functions

		// AI - Wipe any text or settings that the game may or may not start with.
		protected override void Awake()
		{
			base.Awake();
			
			EnemiesKilledText.text = "";
			TimesSpottedText.text = "";
			PacifistToggle.isOn = false;
			ShadowToggle.isOn = false;
			WantedLevelText.text = "";
		}

		// AI - If this menu is enabled, the game is over - populate the end-game data.
		protected override void OnEnable()
		{
			base.OnEnable();
			
			if (MoralitySystem.Instance == null)
			{
				return;
			}

			_enemiesKilled = MoralitySystem.Instance.EnemiesKilled;
			_timesSpotted = MoralitySystem.Instance.TimesSpotted;

			UpdateStatistics();
		}

		#endregion

		#region Statistics Functions

		// AI - Update the displayed statistics for mission completion.
		private void UpdateStatistics()
		{
			EnemiesKilledText.text = _enemiesKilled + " / " + NumberOfGuards;
			TimesSpottedText.text = _timesSpotted.ToString();
			PacifistToggle.isOn = _enemiesKilled == 0;
			ShadowToggle.isOn = _timesSpotted == 0;
			WantedLevelText.text = GetWantedLevel();
		}

		// AI - Calculate wanted level based on player's performance.
		private string GetWantedLevel()
		{
			if (_enemiesKilled == 0 && _timesSpotted == 0)
			{
				return "None";
			}

			if (_enemiesKilled == 0 && _timesSpotted < 3)
			{
				return "Low";
			}

			if (_enemiesKilled < 3 && _timesSpotted < 5)
			{
				return "Medium";
			}

			return "High";
		}

		#endregion

		#region Button Handling Functions

		// AI - Select Mission Complete Buttons.
		protected override void SelectButton(int button)
		{
			base.SelectButton(button);

			switch (button)
			{
				case 0:
					ContinueToNextLevel();
					return;
				case 1:
					RestartLevel();
					return;
				case 2:
					ToLevelSelect();
					return;
			}
		}

		// AI - Continue to the next level in the sequence.
		private void ContinueToNextLevel()
		{
			string currentLevel = SceneManager.GetActiveScene().name;
			string nextLevel = GetNextLevel(currentLevel);
			LoadLevel(nextLevel);
		}

		// AI - Determine the next level based on the current level name.
		private string GetNextLevel(string currentLevel)
		{
			return currentLevel switch
			{
				"Level 1" => "Level 2",
				"Level 2" => "Level 3",
				_ => "Main Menu"
			};
		}

		#endregion
	}
}
