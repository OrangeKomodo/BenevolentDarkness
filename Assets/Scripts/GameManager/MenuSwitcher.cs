using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameManager
{
	public class MenuSwitcher : Singleton<MenuSwitcher>
	{
		public Transform Canvas;
		public int SelectedMenu = 0;

		private void Start()
		{
			Time.timeScale = 1f;
			if (SceneManager.GetActiveScene().name.Equals("Main Menu"))
			{
				LoadMenu(PlayerPrefs.GetInt("MainMenuSetting", 0));
				PlayerPrefs.DeleteKey("MainMenuSetting");
				return;
			}
			
			LoadMenu(SelectedMenu);
		}

		public void LoadMenu(int newMenu)
		{
			int x = 0;
			SelectedMenu = newMenu;
			foreach (Transform menu in Canvas)
			{
				menu.gameObject.SetActive(x == SelectedMenu);

				x++;
			}
		}
	}
}