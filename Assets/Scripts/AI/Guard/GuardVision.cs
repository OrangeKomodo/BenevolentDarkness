using System.Collections.Generic;
using Player;
using UnityEngine;

namespace AI.Guard
{
	public class GuardVision : MonoBehaviour
	{
		Guard guard;
		GameObject player;
		PlayerController playerController;

		public bool boxVisible = false;
		public bool circleVisible = false;
		bool inRange = false;
		bool wasInRange = false;

		LayerMask layerMask;

		List<Guard> visibleGuards = new List<Guard>();
		List<Guard> incapacitatedGuards = new List<Guard>();

		void Start()
		{
			guard = gameObject.transform.parent.GetComponent<Guard>();
			player = GameObject.FindGameObjectWithTag("Player");
			playerController = player.GetComponent<PlayerController>();
			layerMask = LayerMask.GetMask("Player", "Platforms", "Effected Platforms");
		}

		void FixedUpdate()
		{
			if (!playerController.disguisedAsGuard)
			{
				if (boxVisible || circleVisible)
				{
					Debug.DrawRay(transform.position,
						(player.transform.position - transform.position).normalized
						* Mathf.Clamp(Vector2.Distance(transform.position, player.transform.position), 0f, 15f),
						Color.yellow);
					RaycastHit2D playerRayHit = Physics2D.Raycast(transform.position,
						player.transform.position - transform.position,
						Mathf.Clamp(Vector2.Distance(transform.position, player.transform.position), 0f, 15f), layerMask);

					if (playerRayHit.collider != null && playerRayHit.collider.tag.Equals("Player"))
					{
						guard.SeesPlayer(player.GetComponent<PlayerController>().visibilityFactor);
					}
				}

				if (!boxVisible && !circleVisible && guard.seesPlayer)
				{
					guard.LostPlayer();
				}

				if (!playerController.inShadowSink && (!wasInRange && inRange || wasInRange && !inRange))
				{
					guard.PlayerInMeleeRange(inRange);
					wasInRange = !wasInRange;
				}
			}

			if (visibleGuards.Count > 0)
			{
				for (int i = 0; i < visibleGuards.Count; i++)
				{
					Guard fellowGuard = visibleGuards[i];
					if ((fellowGuard.state == Guard.State.dead || fellowGuard.state == Guard.State.unconscious)
					    && !incapacitatedGuards.Contains(fellowGuard)
					    && fellowGuard.transform.parent.parent == transform.parent.parent.parent)
					{
						incapacitatedGuards.Add(fellowGuard);
						guard.FoundGuard(fellowGuard);
					}
				}
			}
		}

		void OnTriggerEnter2D(Collider2D collider)
		{
			if (collider.tag.Equals("Player"))
			{
				if (collider.Equals(player.GetComponent<BoxCollider2D>()))
				{
					boxVisible = true;
				}
				else if (collider.Equals(player.GetComponent<CircleCollider2D>()))
				{
					circleVisible = true;
				}
				inRange = GetComponent<CircleCollider2D>().IsTouching(player.GetComponent<BoxCollider2D>())
				          || GetComponent<CircleCollider2D>().IsTouching(player.GetComponent<CircleCollider2D>());
			}

			if (collider.name.Equals("Guard Actual") && !visibleGuards.Contains(collider.GetComponent<Guard>()))
			{
				visibleGuards.Add(collider.GetComponent<Guard>());
			}
		}

		void OnTriggerExit2D(Collider2D collider)
		{
			if (collider.tag.Equals("Player"))
			{
				if (collider.Equals(player.GetComponent<BoxCollider2D>()) && !GetComponent<PolygonCollider2D>()
					    .IsTouching(player.GetComponent<BoxCollider2D>()))
				{
					boxVisible = false;
				}
				else if (collider.Equals(player.GetComponent<CircleCollider2D>()))
				{
					circleVisible = false;
				}
			}

			inRange = GetComponent<CircleCollider2D>().IsTouching(player.GetComponent<BoxCollider2D>())
			          || GetComponent<CircleCollider2D>().IsTouching(player.GetComponent<CircleCollider2D>());

			if (collider.name.Equals("Guard Actual") && visibleGuards.Contains(collider.GetComponent<Guard>()))
			{
				visibleGuards.Remove(collider.GetComponent<Guard>());
			}
		}
	}
}
