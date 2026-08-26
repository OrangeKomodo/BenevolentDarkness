using GameManager;
using UnityEngine;

namespace UI
{
	public class TitleCardManagement : MonoBehaviour
	{
		private void Start()
		{
			bool usingController = Input.GetJoystickNames().Length > 0;
			if (usingController)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
		}

		private void Update()
		{
			if (Input.anyKeyDown)
			{
				MenuSwitcher.Instance.LoadMenu(1);
				AudioManager.Instance.PlaySound("Select");
			}
		}
	}
}
