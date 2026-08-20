using AI.Guard;
using AI.Sentry;
using Player;
using UnityEngine;

namespace Items
{
	public class HidingPlace : Item
	{

		public enum Place
		{
			underTable,
			inWardrobe
		}

		public Place place;
		public Transform center;
		public Transform floor;
		public bool isHiding;

		GameObject player;

		void Start()
		{
			player = GameObject.FindGameObjectWithTag("Player");
			center = transform.GetChild(0);
		}

		public void Hide()
		{
			if (!isHiding)
			{
				bool isBeingChased = false;
				int v = 0;
				while (!isBeingChased && v < floor.childCount)
				{
					Transform floorChild = floor.GetChild(v);
					
					Guard potentialGuard = floorChild.GetChild(0).GetComponent<Guard>();
					Sentry potentialSentry = floorChild.GetChild(0).GetComponent<Sentry>();
					
					if (potentialGuard?.suspicionPercentage >= 1f || potentialSentry?.suspicionPercentage >= 1f)
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
					player.transform.position = center.position;
					player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
					Switch(true);
				}
			}
			else
			{
				Switch(false);
			}
		}

		void Switch(bool newState)
		{
			isHiding = newState;
			player.GetComponent<PlayerInfo>().InHidingPlace(isHiding);

			for (int i = 0; i < floor.childCount; i++)
			{
				Transform floorChild = floor.GetChild(i);
				if (floorChild.name.Contains("Guard"))
				{
					floorChild.GetChild(0).GetComponent<Guard>().PlayerHiding(isHiding);
				}
				else if (floorChild.name.Contains("Sentry"))
				{
					floorChild.GetChild(0).GetComponent<Sentry>().PlayerHiding(isHiding);
				}
			}
		}
	}
}
