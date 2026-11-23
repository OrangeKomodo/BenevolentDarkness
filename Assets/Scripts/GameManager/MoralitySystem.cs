using UI;

namespace GameManager
{
	public class MoralitySystem : Singleton<MoralitySystem>
	{
		#region Structs

		[System.Serializable]
		public class StatsQuickSave
		{
			public int EnemiesKilled;
			public int TimesSpotted;
		}

		#endregion

		#region Fields

		public int EnemiesKilled = 0;
		public int TimesSpotted = 0;

		private int _failureCauseIndex = 0;
		private MissionFailedManagement _missionFailedManagement;
		private StatsQuickSave _statsQuickSave;

		#endregion

		#region Properties

		// AI - Property to track failure cause and update UI when changed.
		public int FailureCauseIndex
		{
			get => _failureCauseIndex;
			set
			{
				if (_failureCauseIndex == value)
				{
					return;
				}
				
				if (_missionFailedManagement != null)
				{
					_missionFailedManagement.SetCause(value);
				}
				_failureCauseIndex = value;
			}
		}

		#endregion

		#region Initialization Functions

		private void Start()
		{
			// AI - TODO: Find this a better way later (dependency injection would be preferred).
			_missionFailedManagement = FindAnyObjectByType(typeof(MissionFailedManagement)) as MissionFailedManagement;
			_statsQuickSave = new StatsQuickSave();
		}

		#endregion

		#region Quick Save Functions

		// AI - Save current morality statistics.
		public void QuickSave()
		{
			_statsQuickSave.EnemiesKilled = EnemiesKilled;
			_statsQuickSave.TimesSpotted = TimesSpotted;
		}

		// AI - Restore previously saved morality statistics.
		public void QuickLoad()
		{
			EnemiesKilled = _statsQuickSave.EnemiesKilled;
			TimesSpotted = _statsQuickSave.TimesSpotted;
		}

		#endregion
	}
}
