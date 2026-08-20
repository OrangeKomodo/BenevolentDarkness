using Player;
using UnityEngine;

namespace Spells
{
	public class Translocation : Spell
	{
		public float maxDistance;
		public bool translocationOccured;
		public Color canTranslocate = Color.white;
		public Color canNotTranslocate = Color.gray;

		GameObject translocationMarker;
		GameObject player;
		PlayerController playerController;
		SpriteRenderer spriteRenderer;

		bool positionValid;
		bool hittingPlatform;
		Vector2 normal;

		bool usingController;

		void Start()
		{
			maxDistance = FindObjectOfType<SpellCasting>().spellLevel * 5f + 5f;
			translocationMarker = gameObject.transform.GetChild(0).gameObject;
			player = GameObject.FindGameObjectWithTag("Player");
			playerController = player.GetComponent<PlayerController>();
			spriteRenderer = translocationMarker.GetComponent<SpriteRenderer>();
			usingController = Input.GetJoystickNames().Length > 0;

			if (!usingController)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}

		void Update()
		{
			if (Input.GetAxis("Use Item") == 1f)
			{
				translocationMarker.SetActive(true);

				Vector2 mouseRay;
				if (usingController)
				{
					transform.Translate(new Vector3(Input.GetAxis("Mouse X") * transform.parent.localScale.x,
						Input.GetAxis("Mouse Y"), 0f));
					mouseRay = transform.position;
				}
				else
				{
					mouseRay = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				}

				RaycastHit2D mouseRayHit = Physics2D.Raycast(mouseRay, Vector2.zero, 100f);
				RaycastHit2D playerRayHit;
				LayerMask layerMask = LayerMask.GetMask("Platforms", "Walls");

				if (mouseRayHit)
				{
					Vector3 targetPosition = mouseRayHit.point;
					Vector3 playerPosition = player.transform.position;

					Debug.DrawRay(playerPosition,
						(targetPosition - playerPosition).normalized
						* Mathf.Clamp(Vector2.Distance(playerPosition, targetPosition), 0f, maxDistance), Color.red);
					playerRayHit = Physics2D.Raycast(playerPosition, targetPosition - playerPosition,
						Mathf.Clamp(Vector2.Distance(playerPosition, targetPosition), 0f, maxDistance), layerMask);

					if (playerRayHit.collider == null)
					{
						transform.position = mouseRayHit.point;
						positionValid = Vector2.Distance(playerPosition, targetPosition) <= maxDistance;
					}
					else if (playerRayHit.collider.gameObject.layer == 11 || playerRayHit.collider.gameObject.layer == 13)
					{
						transform.position = playerRayHit.point;
						normal = playerRayHit.normal;
						positionValid = true;
						hittingPlatform = true;
					}
					//Debug.Log (playerRayHit.distance);
				}

				spriteRenderer.color = positionValid ? canTranslocate : canNotTranslocate;
				translocationMarker.transform.Rotate(Vector3.forward);
			}

			if (Input.GetAxis("Use Item") == 0f)
			{
				if (positionValid)
				{
					playerController.PlaySound("Translocation");

					translocationOccured = true;

					if ((translocationMarker.transform.position.x - player.transform.position.x) * player.transform.localScale.x < 0)
					{
						player.GetComponent<PlayerController>().Flip();
					}

					if (hittingPlatform && normal.x == 0)
					{
						player.transform.position = transform.GetChild(normal.y == 1 ? 1 : 2).position;
					}
					else
					{
						player.transform.position = translocationMarker.transform.position;
					}
				}

				EndTranslocation();
			}

			if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Exit"))
			{
				EndTranslocation();
			}
		}

		void EndTranslocation()
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			player.GetComponent<SpellCasting>().EndSpell(SpellCasting.SpellNames.translocation);
		}
	}
}

/*
 * 		if (Input.GetMouseButton (1)) {
			translocationMarker.SetActive (true);

			Vector2 mouseRay = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			RaycastHit2D mouseRayHit = Physics2D.Raycast (mouseRay, Vector2.zero, 100f);
			RaycastHit2D playerRayHit;
			LayerMask layerMask = LayerMask.GetMask ("Platforms", "Walls");

			if (mouseRayHit) {
				Vector3 targetPosition = mouseRayHit.point;
				Vector3 playerPosition = player.transform.position;

				Debug.DrawRay (playerPosition, (targetPosition - playerPosition).normalized * Mathf.Clamp (Vector2.Distance (playerPosition, targetPosition), 0f, maxDistance), Color.red);
				playerRayHit = Physics2D.Raycast (playerPosition, targetPosition - playerPosition, Mathf.Clamp (Vector2.Distance (playerPosition, targetPosition), 0f, maxDistance), layerMask);

				if (playerRayHit.collider == null) {
					translocationMarker.transform.position = mouseRayHit.point;
					positionValid = Vector2.Distance (playerPosition, targetPosition) <= maxDistance;
				} else if (playerRayHit.collider.gameObject.layer == 11) {
					translocationMarker.transform.position = playerRayHit.point;
					normal = playerRayHit.normal;
					positionValid = true;
					hittingPlatform = true;
				}
				//Debug.Log (playerRayHit.distance);
			}
			spriteRenderer.color = positionValid ? canTranslocate : canNotTranslocate;
			translocationMarker.transform.Rotate (Vector3.forward);
		}

		if (Input.GetMouseButtonUp (1)) {
			if (positionValid) {
				translocationOccured = true;

				if ((translocationMarker.transform.position.x - player.transform.position.x) * player.transform.localScale.x < 0)
					player.GetComponent<PlayerInfo> ().Flip ();

				if (hittingPlatform && normal.x == 0)
					player.transform.position = translocationMarker.transform.GetChild (normal.y == 1 ? 0 : 1).position;
				else
					player.transform.position = translocationMarker.transform.position;
			}

			EndTranslocation ();
		}

		if (Input.GetKeyDown (KeyCode.F)) {
			EndTranslocation ();
		}
		*/
