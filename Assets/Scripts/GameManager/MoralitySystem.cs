using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatsQuickSave {

	public int enemiesKilled;
	public int timesSpotted;
}

public class MoralitySystem : MonoBehaviour {

	public int enemiesKilled = 0;
	public int timesSpotted = 0;

	public StatsQuickSave statsQuickSave;

	public void QuickSave () {
		statsQuickSave.enemiesKilled = enemiesKilled;
		statsQuickSave.timesSpotted = timesSpotted;
	}

	public void QuickLoad () {
		enemiesKilled = statsQuickSave.enemiesKilled;
		timesSpotted = statsQuickSave.timesSpotted;
	}
}
