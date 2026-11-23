using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameManager
{
	// AI - Note: This is a small utility class and doesn't require extensive regions.
	public class MenuSwitcher : Singleton<MenuSwitcher>
	{
		public Transform Canvas;
		public int SelectedMenu = 0;

		private const string MAIN_MENU_SCENE_NAME = "Main Menu";
		private const string MAIN_MENU_SETTING_KEY = "MainMenuSetting";

		// AI - Initialize the menu system and load the appropriate starting menu.
		private void Start()
		{
			Time.timeScale = 1f;
			
			if (SceneManager.GetActiveScene().name.Equals(MAIN_MENU_SCENE_NAME))
			{
				LoadMenu(PlayerPrefs.GetInt(MAIN_MENU_SETTING_KEY, 0));
				PlayerPrefs.DeleteKey(MAIN_MENU_SETTING_KEY);
				return;
			}
			
			LoadMenu(SelectedMenu);
		}

		// AI - Switch to a specific menu by index, activating only that menu's canvas child.
		public void LoadMenu(int newMenu)
		{
			SelectedMenu = newMenu;
			int menuIndex = 0;
			
			foreach (Transform menu in Canvas)
			{
				menu.gameObject.SetActive(menuIndex == SelectedMenu);
				menuIndex++;
			}
		}
	}
}
