using AI.Guard;
using GameManager;
using UnityEngine;

namespace Spells
{
	public class Mimic : Spell
	{
		public SpriteRenderer MimicMarker;
		
		public int ManaTickCost;
		public float ManaDeductTick;

		[Range(0, 100)]
		public float Sensitivity;
		public float MaxTransferDistance;
		public float TransferTime;
		public bool Disguised;
		public Color CanMimic = Color.white;
		public Color CanNotMimic = Color.gray;
		
		public LayerMask WhatAreEnemies;
		public LayerMask WhatIsGround;
		public LayerMask WhatIsWall;
		
		private Camera _camera;
		
		private float _sensitivity => Sensitivity / 50f;
		
		private float _markerUpdateTime = 0.02f;
		private float _nextMarkerUpdate;

		private bool _canTransfer = false;
		private bool _isTransferring = false;
		private float _transferStartTime;
		private float _percentTransferred;

		private readonly float _markerSizeMin = 0.5f;
		private readonly float _markerSizeMax = 1.0f;

		private void Start()
		{
			ManaDeductTick = (SpellCaster.SpellLevel - 1f) * 0.5f;

			// Show the Marker and prevent the Player from attacking while aiming the Mimic
			MimicMarker.gameObject.SetActive(true);
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
			// If Disguised, we don't care
			if (Disguised)
			{
				return;
			}

			// If the Mimic was aborted, end the spell
			if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Exit"))
			{
				SpellCaster.EndSpell(SpellCasting.SpellNames.Mimic);
				return;
			}

			// If the Player releases the spell input before the Mimic completes, end the spell
			if (Input.GetAxis("Use Item") < 0.95f)
			{
				SpellCaster.EndSpell(SpellCasting.SpellNames.Mimic);
				return;
			}

			// Ensure the Marker updates only so often, no matter the framerate.
			if (Time.time < _nextMarkerUpdate)
			{
				return;
			}
			
			_nextMarkerUpdate = Time.time + _markerUpdateTime;
			
			// Perform Marker position and sprite updates
			UpdateMarkerPosition();
			UpdateMarkerSprite();

			// Check if Transfer is completed
			_percentTransferred = Mathf.Clamp(_percentTransferred, 0f, 1f);
			if (_percentTransferred < 1f)
			{
				return;
			}
			
			// Perform Mimic
			PerformMimic();
		}

		// Move the Marker according to the Player's input
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

			// If the raycast hit a Guard, target them and return
			if (IsRaycastHittingMask(playerRayHit, WhatAreEnemies))
			{
				Guard guard = playerRayHit.collider.GetComponentInParent<Guard>();
				if (guard != null)
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
			
			// Claude: Otherwise, set the Marker's position to the cursor position or the farthest it can go in that direction
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
			MimicMarker.color = CanNotMimic;
			_canTransfer = false;
			_isTransferring = false;
			_percentTransferred = 0;
		}

		// Snap the Marker to the given Guard and maintain the connection while Attack is held
		private void TargetGuard(Guard guard)
		{
			transform.position = guard.Head.position;

			// If the Guard can't be Mimicked, reset the transfer and abort
			if (!guard.CanMimic)
			{
				ResetTransfer();
				return;
			}

			// Enable the Transfer if not already
			if (!_canTransfer)
			{
				MimicMarker.color = CanMimic;
				_canTransfer = true;
			}

			// While Attack is held, update its progress
			if (Input.GetAxis("Attack") >= 0.95f)
			{
				// Mark Transfer started if not already
				if (!_isTransferring)
				{
					_transferStartTime = Time.time;
					_isTransferring = true;
				}

				_percentTransferred = (Time.time - _transferStartTime) / TransferTime;

				return;
			}
			
			// If Attack is released mid-transfer, reset the Transfer
			if (Input.GetAxis("Attack") < 0.95f && _isTransferring)
			{
				ResetTransfer();
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

		// Update the Marker's scale as the transfer progresses
		private void UpdateMarkerSprite()
		{
			float markerSizeRange = _markerSizeMax - _markerSizeMin;
			float markerScale = markerSizeRange * _percentTransferred + _markerSizeMin;
			MimicMarker.transform.localScale = Vector2.one * markerScale;
		}

		// Complete the Mimic, disguise the Player, and start the mana drain
		private void PerformMimic()
		{
			//Debug.Log ("Transfer Complete");
			MimicMarker.gameObject.SetActive(false);
			Disguised = true;
			SpellCaster.Disguised();
			PlayerController.gameObject.layer = 9;

			// DISGUISE HERE
			PlayerController.PlaySound("Mimic");
			PlayerController.InDisguise(true);
			PlayerController.CanAttack = true;

			// Lock and hide the cursor again now that aiming is done
			if (!InputManager.UsingGamepad)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
		}

		// End the spell, removing the disguise if it was active, and destroy the Mimic object
		public void EndMimic()
		{
			// UNDISGUISE
			if (Disguised)
			{
				PlayerController.gameObject.layer = 8;
				PlayerController.PlaySound("Mimic");
				PlayerController.InDisguise(false);
			}

			// Let the Player attack again and lock the cursor
			PlayerController.CanAttack = true;
			if (!InputManager.UsingGamepad)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}

			Destroy(gameObject);
		}
	}
}
