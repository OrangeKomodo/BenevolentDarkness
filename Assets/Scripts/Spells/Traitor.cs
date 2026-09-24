using AI.Guard;
using GameManager;
using UnityEngine;

namespace Spells
{
	public class Traitor : Spell
	{
		public SpriteRenderer TraitorMarker;
		
		public float MaxTransferDistance;

		public Color CanCorrupt = Color.white;
		public Color CanNotCorrupt = Color.gray;
		
		private bool _canTransfer = false;
		
		private void Start()
		{
			PlayerController.CanAttack = false;

			if (!InputManager.UsingGamepad)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}

		private void FixedUpdate()
		{
			TraitorMarker.gameObject.SetActive(true);

			Vector2 mouseRay;
			if (InputManager.UsingGamepad)
			{
				transform.Translate(new Vector3(Input.GetAxis("Mouse X") * transform.parent.localScale.x, Input.GetAxis("Mouse Y"), 0f));
				mouseRay = transform.position;
			}
			else
			{
				mouseRay = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			}

			RaycastHit2D mouseRayHit = Physics2D.Raycast(mouseRay, Vector2.zero, 100f);
			RaycastHit2D playerRayHit;
			LayerMask layerMask = LayerMask.GetMask("Enemies", "Platforms", "Walls");

			if (mouseRayHit)
			{
				Vector3 targetPosition = mouseRayHit.point;
				Vector3 playerPosition = PlayerController.transform.position;

				Vector3 targetDirection = targetPosition - playerPosition;
				float distance = Vector2.Distance(playerPosition, targetPosition);
				float clampedDistance = Mathf.Clamp(distance,0f, MaxTransferDistance);
				Vector3 totalVector = targetDirection.normalized * clampedDistance;

				Debug.DrawRay(playerPosition, totalVector, Color.blue);
				playerRayHit = Physics2D.Raycast(playerPosition, targetDirection, clampedDistance, layerMask);

				if (playerRayHit.collider == null && !(mouseRayHit.collider.gameObject.layer == 9 && playerRayHit.collider.gameObject.layer == 9))
				{
					if (_canTransfer)
					{
						_canTransfer = false;
					}

					TraitorMarker.transform.position = mouseRayHit.point;
				}
				else
				{
					if ("Guard Actual Backside".Contains(playerRayHit.collider.name))
					{
						if (!_canTransfer)
						{
							_canTransfer = true;
						}

						//Debug.Log (playerRayHit.collider.name);
						Transform guard = playerRayHit.collider.name.Equals("Backside")
							? playerRayHit.collider.transform.parent
							: playerRayHit.transform;
						TraitorMarker.transform.position = guard.GetChild(4).position;

						//Debug.Log (playerRayHit.distance);

						if (_canTransfer && Input.GetAxis("Attack") == 1f)
						{
							PlayerController.PlaySound("Traitor");

							TraitorMarker.gameObject.SetActive(false);

							//CORRUPT GUARD HERE
							guard.GetComponent<Guard>().Corrupt();
							SpellCaster.Corrupted();

							EndTraitor();
						}

						TraitorMarker.color = _canTransfer ? CanCorrupt : CanNotCorrupt;
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
			PlayerController.CanAttack = true;
			Destroy(gameObject);
		}
	}
}
