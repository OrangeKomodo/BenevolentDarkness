using GameManager;
using UnityEngine;

namespace UI
{
	public class TitleCardManagement : UserInterfaceInputWrapper
	{
		// Using Update to check for any button press
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
