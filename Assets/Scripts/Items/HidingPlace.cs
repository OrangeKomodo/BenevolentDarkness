using Player;
using UnityEngine;

namespace Items
{
	public class HidingPlace : Item
	{
		#region Enums

		public enum PlaceType
		{
			UnderTable,
			InWardrobe
		}

		#endregion

		#region Fields

		public PlaceType Place;
		public Transform Center;
		public bool IsHiding;

		private PlayerController _playerController;

		#endregion

		#region Initialization Functions

		private void Start()
		{
			_playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		}

		#endregion

		#region Interaction Functions

		// AI - Toggle the player's hiding state. Enter hiding by moving to center; exit by leaving.
		public override void Interact()
		{
			if (IsHiding)
			{
				SetPlayerIsHiding(false);
				return;
			}
			
			_playerController.transform.position = Center.position;
			SetPlayerIsHiding(true);
		}

		// AI - Update the hiding state for both this object and the player controller.
		private void SetPlayerIsHiding(bool newState)
		{
			IsHiding = newState;
			_playerController.IsInHidingPlace(IsHiding);
		}

		#endregion
	}
}
