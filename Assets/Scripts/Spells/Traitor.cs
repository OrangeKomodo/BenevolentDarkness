using AI.Guard;
using GameManager;
using UnityEngine;

namespace Spells
{
	public class Traitor : Spell
	{
		public SpriteRenderer TraitorMarker;
		
		[Range(0, 100)]
		public float Sensitivity;
		public float MaxTransferDistance;

		public Color CanCorrupt = Color.white;
		public Color CanNotCorrupt = Color.gray;
		
		public LayerMask WhatAreEnemies;
		public LayerMask WhatIsGround;
		public LayerMask WhatIsWall;
		
		private Camera _camera;
		
		private float _sensitivity => Sensitivity / 50f;
		
		private float _markerUpdateTime = 0.02f;
		private float _nextMarkerUpdate;
		
		private bool _canTransfer = false;
		
		private void Start()
		{
			// Show the Marker and prevent the Player from attacking while aiming the Mimic
			TraitorMarker.gameObject.SetActive(true);
			PlayerController.CanAttack = false;

			_camera = Camera.main;

			// If not using a Gamepad, unlock and show the cursor so the Player can aim with the mouse
			if (!InputManager.UsingGamepad)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}

		private void Update()
		{
			// If the Traitor was aborted, end the spell
			if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Exit"))
			{
				EndTraitor();
				return;
			}

			// If the Player releases the spell input before the Traitor is consummated, end the spell
			if (Input.GetAxis("Use Item") < 0.95f)
			{
				SpellCaster.EndSpell(SpellCasting.SpellNames.Traitor);
				return;
			}

			// Ensure the Marker updates only so often, no matter the framerate.
			if (Time.time < _nextMarkerUpdate)
			{
				return;
			}
			
			_nextMarkerUpdate = Time.time + _markerUpdateTime;

			UpdateMarkerPosition();
		}

		private void UpdateMarkerPosition()
		{
			// Get cursor position for the mouse or the controller
			Vector2 cursorPosition = GetUpdatedCursorPosition();

			RaycastHit2D cursorRayHit = Physics2D.Raycast(cursorPosition, Vector2.zero, 100f);

			// If the cursor is not in a valid location, abort
			if (!cursorRayHit)
			{
				ResetTransfer();
				return;
			}

			// Shoot a raycast from the Player to the cursor to place the marker at the farthest valid position in that direction
			LayerMask layerMask = WhatIsGround | WhatIsWall | WhatAreEnemies;
				
			Vector2 targetPosition = cursorRayHit.point;
			Vector2 playerPosition = PlayerController.transform.position;
			Vector2 targetDirection = targetPosition - playerPosition;
			Vector2 normalizedTargetDirection = targetDirection.normalized;
			float targetDistance = Vector2.Distance(playerPosition, targetPosition);
			float clampedDistance = Mathf.Clamp(targetDistance, 0f, MaxTransferDistance);

			Debug.DrawRay(playerPosition, normalizedTargetDirection * clampedDistance, Color.blue);
			RaycastHit2D playerRayHit = Physics2D.Raycast(playerPosition, targetDirection, clampedDistance, layerMask);

			// If the raycast hit a Guard, validate the target, target them, and return
			if (IsRaycastHittingMask(playerRayHit, WhatAreEnemies))
			{
				Guard guard = playerRayHit.collider.GetComponentInParent<Guard>();
				
				if (IsTargetValid(guard))
				{
					TargetGuard(guard);
				
					return;
				}
			}

			// If no Guard, reset Transfer
			ResetTransfer();

			// If the raycast hit a wall or ceiling, record data to be used later
			if (IsRaycastHittingMask(playerRayHit, WhatIsGround | WhatIsWall))
			{
				transform.position = playerRayHit.point;
				
				return;
			}
			
			// Otherwise, set the Marker's position to the cursor position or the farthest it can go in that direction
			transform.position = playerPosition + normalizedTargetDirection * clampedDistance;
		}

		// Get the cursor position depending on whether we're using a mouse or a controller
		private Vector3 GetUpdatedCursorPosition()
		{
			if (InputManager.UsingGamepad)
			{
				float xPositionDelta = Input.GetAxis("Mouse X") * _sensitivity;
				float yPositionDelta = Input.GetAxis("Mouse Y") * _sensitivity;
				Vector3 cursorPositionDelta = new Vector3(xPositionDelta, yPositionDelta, 0f);
				
				return transform.position + cursorPositionDelta;
			}
			
			return _camera.ScreenToWorldPoint(Input.mousePosition);
		}

		// Clear any transfer progress and reset the Marker color
		private void ResetTransfer()
		{
			TraitorMarker.color = CanNotCorrupt;
			_canTransfer = false;
		}

		// Check if the Guard is valid target
		private bool IsTargetValid(Guard guard)
		{
			// Check if Guard even exists
			if (guard == null)
			{
				return false;
			}
			
			// Check to see if the Guard is already corrupted
			if (guard.State == Guard.GuardState.Corrupted)
			{
				return false;
			}

			// Check if Guard is already incapacitated
			if (guard.State == Guard.GuardState.Dead || guard.State == Guard.GuardState.Unconscious)
			{
				return false;
			}

			// Return true if all checks pass
			return true;
		}

		// Snap the Marker to the given Guard and maintain the connection while Attack is held
		private void TargetGuard(Guard guard)
		{
			transform.position = guard.Head.position;

			// Enable the Transfer if not already
			if (!_canTransfer)
			{
				TraitorMarker.color = CanCorrupt;
				_canTransfer = true;
			}
			
			// Perform the Corruption
			// This spell does not have a transfer time, the Corruption occurs right when the attack is consummated
			if (Input.GetAxis("Attack") == 1f)
			{
				PerformTraitor(guard);
			}
		}

		// Check whether the raycast hit a collider on one of the layers in the given mask
		private bool IsRaycastHittingMask(RaycastHit2D raycastHit, LayerMask mask)
		{
			if (raycastHit.collider == null)
			{
				return false;
			}
			
			int layer = raycastHit.collider.gameObject.layer;
			int layerValue = 1 << layer; // this converts the layer index to a bit so we can compare it against the mask
			int maskAndLayerValue = mask & layerValue;
			return maskAndLayerValue != 0;
		}

		// Corrupt the Guard and end the spell
		private void PerformTraitor(Guard guard)
		{
			PlayerController.PlaySound("Traitor");

			//CORRUPT GUARD HERE
			guard.Corrupt();
			SpellCaster.Corrupted();

			EndTraitor();
		}

		// End the spell, and destroy the Traitor object
		// This spell doesn't end its effects when the effects on the Guard ends
		public void EndTraitor()
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			PlayerController.CanAttack = true;
			Destroy(gameObject);
		}
	}
}
