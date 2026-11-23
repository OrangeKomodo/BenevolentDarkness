using AI.Guard;
using AI.Sentry;
using Items;
using Player;
using UnityEngine;

namespace GameManager
{
	public class QuickSaveSystem : Singleton<QuickSaveSystem>
	{
		#region Fields

		public Transform Player;
		public Transform EnemiesHolder;
		public Transform ItemsHolder;

		private PlayerController _playerController;
		private Guard[] _guards;
		private Sentry[] _sentries;
		private ItemManager _itemManager;

		private bool _started = false;

		#endregion

		#region Initialization Functions

		private void Start()
		{
			_playerController = Player.GetComponent<PlayerController>();
			_guards = EnemiesHolder.GetComponentsInChildren<Guard>();
			_sentries = EnemiesHolder.GetComponentsInChildren<Sentry>();
			_itemManager = ItemsHolder.GetComponent<ItemManager>();
		}

		#endregion

		#region Update Functions

		private void Update()
		{
			// AI - Create initial save shortly after scene starts.
			if (Time.time > 0.1f && !_started)
			{
				QuickSaveAll();
				_started = true;
			}

			HandleQuickSaveInput();
			HandleQuickLoadInput();
		}

		// AI - Handle F5 quick save input (only if player is alive, in-game, and not spotted).
		private void HandleQuickSaveInput()
		{
			if (!Input.GetKeyDown(KeyCode.F5))
			{
				return;
			}

			if (!_playerController.GetStatus() || MenuSwitcher.Instance.SelectedMenu != 0)
			{
				return;
			}

			if (IsPlayerSpotted())
			{
				return;
			}

			QuickSaveAll();
		}

		// AI - Handle F9 quick load input.
		private void HandleQuickLoadInput()
		{
			if (Input.GetKeyDown(KeyCode.F9))
			{
				QuickLoadAll();
			}
		}

		// AI - Check if the player is currently spotted by any enemy.
		private bool IsPlayerSpotted()
		{
			for (int i = 0; i < _guards.Length; i++)
			{
				if (_guards[i] != null && _guards[i].SuspicionPercentage > GameConstants.NOT_SPOTTED_THRESHOLD)
				{
					return true;
				}
			}

			for (int i = 0; i < _sentries.Length; i++)
			{
				if (_sentries[i] != null && _sentries[i].SuspicionPercentage > GameConstants.NOT_SPOTTED_THRESHOLD)
				{
					return true;
				}
			}

			return false;
		}

		#endregion

		#region Quick Save Functions

		// AI - Save the state of all game systems.
		private void QuickSaveAll()
		{
			MoralitySystem.Instance.QuickSave();
			_playerController.QuickSave();
			
			for (int i = 0; i < _guards.Length; i++)
			{
				if (_guards[i] != null)
				{
					_guards[i].QuickSave();
				}
			}

			for (int i = 0; i < _sentries.Length; i++)
			{
				if (_sentries[i] != null)
				{
					_sentries[i].QuickSave();
				}
			}
			
			_itemManager.QuickSave();
		}

		// AI - Restore the state of all game systems.
		public void QuickLoadAll()
		{
			MenuSwitcher.Instance.LoadMenu(0);
			MoralitySystem.Instance.QuickLoad();
			_playerController.QuickLoad();
			
			for (int i = 0; i < _guards.Length; i++)
			{
				if (_guards[i] != null)
				{
					_guards[i].QuickLoad();
				}
			}

			for (int i = 0; i < _sentries.Length; i++)
			{
				if (_sentries[i] != null)
				{
					_sentries[i].QuickLoad();
				}
			}
			
			_itemManager.QuickLoad();
		}

		#endregion
	}
}
