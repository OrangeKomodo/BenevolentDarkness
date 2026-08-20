using UnityEngine;

namespace GameManager
{
	public class MoralitySystem : MonoBehaviour {

		[System.Serializable]
		public class StatsQuickSave {

			public int enemiesKilled;
			public int timesSpotted;
		}
	
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
}
