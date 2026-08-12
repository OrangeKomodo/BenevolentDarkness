using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickSave : MonoBehaviour {

	public Transform player;
	public Transform enemiesHolder;
	public Transform itemsHolder;

	MoralitySystem moralitySystem;
	MenuSwitcher menuSwitcher;
	PlayerInfo playerInfo;
	Guard[] guards;
	Sentry[] sentries;
	ItemManager itemManager;

	bool started = false;

	void Start () {
		moralitySystem = GetComponent<MoralitySystem> ();
		menuSwitcher = GetComponent<MenuSwitcher> ();
		playerInfo = player.GetComponent<PlayerInfo> ();
		guards = enemiesHolder.GetComponentsInChildren<Guard> ();
		sentries = enemiesHolder.GetComponentsInChildren<Sentry> ();
		itemManager = itemsHolder.GetComponent<ItemManager> ();

		//QuickSaveAll ();
	}

	void Update () {
		if (Time.time > 0.1f && !started) {
			QuickSaveAll ();
			started = true;
		}

		if (Input.GetKeyDown (KeyCode.F5)) {
			if (playerInfo.GetStatus () && menuSwitcher.selectedMenu == 0) {
				bool isSeen = false;
				for (int i = 0; i < guards.Length && !isSeen; i++)
					if (!isSeen)
						isSeen = guards [i].suspicionPercentage > 0f;
				for (int i = 0; i < sentries.Length; i++)
					if (!isSeen)
						isSeen = sentries [i].suspicionPercentage > 0f;
				if (!isSeen)
					QuickSaveAll ();
			}
		} else if (Input.GetKeyDown (KeyCode.F9))
			QuickLoadAll ();
	}

	void QuickSaveAll () {
		moralitySystem.QuickSave ();
		playerInfo.QuickSave ();
		for (int i = 0; i < guards.Length; i++)
			guards [i].QuickSave ();
		for (int i = 0; i < sentries.Length; i++)
			sentries [i].QuickSave ();
		itemManager.QuickSave ();
	}

	public void QuickLoadAll () {
		menuSwitcher.LoadMenu (0);
		moralitySystem.QuickLoad ();
		playerInfo.QuickLoad ();
		for (int i = 0; i < guards.Length; i++)
			guards [i].QuickLoad ();
		for (int i = 0; i < sentries.Length; i++)
			sentries [i].QuickLoad ();
		itemManager.QuickLoad ();
	}
}
