using GameManager;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class CreditsManagement : MonoBehaviour
	{
		private void Start()
		{
			if (Input.GetJoystickNames().Length > 0)
			{
				transform.Find("Back Text").GetComponent<Text>().text = "[B] Back";
			}
		}

		private void Update()
		{
			if (Input.GetButtonDown("Exit") || Input.GetKeyDown(KeyCode.Escape))
			{
				AudioManager.Instance.PlaySound("Select");
				MenuSwitcher.Instance.LoadMenu(1);
			}
		}
	}
}
