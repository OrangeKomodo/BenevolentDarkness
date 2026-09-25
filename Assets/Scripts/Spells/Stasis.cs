using System.Collections.Generic;
using AI.Guard;
using AI.Sentry;
using GameManager;
using Player;
using UnityEngine;

namespace Spells
{
	public class Stasis : Spell
	{
		[Range(0, 100)]
		public float Sensitivity;
		public float MaxDistance;
		public float Duration;
		public bool StasisOccured;

		private float _stasisStartedTime;

		private Camera _camera;
		private CameraController _cameraController;
		
		public LayerMask WhatAreEnemies;
		public LayerMask WhatIsGround;
		public LayerMask WhatIsWall;

		private List<Guard> _frozenGuards = new List<Guard>();
		private List<Sentry> _frozenSentries = new List<Sentry>();
		
		private float _sensitivity => Sensitivity / 50f;
		
		private float _markerUpdateTime = 0.02f;
		private float _nextMarkerUpdate;

		private void Start()
		{
			_camera = Camera.main;
			//_cameraController = PlayerController.CameraController;
			//_cameraController.NewTarget(transform, Vector2.zero, 0.5f);

			if (!InputManager.UsingGamepad)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}

		private void Update()
		{
			if (StasisOccured)
			{
				if (Time.time >= _stasisStartedTime + Duration)
				{
					EndStasis();
				}
				
				return;
			}

			if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Exit"))
			{
				EndStasis();
				return;
			}

			if (Input.GetAxis("Use Item") == 0f)
			{
				PerformStasis();
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
			Vector2 playerPosition = PlayerController.transform.position;
			Vector2 targetDirection = targetPosition - playerPosition;
			Vector2 normalizedTargetDirection = targetDirection.normalized;
			float targetDistance = Vector2.Distance(playerPosition, targetPosition);
			float clampedDistance = Mathf.Clamp(targetDistance, 0f, MaxDistance);

			Debug.DrawRay(playerPosition, normalizedTargetDirection * clampedDistance, Color.orange);
			RaycastHit2D playerRayHit = Physics2D.Raycast(playerPosition, targetDirection, clampedDistance, layerMask);

			// If the raycast hit a wall or ceiling, record data to be used later
			if (playerRayHit.collider != null)
			{
				transform.position = playerRayHit.point;
				
				return;
			}
			
			// Otherwise, set the Marker's position to the cursor position or the farthest it can go in that direction
			transform.position = playerPosition + normalizedTargetDirection * clampedDistance;
		}

		private void PerformStasis()
		{
			PlayerController.PlaySound("Stasis");
			//_cameraController.ResetTarget();
			
			Collider2D[] enemiesToFreeze = Physics2D.OverlapCircleAll(transform.position, transform.localScale.x, WhatAreEnemies);
			for (int enemyIndex = 0; enemyIndex < enemiesToFreeze.Length; ++enemyIndex)
			{
				if (enemiesToFreeze[enemyIndex].name.Contains("Guard"))
				{
					Guard guard = enemiesToFreeze[enemyIndex].GetComponent<Guard>();
					guard.InStasis(true);
					_frozenGuards.Add(guard);
					
					continue;
				}
				
				if (enemiesToFreeze[enemyIndex].name.Contains("Sentry"))
				{
					Sentry sentry = enemiesToFreeze[enemyIndex].GetComponent<Sentry>();
					sentry.InStasis(true);
					_frozenSentries.Add(sentry);

					continue;
				}
			}

			_stasisStartedTime = Time.time;
			StasisOccured = true;
			SpellCaster.StasisOccured();

			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
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

		// Check whether the raycast hit a collider on one of the layers in the given mask
		private bool IsLayerInMask(int layer, LayerMask mask)
		{
			int layerValue = 1 << layer; // this converts the layer index to a bit so we can compare it against the mask
			int maskAndLayerValue = mask & layerValue;
			return maskAndLayerValue != 0;
		}

		private void EndStasis()
		{
			if (StasisOccured)
			{
				foreach (Guard guard in _frozenGuards)
				{
					guard.InStasis(false);
				}

				foreach (Sentry sentry in _frozenSentries)
				{
					sentry.InStasis(false);
				}
			
				_frozenGuards.Clear();
				_frozenSentries.Clear();
			}
			
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			//_cameraController.ResetTarget();

			Destroy(gameObject);
		}

		private void OnTriggerEnter2D(Collider2D otherCollider)
		{
			if (!StasisOccured)
			{
				return;
			}

			int layer =  otherCollider.gameObject.layer;
			if (IsLayerInMask(layer, WhatAreEnemies))
			{
				return;
			}
			
			if (otherCollider.name.Contains("Guard"))
			{
				Guard guard = otherCollider.GetComponent<Guard>();
				if (guard == null)
				{
					return;
				}
				
				guard.InStasis(true);
				_frozenGuards.Add(guard);
				
				return;
			}
			
			if (otherCollider.name.Contains("Sentry"))
			{
				Sentry sentry = otherCollider.GetComponent<Sentry>();
				if (sentry == null)
				{
					return;
				}
				
				sentry.InStasis(true);
				_frozenSentries.Add(sentry);
				
				return;
			}
		}
	}
}
