using AI.Guard;
using Player;
using UnityEngine;

namespace Spells
{
	public class Traitor : Spell
	{
		public float maxTransferDistance;

		public Color canCorrupt = Color.white;
		public Color canNotCorrupt = Color.gray;

		GameObject traitorMarker;
		GameObject player;
		PlayerController playerController;
		SpriteRenderer spriteRenderer;

		bool canTransfer = false;

		bool usingController;

		void Start()
		{
			traitorMarker = gameObject.transform.GetChild(0).gameObject;
			player = GameObject.FindGameObjectWithTag("Player");
			playerController = player.GetComponent<PlayerController>();
			spriteRenderer = traitorMarker.GetComponent<SpriteRenderer>();
			usingController = Input.GetJoystickNames().Length > 0;

			playerController.canAttack = false;

			if (!usingController)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}

		void FixedUpdate()
		{
			traitorMarker.SetActive(true);

			Vector2 mouseRay;
			if (usingController)
			{
				transform.Translate(new Vector3(Input.GetAxis("Mouse X") * transform.parent.localScale.x, Input.GetAxis("Mouse Y"), 0f));
				mouseRay = transform.position;
			}
			else
				mouseRay = Camera.main.ScreenToWorldPoint(Input.mousePosition);

			RaycastHit2D mouseRayHit = Physics2D.Raycast(mouseRay, Vector2.zero, 100f);
			RaycastHit2D playerRayHit;
			LayerMask layerMask = LayerMask.GetMask("Enemies", "Platforms", "Walls");

			if (mouseRayHit)
			{
				Vector3 targetPosition = mouseRayHit.point;
				Vector3 playerPosition = player.transform.position;

				Vector3 targetDirection = targetPosition - playerPosition;
				float distance = Vector2.Distance(playerPosition, targetPosition);
				float clampedDistance = Mathf.Clamp(distance,0f, maxTransferDistance);
				Vector3 totalVector = targetDirection.normalized * clampedDistance;

				Debug.DrawRay(playerPosition, totalVector, Color.blue);
				playerRayHit = Physics2D.Raycast(playerPosition, targetDirection, clampedDistance, layerMask);

				if (playerRayHit.collider == null && !(mouseRayHit.collider.gameObject.layer == 9 && playerRayHit.collider.gameObject.layer == 9))
				{
					if (canTransfer)
					{
						canTransfer = false;
					}

					traitorMarker.transform.position = mouseRayHit.point;
				}
				else
				{
					if ("Guard Actual Backside".Contains(playerRayHit.collider.name))
					{
						if (!canTransfer)
						{
							canTransfer = true;
						}

						//Debug.Log (playerRayHit.collider.name);
						Transform guard = playerRayHit.collider.name.Equals("Backside")
							? playerRayHit.collider.transform.parent
							: playerRayHit.transform;
						traitorMarker.transform.position = guard.GetChild(4).position;

						//Debug.Log (playerRayHit.distance);

						if (canTransfer && Input.GetAxis("Attack") == 1f)
						{
							playerController.PlaySound("Traitor");

							traitorMarker.SetActive(false);

							//CORRUPT GUARD HERE
							guard.GetComponent<Guard>().Corrupt();
							player.GetComponent<SpellCasting>().Corrupted();

							EndTraitor();
						}

						spriteRenderer.color = canTransfer ? canCorrupt : canNotCorrupt;
					}
				}
			}

			if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Exit"))
			{
				EndTraitor();
			}
		}

		public void EndTraitor()
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			playerController.canAttack = true;
			Destroy(gameObject);
		}
	}
}
