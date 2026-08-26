using Player;
using UnityEngine;

namespace AI.Sentry
{
	public class SentryVision : MonoBehaviour
	{
		Sentry sentry;
		GameObject player;
		PlayerController playerController;

		bool boxVisible = false;
		bool circleVisible = false;

		LayerMask layerMask;

		private void Start()
		{
			sentry = gameObject.transform.parent.GetComponent<Sentry>();
			player = GameObject.FindGameObjectWithTag("Player");
			playerController = player.GetComponent<PlayerController>();
			layerMask = LayerMask.GetMask("Player", "Platforms", "Affected Platforms");
		}

		private void Update()
		{
			if (!playerController.DisguisedAsGuard)
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
						sentry.CheckSeesPlayer(player.GetComponent<PlayerController>().VisibilityFactor);
					}
				}

				if (!boxVisible && !circleVisible)
				{
					sentry.LostPlayer();
				}
			}
		}

		private void OnTriggerEnter2D(Collider2D collider)
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
			}
		}

		void OnTriggerExit2D(Collider2D collider)
		{
			if (collider.tag.Equals("Player"))
			{
				if (collider.Equals(player.GetComponent<BoxCollider2D>()))
				{
					boxVisible = false;
				}
				else if (collider.Equals(player.GetComponent<CircleCollider2D>()))
				{
					circleVisible = false;
				}
			}
		}
	}
}
