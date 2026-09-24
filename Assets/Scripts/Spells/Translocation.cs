using GameManager;
using UnityEngine;

namespace Spells
{
	public class Translocation : Spell
	{
		public SpriteRenderer TranslocationMarker;
		public Transform HighPoint;
		public Transform LowPoint;
		
		[Range(0, 100)]
		public float Sensitivity;
		public float MaxDistance;
		public float MarkerSpinSpeed;
		public Color CanTranslocate = Color.white;
		public Color CanNotTranslocate = Color.gray;
		
		public LayerMask WhatIsGround;
		public LayerMask WhatIsWall;
		
		private Camera _camera;
		
		private Transform _playerTransform;
		private Transform _markerTransform;
		
		private float _sensitivity => Sensitivity / 50f;
		
		private float _markerUpdateTime = 0.02f;
		private float _nextMarkerUpdate;

		private bool _positionValid; // <- This has been made a bit useless since the spell was redesigned to snap to its distance boundary
		private bool _hittingPlatform;
		private Vector2 _normal;

		private void Start()
		{
			MaxDistance = SpellCaster.SpellLevel * 5f + 5f;
			
			TranslocationMarker.gameObject.SetActive(true);
			
			_camera = Camera.main;

			_playerTransform = PlayerController.transform;
			_markerTransform = TranslocationMarker.transform;
			
			if (!InputManager.UsingGamepad)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}

		private void Update()
		{
			// If the Translocation was aborted, end the spell
			if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Exit"))
			{
				EndTranslocation();
				return;
			}

			// If the player uses the Translocation, attempt to perform it and end the spell
			if (Input.GetAxis("Use Item") < 0.95f)
			{
				TryPerformTranslocation();
				EndTranslocation();
				return;
			}

			// Ensure the Marker updates only so often, no matter the framerate.
			if (Time.time < _nextMarkerUpdate)
			{
				return;
			}
			
			_nextMarkerUpdate = Time.time + _markerUpdateTime;
			
			// Perform Marker updates
			UpdateMarkerPosition();
			UpdateMarkerSprite();
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
				return;
			}
			
			// Shoot a raycast from the Player to the cursor to place the marker at the farthest valid position in that direction
			LayerMask layerMask = WhatIsGround | WhatIsWall;
				
			Vector2 targetPosition = cursorRayHit.point;
			Vector2 playerPosition = _playerTransform.position;
			Vector2 targetDirection = targetPosition - playerPosition;
			Vector2 normalizedTargetDirection = targetDirection.normalized;
			float targetDistance = Vector2.Distance(playerPosition, targetPosition);
			float clampedDistance = Mathf.Clamp(targetDistance, 0f, MaxDistance);

			Debug.DrawRay(playerPosition, normalizedTargetDirection * clampedDistance, Color.red);
			RaycastHit2D playerRayHit = Physics2D.Raycast(playerPosition, targetDirection, clampedDistance, layerMask);

			// If the raycast hit something (like a wall or the ceiling), record data to be used later
			if (playerRayHit.collider != null)
			{
				transform.position = playerRayHit.point;
				_positionValid = true;
				
				_normal = playerRayHit.normal;
				_hittingPlatform = true;
				
				return;
			}
			
			// Set the Marker's position to the cursor position or the farthest it can go in that direction
			transform.position = playerPosition + normalizedTargetDirection * clampedDistance;
			_positionValid = true;
			//transform.position = cursorRayHit.point;
			//_positionValid = targetDistance <= MaxDistance;
		}

		// Perform the Translocation if the position is valid
		private void TryPerformTranslocation()
		{
			if (!_positionValid)
			{
				return;
			}
			
			PlayerController.PlaySound("Translocation");

			SpellCaster.TranslocationOccured();
			
			Vector2 finalTranslocationPoint = GetFinalTranslocationPoint();

			// If the Translocation Marker is behind the Player, flip them
			if ((_markerTransform.position.x - _playerTransform.position.x) * _playerTransform.localScale.x < 0)
			{
				PlayerController.Flip();
			}

			_playerTransform.position = finalTranslocationPoint;
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

		// Get Final Translocation Point
		private Vector2 GetFinalTranslocationPoint()
		{
			// If Ray isn't hitting a horizontal platform, return normal marker location
			if (!_hittingPlatform || _normal.x != 0)
			{
				return _markerTransform.position;
			}
			
			// If ray is hitting the floor, return the higher transform location
			if (_normal.y >= 1)
			{
				return HighPoint.position;
			}
			
			// If ray is hitting the ceiling, return the lower transform location
			return LowPoint.position;
		}

		// Update the Marker's color and rotation
		private void UpdateMarkerSprite()
		{
			TranslocationMarker.color = _positionValid ? CanTranslocate : CanNotTranslocate;
			TranslocationMarker.transform.Rotate(Vector3.forward * MarkerSpinSpeed);
		}

		// End Translocation
		private void EndTranslocation()
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			SpellCaster.EndSpell(SpellCasting.SpellNames.Translocation);
		}
	}
}
