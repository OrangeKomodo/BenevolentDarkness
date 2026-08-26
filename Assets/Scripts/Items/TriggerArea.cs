using GameManager;
using Player;
using UnityEngine;

namespace Items
{
	public class TriggerArea : MonoBehaviour
	{
		public int FunctionNumber;
		public bool SingleTrigger = false;
		public bool Triggered = false;

		private PlayerController _playerController;

		private bool _playerIn = false;

		private void Start()
		{
			_playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		}

		private void OnTriggerEnter2D(Collider2D otherCollider)
		{
			if (otherCollider.tag.Equals("Player") && !_playerIn)
			{
				_playerIn = true;
				PerformFunction();
			}
		}

		private void OnTriggerExit2D(Collider2D otherCollider)
		{
			if (otherCollider.tag.Equals("Player") && _playerIn)
			{
				_playerIn = false;
			}
		}

		private void PerformFunction()
		{
			if (SingleTrigger && Triggered)
			{
				return;
			}
			
			switch (FunctionNumber)
			{
				case 0:
				{
					//Level 1 End
					if (_playerController.Inventory.Contains("Ledger"))
					{
						ObjectiveSystem.Instance.SetObjectiveStatus(102, Objective.Status.Completed);
						MenuSwitcher.Instance.LoadMenu(2);
						_playerController.Freeze(true);
						Triggered = true;
					}

					break;
				}
				case 1:
				{
					//Level 2 End
					if (_playerController.Inventory.Contains("Chalice"))
					{
						ObjectiveSystem.Instance.SetObjectiveStatus(202, Objective.Status.Completed);
						MenuSwitcher.Instance.LoadMenu(2);
						_playerController.Freeze(true);
						Triggered = true;
					}

					break;
				}
				case 2:
				{
					//Level 3 End
					MenuSwitcher.Instance.LoadMenu(2);
					_playerController.Freeze(true);
					Triggered = true;
					break;
				}
				case 3:
				{
					//Death Area
					_playerController.TakeHit(1000);
					Triggered = true;
					break;
				}
				case 4:
				{
					//Falling Area
					_playerController.IsFalling(true);
					Triggered = true;
					break;
				}
			}
		}
	}
}
