using AI.Guard;
using AI.Sentry;
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

		private PlayerController _player;

		private void Start()
		{
			_player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		}

		public void Hide()
		{
			if (IsHiding)
			{
				SetPlayerIsHiding(false);
				return;
			}
			
			_player.transform.position = Center.position;
			SetPlayerIsHiding(true);
		}

		private void SetPlayerIsHiding(bool newState)
		{
			IsHiding = newState;
			_player.IsInHidingPlace(IsHiding);
		}
	}
}
