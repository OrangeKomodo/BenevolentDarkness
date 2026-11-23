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
		#region Fields

		[Range(0, 100)]
		public float Sensitivity;
		public float MaxDistance;
		public float Duration;
		public bool StasisOccured;

		private float _stasisStartedTime;

		private Camera _camera;
		
		public LayerMask WhatAreEnemies;
		public LayerMask WhatIsGround;
		public LayerMask WhatIsWall;

		private List<Guard> _frozenGuards = new List<Guard>();
		private List<Sentry> _frozenSentries = new List<Sentry>();
		
		private float _sensitivity => Sensitivity / 50f;
		
		private float _markerUpdateTime = 0.02f;
		private float _nextMarkerUpdate;

		#endregion

		#region Initialization Functions

		private void Start()
		{
			_camera = Camera.main;

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
				return;
			}

			// AI - Shoot a raycast from the Player to the cursor to place the marker at the farthest valid position.
			LayerMask layerMask = WhatIsGround | WhatIsWall;
				
			Vector2 targetPosition = cursorRayHit.point;
			Vector2 playerPosition = PlayerController.transform.position;
			Vector2 targetDirection = targetPosition - playerPosition;
			Vector2 normalizedTargetDirection = targetDirection.normalized;
			float targetDistance = Vector2.Distance(playerPosition, targetPosition);
			float clampedDistance = Mathf.Clamp(targetDistance, 0f, MaxDistance);

			Debug.DrawRay(playerPosition, normalizedTargetDirection * clampedDistance, Color.orange);
			RaycastHit2D playerRayHit = Physics2D.Raycast(playerPosition, targetDirection, clampedDistance, layerMask);

			// AI - If the raycast hit a wall or ceiling, snap to that point.
			if (playerRayHit.collider != null)
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

		#region Stasis Functions

		// AI - Freeze all enemies within the stasis bubble's radius.
		private void PerformStasis()
		{
			PlayerController.PlaySound("Stasis");
			
			Collider2D[] enemiesToFreeze = Physics2D.OverlapCircleAll(transform.position, transform.localScale.x, WhatAreEnemies);
			for (int enemyIndex = 0; enemyIndex < enemiesToFreeze.Length; ++enemyIndex)
			{
				FreezeEnemy(enemiesToFreeze[enemyIndex]);
			}

			_stasisStartedTime = Time.time;
			StasisOccured = true;
			SpellCaster.StasisOccured();

			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		// AI - Freeze a single enemy (guard or sentry).
		private void FreezeEnemy(Collider2D enemyCollider)
		{
			if (enemyCollider == null)
			{
				return;
			}

			if (enemyCollider.name.Contains("Guard"))
			{
				Guard guard = enemyCollider.GetComponent<Guard>();
				if (guard != null && !_frozenGuards.Contains(guard))
				{
					guard.InStasis(true);
					_frozenGuards.Add(guard);
				}
				return;
			}
			
			if (enemyCollider.name.Contains("Sentry"))
			{
				Sentry sentry = enemyCollider.GetComponent<Sentry>();
				if (sentry != null && !_frozenSentries.Contains(sentry))
				{
					sentry.InStasis(true);
					_frozenSentries.Add(sentry);
				}
			}
		}

		// AI - End the stasis effect and unfreeze all enemies.
		private void EndStasis()
		{
			if (StasisOccured)
			{
				foreach (Guard guard in _frozenGuards)
				{
					if (guard != null)
					{
						guard.InStasis(false);
					}
				}

				foreach (Sentry sentry in _frozenSentries)
				{
					if (sentry != null)
					{
						sentry.InStasis(false);
					}
				}
			
				_frozenGuards.Clear();
				_frozenSentries.Clear();
			}
			
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

			Destroy(gameObject);
		}

		#endregion

		#region Collision Functions

		// AI - Freeze enemies that enter the stasis bubble after it's been cast.
		private void OnTriggerEnter2D(Collider2D otherCollider)
		{
			if (!StasisOccured)
			{
				return;
			}

			int layer = otherCollider.gameObject.layer;
			if (!IsLayerInMask(layer, WhatAreEnemies))
			{
				return;
			}
			
			FreezeEnemy(otherCollider);
		}

		#endregion

		#region Helper Functions

		// AI - Check whether the given layer is included in the mask.
		private bool IsLayerInMask(int layer, LayerMask mask)
		{
			int layerValue = 1 << layer;
			int maskAndLayerValue = mask & layerValue;
			return maskAndLayerValue != 0;
		}

		#endregion
	}
}
