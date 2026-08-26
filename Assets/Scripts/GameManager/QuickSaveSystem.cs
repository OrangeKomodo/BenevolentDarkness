using AI.Guard;
using AI.Sentry;
using Items;
using Player;
using UnityEngine;

namespace GameManager
{
	public class QuickSaveSystem : Singleton<QuickSaveSystem>
	{
		public Transform Player;
		public Transform EnemiesHolder;
		public Transform ItemsHolder;

		private PlayerController _playerController;
		private Guard[] _guards;
		private Sentry[] _sentries;
		private ItemManager _itemManager;

		private bool _started = false;

		void Start()
		{
			_playerController = Player.GetComponent<PlayerController>();
			_guards = EnemiesHolder.GetComponentsInChildren<Guard>();
			_sentries = EnemiesHolder.GetComponentsInChildren<Sentry>();
			_itemManager = ItemsHolder.GetComponent<ItemManager>();

			//QuickSaveAll ();
		}

		private void Update()
		{
			if (Time.time > 0.1f && !_started)
			{
				QuickSaveAll();
				_started = true;
			}

			if (Input.GetKeyDown(KeyCode.F5))
			{
				if (_playerController.GetStatus() && MenuSwitcher.Instance.SelectedMenu == 0)
				{
					bool isSeen = false;
					for (int i = 0; i < _guards.Length && !isSeen; i++)
					{
						if (!isSeen)
						{
							isSeen = _guards[i].SuspicionPercentage > 0f;
						}
					}

					for (int i = 0; i < _sentries.Length; i++)
					{
						if (!isSeen)
						{
							isSeen = _sentries[i].SuspicionPercentage > 0f;
						}
					}

					if (!isSeen)
					{
						QuickSaveAll();
					}
				}
			}
			else if (Input.GetKeyDown(KeyCode.F9))
			{
				QuickLoadAll();
			}
		}

		private void QuickSaveAll()
		{
			MoralitySystem.Instance.QuickSave();
			_playerController.QuickSave();
			
			for (int i = 0; i < _guards.Length; i++)
			{
				_guards[i].QuickSave();
			}

			for (int i = 0; i < _sentries.Length; i++)
			{
				_sentries[i].QuickSave();
			}
			
			_itemManager.QuickSave();
		}

		public void QuickLoadAll()
		{
			MenuSwitcher.Instance.LoadMenu(0);
			MoralitySystem.Instance.QuickLoad();
			_playerController.QuickLoad();
			
			for (int i = 0; i < _guards.Length; i++)
			{
				_guards[i].QuickLoad();
			}

			for (int i = 0; i < _sentries.Length; i++)
			{
				_sentries[i].QuickLoad();
			}
			
			_itemManager.QuickLoad();
		}
	}
}
