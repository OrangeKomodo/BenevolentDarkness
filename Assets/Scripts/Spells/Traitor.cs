using AI.Guard;
using GameManager;
using UnityEngine;

namespace Spells
{
	public class Traitor : Spell
	{
		#region Fields

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

		#endregion

		#region Initialization Functions
		
		private void Start()
		{
			TraitorMarker.gameObject.SetActive(true);
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
			// AI - If the Traitor was aborted, end the spell.
			if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Exit"))
			{
				EndTraitor();
				return;
			}

			// AI - If the Player releases the spell input before the Traitor is consummated, end the spell.
			if (Input.GetAxis("Use Item") < 0.95f)
			{
				SpellCaster.EndSpell(SpellCasting.SpellNames.Traitor);
				return;
			}

			// AI - Ensure the Marker updates only so often, no matter the framerate.
			if (Time.time < _nextMarkerUpdate)
			{
				return;
			}
			
			_nextMarkerUpdate = Time.time + _markerUpdateTime;
			UpdateMarkerPosition();
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

			// AI - If the raycast hit a Guard, validate the target.
			if (IsRaycastHittingMask(playerRayHit, WhatAreEnemies))
			{
				Guard guard = playerRayHit.collider?.GetComponentInParent<Guard>();
				
				if (IsTargetValid(guard))
				{
					TargetGuard(guard);
					return;
				}
			}

			// AI - If no Guard, reset Transfer.
			ResetTransfer();

			// AI - If the raycast hit a wall or ceiling, snap to that point.
			if (IsRaycastHittingMask(playerRayHit, WhatIsGround | WhatIsWall))
			{
				transform.position = playerRayHit.point;
				return;
			}
			
			// AI - Otherwise, set the Marker's position to the cursor position or the farthest it can go.
			transform.position = playerPosition + normalizedTargetDirection * clampedDistance;
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
			TraitorMarker.color = CanNotCorrupt;
			_canTransfer = false;
		}

		// AI - Check if the Guard is a valid target for corruption.
		private bool IsTargetValid(Guard guard)
		{
			if (guard == null)
			{
				return false;
			}
			
			if (guard.State == Guard.GuardState.Corrupted)
			{
				return false;
			}

			if (guard.State == Guard.GuardState.Dead || guard.State == Guard.GuardState.Unconscious)
			{
				return false;
			}

			return true;
		}

		// AI - Snap the Marker to the given Guard and corrupt them when attack is pressed.
		private void TargetGuard(Guard guard)
		{
			if (guard == null || guard.Head == null)
			{
				ResetTransfer();
				return;
			}

			transform.position = guard.Head.position;

			// AI - Enable the Transfer if not already.
			if (!_canTransfer)
			{
				TraitorMarker.color = CanCorrupt;
				_canTransfer = true;
			}
			
			// AI - Perform the Corruption (this spell has no transfer time, corruption is instant).
			if (Input.GetAxis("Attack") == 1f)
			{
				PerformTraitor(guard);
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

		#region Corruption Functions

		// AI - Corrupt the Guard and end the spell.
		private void PerformTraitor(Guard guard)
		{
			PlayerController.PlaySound("Traitor");

			guard.Corrupt();
			SpellCaster.Corrupted();

			EndTraitor();
		}

		// AI - End the spell and destroy the Traitor object.
		public void EndTraitor()
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			PlayerController.CanAttack = true;
			Destroy(gameObject);
		}

		#endregion
	}
}
