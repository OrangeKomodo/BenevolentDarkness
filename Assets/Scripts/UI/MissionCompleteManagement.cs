using GameManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
	public class MissionCompleteManagement : UserInterfaceInputWrapper
	{
		public Text EnemiesKilledText;
		public Text TimesSpottedText;
		public Toggle PacifistToggle;
		public Toggle ShadowToggle;
		public Text WantedLevelText;
		
		public int NumberOfGuards;

		private int _enemiesKilled;
		private int _timesSpotted;

		// Wipe any text or settings that the game may or may not start with
		protected override void Awake()
		{
			base.Awake();
			
			EnemiesKilledText.text = "";
			TimesSpottedText.text = "";
			PacifistToggle.isOn = false;
			ShadowToggle.isOn = false;
			WantedLevelText.text = "";
		}

		// If this menu is enabled, the game is over - populate the end-game data
		protected override void OnEnable()
		{
			base.OnEnable();
			
			if (MoralitySystem.Instance == null)
			{
				return;
			}

			_enemiesKilled = MoralitySystem.Instance.EnemiesKilled;
			_timesSpotted = MoralitySystem.Instance.TimesSpotted;

			EnemiesKilledText.text = _enemiesKilled.ToString();
			TimesSpottedText.text = _timesSpotted.ToString();
			PacifistToggle.isOn = _enemiesKilled == 0;
			ShadowToggle.isOn = _timesSpotted == 0;
			WantedLevelText.text = _enemiesKilled > NumberOfGuards / 2 ? "High" : "Low";
		}

		// Listen for specific inputs to augment this menu's functions
		protected override void Update()
		{
			base.Update();

			if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
			{
				SelectButton(-1);
			}
			
			if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
			{
				Cancel();
			}
		}

		// Reload the level
		protected override void SelectButton(int button)
		{
			base.SelectButton(button);
			
			// Ignore the button index in this case - it's not needed
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}

		// Go back to Main Menu
		protected override void Cancel()
		{
			base.Cancel();
			
			SceneManager.LoadScene("Main Menu");
		}
	}
}
