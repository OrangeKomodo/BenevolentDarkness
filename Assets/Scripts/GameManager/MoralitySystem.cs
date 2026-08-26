
namespace GameManager
{
	public class MoralitySystem : Singleton<MoralitySystem>
	{

		[System.Serializable]
		public class StatsQuickSave
		{

			public int EnemiesKilled;
			public int TimesSpotted;
		}

		public int EnemiesKilled = 0;
		public int TimesSpotted = 0;

		private StatsQuickSave _statsQuickSave;

		private void Start()
		{
			_statsQuickSave = new StatsQuickSave();
		}

		public void QuickSave()
		{
			_statsQuickSave.EnemiesKilled = EnemiesKilled;
			_statsQuickSave.TimesSpotted = TimesSpotted;
		}

		public void QuickLoad()
		{
			EnemiesKilled = _statsQuickSave.EnemiesKilled;
			TimesSpotted = _statsQuickSave.TimesSpotted;
		}
	}
}
