using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SentryVision : MonoBehaviour {

	Sentry sentry;
	GameObject player;
	PlayerInfo playerInfo;

	bool boxVisible = false;
	bool circleVisible = false;

	LayerMask layerMask;

	void Start () {
		sentry = gameObject.transform.parent.GetComponent<Sentry> ();
		player = GameObject.FindGameObjectWithTag ("Player");
		playerInfo = player.GetComponent<PlayerInfo> ();
		layerMask = LayerMask.GetMask ("Player", "Platforms", "Effected Platforms");
	}

	void Update () {
		if (!playerInfo.disguisedAsGuard) {
			if (boxVisible || circleVisible) {
				Debug.DrawRay (transform.position, (player.transform.position - transform.position).normalized * Mathf.Clamp (Vector2.Distance (transform.position, player.transform.position), 0f, 15f), Color.yellow);
				RaycastHit2D playerRayHit = Physics2D.Raycast (transform.position, player.transform.position - transform.position, Mathf.Clamp (Vector2.Distance (transform.position, player.transform.position), 0f, 15f), layerMask);

				if (playerRayHit.collider != null && playerRayHit.collider.tag.Equals ("Player")) {
					sentry.SeesPlayer (player.GetComponent<PlayerInfo> ().visibilityFactor);
				}
			}
			if (!boxVisible && !circleVisible) {
				sentry.LostPlayer ();
			}
		}
	}

	void OnTriggerEnter2D(Collider2D collider){
		if (collider.tag.Equals ("Player")) {
			if (collider.Equals (player.GetComponent<BoxCollider2D> ()))
				boxVisible = true;
			else if (collider.Equals (player.GetComponent<CircleCollider2D> ()))
				circleVisible = true;
		}
	}

	void OnTriggerExit2D(Collider2D collider){
		if (collider.tag.Equals ("Player")) {
			if (collider.Equals (player.GetComponent<BoxCollider2D> ()))
				boxVisible = false;
			else if (collider.Equals (player.GetComponent<CircleCollider2D> ()))
				circleVisible = false;
		}
	}
}
