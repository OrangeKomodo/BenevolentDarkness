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
		public Transform Floor;
		public bool IsHiding;

		PlayerController player;

		private void Start()
		{
			player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
			Center = transform.GetChild(0);
		}

		public void Hide()
		{
			if (IsHiding)
			{
				Switch(false);
				return;
			}
			
			bool isBeingChased = false;
			int v = 0;
			while (!isBeingChased && v < Floor.childCount)
			{
				Transform floorChild = Floor.GetChild(v);
					
				Guard potentialGuard = floorChild.GetChild(0).GetComponent<Guard>();
				Sentry potentialSentry = floorChild.GetChild(0).GetComponent<Sentry>();
					
				if (potentialGuard?.SuspicionPercentage >= 1f || potentialSentry?.SuspicionPercentage >= 1f)
				{
					isBeingChased = true;
				}
					
				if (!isBeingChased)
				{
					v++;
				}
			}

			if (!isBeingChased)
			{
				player.transform.position = Center.position;
				//player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
				Switch(true);
			}
		}

		private void Switch(bool newState)
		{
			IsHiding = newState;
			player.InHidingPlace(IsHiding);

			for (int i = 0; i < Floor.childCount; i++)
			{
				Transform floorChild = Floor.GetChild(i);
				if (floorChild.name.Contains("Guard"))
				{
					floorChild.GetChild(0).GetComponent<Guard>().PlayerHiding(IsHiding);
				}
				else if (floorChild.name.Contains("Sentry"))
				{
					floorChild.GetChild(0).GetComponent<Sentry>().PlayerHiding(IsHiding);
				}
			}
		}
	}
}
