using GameManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
	public class MissionCompleteManagement : Singleton<MissionCompleteManagement>
	{
		public Text EnemiesKilledText;
		public Text TimesSpottedText;
		public Toggle PacifistToggle;
		public Toggle ShadowToggle;
		public Text WantedLevelText;
		public Text RetryText;
		public Text MainMenuText;
		
		public int NumberOfGuards;

		private int _enemiesKilled;
		private int _timesSpotted;

		private void Start()
		{
			if (Input.GetJoystickNames().Length > 0)
			{
				RetryText.text = "[A] Retry";
				MainMenuText.text = "[START] Main Menu";
			}

			EnemiesKilledText.text = "";
			TimesSpottedText.text = "";
			PacifistToggle.isOn = false;
			ShadowToggle.isOn = false;
			WantedLevelText.text = "";
		}

		private void OnEnable()
		{
			if (MoralitySystem.Instance == null)
			{
				return;
			}
			
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

			_enemiesKilled = MoralitySystem.Instance.EnemiesKilled;
			_timesSpotted = MoralitySystem.Instance.TimesSpotted;

			EnemiesKilledText.text = _enemiesKilled.ToString();
			TimesSpottedText.text = _timesSpotted.ToString();
			PacifistToggle.isOn = _enemiesKilled == 0;
			ShadowToggle.isOn = _timesSpotted == 0;
			WantedLevelText.text = _enemiesKilled > NumberOfGuards / 2 ? "High" : "Low";
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl) || Input.GetButtonDown("Jump"))
			{
				AudioManager.Instance.PlaySound("Select");
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			}
			else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetButtonDown("Cancel"))
			{
				AudioManager.Instance.PlaySound("Select");
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				SceneManager.LoadScene("Main Menu");
			}
		}
	}
}
