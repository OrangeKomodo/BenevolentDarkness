using GameManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
	public class MissionCompleteManagement : MonoBehaviour
	{

		public int numberOfGuards;

		AudioManager audioManager;
		MoralitySystem moralitySystem;

		int enemiesKilled;
		int timesSpotted;

		void Start()
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

			audioManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<AudioManager>();
			moralitySystem = GameObject.FindGameObjectWithTag("GameController").GetComponent<MoralitySystem>();
			enemiesKilled = moralitySystem.enemiesKilled;
			timesSpotted = moralitySystem.timesSpotted;

			transform.GetChild(2).GetComponent<Text>().text += enemiesKilled;
			transform.GetChild(3).GetComponent<Text>().text += timesSpotted;
			transform.GetChild(4).GetChild(0).GetComponent<Toggle>().isOn = enemiesKilled == 0;
			transform.GetChild(5).GetChild(0).GetComponent<Toggle>().isOn = timesSpotted == 0;
			transform.GetChild(6).GetComponent<Text>().text += enemiesKilled > numberOfGuards / 2 ? "High" : "Low";

			if (Input.GetJoystickNames().Length > 0)
			{
				transform.Find("Retry Text").GetComponent<Text>().text = "[A] Retry";
				transform.Find("Main Menu Text").GetComponent<Text>().text = "[START] Main Menu";
			}
		}

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl) || Input.GetButtonDown("Jump"))
			{
				audioManager.PlaySound("Select");
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			}
			else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetButtonDown("Cancel"))
			{
				audioManager.PlaySound("Select");
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				SceneManager.LoadScene("Main Menu");
			}
		}
	}
}
