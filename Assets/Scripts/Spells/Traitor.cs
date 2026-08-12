using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Traitor : Spell {

	public float maxTransferDistance;

	public Color canCorrupt = Color.white;
	public Color canNotCorrupt = Color.gray;

	GameObject traitorMarker;
	GameObject player;
	PlayerInfo playerInfo;
	SpriteRenderer spriteRenderer;

	bool canTransfer = false;

	bool usingController;

	void Start () {
		traitorMarker = gameObject.transform.GetChild (0).gameObject;
		player = GameObject.FindGameObjectWithTag ("Player");
		playerInfo = player.GetComponent<PlayerInfo> ();
		spriteRenderer = traitorMarker.GetComponent<SpriteRenderer> ();
		usingController = Input.GetJoystickNames ().Length > 0;

		playerInfo.canAttack = false;

        if (!usingController) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
	}

	void FixedUpdate () {
			traitorMarker.SetActive (true);

			Vector2 mouseRay;
			if (usingController) {
				transform.Translate (new Vector3 (Input.GetAxis ("Mouse X") * transform.parent.localScale.x, Input.GetAxis ("Mouse Y"), 0f));
				mouseRay = transform.position; 
			} else
				mouseRay = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			
			RaycastHit2D mouseRayHit = Physics2D.Raycast (mouseRay, Vector2.zero, 100f);
			RaycastHit2D playerRayHit;
			LayerMask layerMask = LayerMask.GetMask ("Enemies", "Platforms", "Walls");

		if (mouseRayHit) {
			Vector3 targetPosition = mouseRayHit.point;
			Vector3 playerPosition = player.transform.position;

			Debug.DrawRay (playerPosition, (targetPosition - playerPosition).normalized * Mathf.Clamp (Vector2.Distance (playerPosition, targetPosition), 0f, maxTransferDistance), Color.blue);
			playerRayHit = Physics2D.Raycast (playerPosition, targetPosition - playerPosition, Mathf.Clamp (Vector2.Distance (playerPosition, targetPosition), 0f, maxTransferDistance), layerMask);

			if (playerRayHit.collider == null && !(mouseRayHit.collider.gameObject.layer == 9 && playerRayHit.collider.gameObject.layer == 9)) {
				if (canTransfer)
					canTransfer = false;

				traitorMarker.transform.position = mouseRayHit.point;
			} else {
				if ("Guard Actual Backside".Contains (playerRayHit.collider.name)) {
					if (!canTransfer)
						canTransfer = true;

					//Debug.Log (playerRayHit.collider.name);
					Transform guard = playerRayHit.collider.name.Equals ("Backside") ? playerRayHit.collider.transform.parent : playerRayHit.transform;
					traitorMarker.transform.position = guard.GetChild (4).position;
					
					//Debug.Log (playerRayHit.distance);

					if (canTransfer && Input.GetAxis ("Attack") == 1f) {
                        playerInfo.PlaySound("Traitor");

						traitorMarker.SetActive (false);

						//CORRUPT GUARD HERE
						guard.GetComponent<Guard> ().Corrupt ();
						player.GetComponent<SpellCasting> ().Corrupted ();

						EndTraitor ();
					}

					spriteRenderer.color = canTransfer ? canCorrupt : canNotCorrupt;
				}
			}
		}

		if (Input.GetKeyDown (KeyCode.F) || Input.GetButtonDown ("Exit"))
			EndTraitor ();
	}

	public void EndTraitor () {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		playerInfo.canAttack = true;
		Destroy (gameObject);
	}
}
