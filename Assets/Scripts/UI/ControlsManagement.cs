using GameManager;
using UnityEngine;

namespace UI
{
	public class ControlsManagement : MonoBehaviour {

		public MenuSwitcher MenuSwitcher;
	
		// Update is called once per frame
		private void Update () {
			if (Input.GetKeyDown (KeyCode.Escape)) {
				MenuSwitcher.LoadMenu (1);
			}
		}
	}
}
