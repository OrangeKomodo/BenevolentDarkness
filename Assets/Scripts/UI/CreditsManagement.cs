using GameManager;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class CreditsManagement : MonoBehaviour
	{
		private bool _usingController;

		private void Start()
		{
			_usingController = Input.GetJoystickNames().Length > 0;
			if (_usingController)
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
