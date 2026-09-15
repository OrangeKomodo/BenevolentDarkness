
using Player;
using UnityEngine;

namespace Items
{
	public class HidingPlace : Item
	{
		public enum PlaceType
		{
			UnderTable,
			InWardrobe
		}

		public PlaceType Place;
		public Transform Center;
		public bool IsHiding;

		private PlayerController _playerController;

		private void Start()
		{
			_playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		}

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

		private void SetPlayerIsHiding(bool newState)
		{
			IsHiding = newState;
			_playerController.IsInHidingPlace(IsHiding);
		}
	}
}
