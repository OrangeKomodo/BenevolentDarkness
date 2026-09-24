
using UI;

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

		private int _failureCauseIndex = 0;

		public int FailureCauseIndex
		{
			get => _failureCauseIndex;
			set
			{
				if(_failureCauseIndex == value)
				{
					return;
				}
				
				_missionFailedManagement.SetCause(value);
				_failureCauseIndex = value;
			}
		}

		private MissionFailedManagement _missionFailedManagement;
		private StatsQuickSave _statsQuickSave;

		private void Start()
		{
			// TODO: Find this a better way later
			_missionFailedManagement = FindAnyObjectByType(typeof(MissionFailedManagement)) as MissionFailedManagement;
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
