using AI.Guard;
using GameManager;
using UnityEngine;

namespace Spells
{
	public class Mimic : Spell
	{
		#region Fields

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

		#endregion

		#region Initialization Functions

		private void Start()
		{
			ManaDeductTick = (SpellCaster.SpellLevel - 1f) * 0.5f;

			MimicMarker.gameObject.SetActive(true);
			PlayerController.CanAttack = false;
			
			_camera = Camera.main;

			// AI - If not using a Gamepad, unlock and show the cursor so the Player can aim with the mouse.
			if (!InputManager.UsingGamepad)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}

		#endregion

		#region Update Functions

		private void Update()
		{
			// AI - If Disguised, spell is complete and no updates needed.
			if (Disguised)
			{
				return;
			}

			// AI - If the Mimic was aborted, end the spell.
			if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Exit"))
			{
				SpellCaster.EndSpell(SpellCasting.SpellNames.Mimic);
				return;
			}

			// AI - If the Player releases the spell input before the Mimic completes, end the spell.
			if (Input.GetAxis("Use Item") < 0.95f)
			{
				SpellCaster.EndSpell(SpellCasting.SpellNames.Mimic);
				return;
			}

			// AI - Ensure the Marker updates only so often, no matter the framerate.
			if (Time.time < _nextMarkerUpdate)
			{
				return;
			}
			
			_nextMarkerUpdate = Time.time + _markerUpdateTime;
			
			UpdateMarkerPosition();
			UpdateMarkerSprite();

			// AI - Check if Transfer is completed.
			_percentTransferred = Mathf.Clamp(_percentTransferred, 0f, 1f);
			if (_percentTransferred >= 1f)
			{
				PerformMimic();
			}
		}

		#endregion

		#region Marker Update Functions

		// AI - Move the Marker according to the Player's input.
		private void UpdateMarkerPosition()
		{
			Vector2 cursorPosition = GetUpdatedCursorPosition();
			RaycastHit2D cursorRayHit = Physics2D.Raycast(cursorPosition, Vector2.zero, 100f);

			// AI - If the cursor is not in a valid location, abort.
			if (!cursorRayHit)
			{
				ResetTransfer();
				return;
			}

			// AI - Shoot a raycast from the Player to the cursor to place the marker at the farthest valid position.
			LayerMask layerMask = WhatIsGround | WhatIsWall | WhatAreEnemies;
				
			Vector2 targetPosition = cursorRayHit.point;
			Vector2 playerPosition = PlayerController.transform.position;
			Vector2 targetDirection = targetPosition - playerPosition;
			Vector2 normalizedTargetDirection = targetDirection.normalized;
			float targetDistance = Vector2.Distance(playerPosition, targetPosition);
			float clampedDistance = Mathf.Clamp(targetDistance, 0f, MaxTransferDistance);

			Debug.DrawRay(playerPosition, normalizedTargetDirection * clampedDistance, Color.blue);
			RaycastHit2D playerRayHit = Physics2D.Raycast(playerPosition, targetDirection, clampedDistance, layerMask);

			// AI - If the raycast hit a Guard, target them.
			if (IsRaycastHittingMask(playerRayHit, WhatAreEnemies))
			{
				Guard guard = playerRayHit.collider?.GetComponentInParent<Guard>();
				if (guard != null)
				{
					TargetGuard(guard);
					return;
				}
			}

			// AI - If no Guard, reset Transfer.
			ResetTransfer();

			// AI - If the raycast hit a wall or ceiling, snap marker to that point.
			if (IsRaycastHittingMask(playerRayHit, WhatIsGround | WhatIsWall))
			{
				transform.position = playerRayHit.point;
				return;
			}
			
			// AI - Otherwise, set the Marker's position to the cursor position or the farthest it can go.
			transform.position = playerPosition + normalizedTargetDirection * clampedDistance;
		}

		// AI - Update the Marker's scale as the transfer progresses.
		private void UpdateMarkerSprite()
		{
			float markerSizeRange = _markerSizeMax - _markerSizeMin;
			float markerScale = markerSizeRange * _percentTransferred + _markerSizeMin;
			MimicMarker.transform.localScale = Vector2.one * markerScale;
		}

		#endregion

		#region Input Functions

		// AI - Get the cursor position depending on whether we're using a mouse or a controller.
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

		#endregion

		#region Transfer Functions

		// AI - Clear any transfer progress and reset the Marker color.
		private void ResetTransfer()
		{
			MimicMarker.color = CanNotMimic;
			_canTransfer = false;
			_isTransferring = false;
			_percentTransferred = 0;
		}

		// AI - Snap the Marker to the given Guard and maintain the connection while Attack is held.
		private void TargetGuard(Guard guard)
		{
			if (guard == null || guard.Head == null)
			{
				ResetTransfer();
				return;
			}

			transform.position = guard.Head.position;

			// AI - If the Guard can't be Mimicked, reset the transfer and abort.
			if (!guard.CanMimic)
			{
				ResetTransfer();
				return;
			}

			// AI - Enable the Transfer if not already.
			if (!_canTransfer)
			{
				MimicMarker.color = CanMimic;
				_canTransfer = true;
			}

			// AI - While Attack is held, update its progress.
			if (Input.GetAxis("Attack") >= 0.95f)
			{
				if (!_isTransferring)
				{
					_transferStartTime = Time.time;
					_isTransferring = true;
				}

				_percentTransferred = (Time.time - _transferStartTime) / TransferTime;
				return;
			}
			
			// AI - If Attack is released mid-transfer, reset the Transfer.
			if (Input.GetAxis("Attack") < 0.95f && _isTransferring)
			{
				ResetTransfer();
			}
		}

		// AI - Check whether the raycast hit a collider on one of the layers in the given mask.
		private bool IsRaycastHittingMask(RaycastHit2D raycastHit, LayerMask mask)
		{
			if (raycastHit.collider == null)
			{
				return false;
			}
			
			int layer = raycastHit.collider.gameObject.layer;
			int layerValue = 1 << layer;
			int maskAndLayerValue = mask & layerValue;
			return maskAndLayerValue != 0;
		}

		#endregion

		#region Mimic Functions

		// AI - Complete the Mimic, disguise the Player, and start the mana drain.
		private void PerformMimic()
		{
			MimicMarker.gameObject.SetActive(false);
			Disguised = true;
			SpellCaster.Disguised();
			PlayerController.gameObject.layer = 9;

			PlayerController.PlaySound("Mimic");
			PlayerController.InDisguise(true);
			PlayerController.CanAttack = true;

			// AI - Lock and hide the cursor again now that aiming is done.
			if (!InputManager.UsingGamepad)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
		}

		// AI - End the spell, removing the disguise if it was active, and destroy the Mimic object.
		public void EndMimic()
		{
			if (Disguised)
			{
				PlayerController.gameObject.layer = GameConstants.PLAYER_LAYER;
				PlayerController.PlaySound("Mimic");
				PlayerController.InDisguise(false);
			}

			PlayerController.CanAttack = true;
			if (!InputManager.UsingGamepad)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}

			Destroy(gameObject);
		}

		#endregion
	}
}
