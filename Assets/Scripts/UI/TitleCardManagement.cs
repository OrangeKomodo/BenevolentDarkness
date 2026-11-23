using GameManager;
using UnityEngine;

namespace UI
{
	// AI - Note: This is a small UI class and doesn't require regions.
	public class TitleCardManagement : UserInterfaceInputWrapper
	{
		// AI - Check for any button press to advance from title screen.
		protected override void Update()
		{
			base.Update();
			
			if (Input.anyKeyDown)
			{
				MenuSwitcher.Instance.LoadMenu(1);
				AudioManager.Instance.PlaySound("Select");
			}
		}
	}
}
