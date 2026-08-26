using Player;
using UnityEngine;

namespace Spells
{
	public class Translocation : Spell
	{
		public SpriteRenderer TranslocationMarker;
		
		public float MaxDistance;
		public bool TranslocationOccured;
		public Color CanTranslocate = Color.white;
		public Color CanNotTranslocate = Color.gray;

		private bool _positionValid;
		private bool _hittingPlatform;
		private Vector2 _normal;

		private bool _usingController;

		private void Start()
		{
			MaxDistance = SpellCaster.SpellLevel * 5f + 5f;
			_usingController = Input.GetJoystickNames().Length > 0;

			if (!_usingController)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}

		private void Update()
		{
			if (Input.GetAxis("Use Item") == 1f)
			{
				TranslocationMarker.gameObject.SetActive(true);

				Vector2 mouseRay;
				if (_usingController)
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
					Vector3 playerPosition = PlayerController.transform.position;

					Debug.DrawRay(playerPosition,
						(targetPosition - playerPosition).normalized
						* Mathf.Clamp(Vector2.Distance(playerPosition, targetPosition), 0f, MaxDistance), Color.red);
					playerRayHit = Physics2D.Raycast(playerPosition, targetPosition - playerPosition,
						Mathf.Clamp(Vector2.Distance(playerPosition, targetPosition), 0f, MaxDistance), layerMask);

					if (playerRayHit.collider == null)
					{
						transform.position = mouseRayHit.point;
						_positionValid = Vector2.Distance(playerPosition, targetPosition) <= MaxDistance;
					}
					else if (playerRayHit.collider.gameObject.layer == 11 || playerRayHit.collider.gameObject.layer == 13)
					{
						transform.position = playerRayHit.point;
						_normal = playerRayHit.normal;
						_positionValid = true;
						_hittingPlatform = true;
					}
				}

				TranslocationMarker.color = _positionValid ? CanTranslocate : CanNotTranslocate;
				TranslocationMarker.transform.Rotate(Vector3.forward);
			}

			if (Input.GetAxis("Use Item") == 0f)
			{
				if (_positionValid)
				{
					PlayerController.PlaySound("Translocation");

					TranslocationOccured = true;

					if ((TranslocationMarker.transform.position.x - PlayerController.transform.position.x) * PlayerController.transform.localScale.x < 0)
					{
						PlayerController.Flip();
					}

					if (_hittingPlatform && _normal.x == 0)
					{
						PlayerController.transform.position = transform.GetChild(_normal.y == 1 ? 1 : 2).position;
					}
					else
					{
						PlayerController.transform.position = TranslocationMarker.transform.position;
					}
				}

				EndTranslocation();
			}

			if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Exit"))
			{
				EndTranslocation();
			}
		}

		private void EndTranslocation()
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			SpellCaster.EndSpell(SpellCasting.SpellNames.Translocation);
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
