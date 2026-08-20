using GameManager;
using UnityEngine;

namespace UI
{
	public class ControlsManagment : MonoBehaviour {

		public MenuSwitcher menuSwitcher;
	
		// Update is called once per frame
		void Update () {
			if (Input.GetKeyDown (KeyCode.Escape)) {
				menuSwitcher.LoadMenu (1);
			}
		}
	}
}
