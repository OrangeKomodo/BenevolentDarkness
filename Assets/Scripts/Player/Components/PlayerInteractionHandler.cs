using System.Collections.Generic;
using Items;
using UI;
using UnityEngine;

namespace Player.Components
{
	// AI - Manages player interaction with items in the world.
	public class PlayerInteractionHandler : MonoBehaviour
	{
		#region Fields

		private List<Item> _nearbyItems = new List<Item>();
		private bool _canUse = false;

		#endregion

		#region Properties

		public bool HasNearbyItems => _nearbyItems.Count > 0;

		#endregion

		#region Update Functions

		// AI - Check for use button input and interact with nearby items.
		public void ProcessInteraction(PlayerController playerController)
		{
			if (_nearbyItems.Count <= 0)
			{
				if (_canUse)
				{
					InGameManagement.Instance.LoadUseIcon(false);
					_canUse = false;
				}
				return;
			}

			if (!_canUse)
			{
				InGameManagement.Instance.LoadUseIcon(true);
				_canUse = true;
			}

			if (!Input.GetButtonDown("Use"))
			{
				return;
			}

			Item currentItem = _nearbyItems[0];
			ProcessItemUse(currentItem, playerController);
		}

		#endregion

		#region Item Management Functions

		// AI - Execute the appropriate interaction based on the item type.
		private void ProcessItemUse(Item currentItem, PlayerController playerController)
		{
			if (currentItem is MissionItem missionItem)
			{
				missionItem.Interact();
				playerController.Inventory.Add(missionItem.ItemName);
				missionItem.gameObject.SetActive(false);
				return;
			}

			if (currentItem is HidingPlace hidingPlace)
			{
				if (!playerController.DisguisedAsGuard)
				{
					hidingPlace.Interact();
				}
				return;
			}

			currentItem.Interact();
		}

		// AI - Register an item as nearby and interactable.
		public void AddItem(Item item)
		{
			if (item == null || _nearbyItems.Contains(item))
			{
				return;
			}

			_nearbyItems.Add(item);
		}

		// AI - Unregister an item when the player moves away.
		public void RemoveItem(Item item)
		{
			if (item == null || !_nearbyItems.Contains(item))
			{
				return;
			}

			_nearbyItems.Remove(item);
		}

		#endregion

		#region Cleanup Functions

		// AI - Hide use icon when interaction handler is disabled.
		private void OnDisable()
		{
			if (_canUse)
			{
				InGameManagement.Instance.LoadUseIcon(false);
				_canUse = false;
			}
		}

		#endregion
	}
}
