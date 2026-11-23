using System.Collections.Generic;
using Player;
using UnityEngine;

namespace AI.Guard
{
	public class GuardVision : MonoBehaviour
	{
		#region Fields

		private bool _boxVisible = false;
		private bool _circleVisible = false;
		private bool _inRange = false;
		private bool _wasInRange = false;

		private Guard _guard;
		private PolygonCollider2D _visionCollider;
		private CircleCollider2D _attackCollider;
		
		private PlayerController _playerController;
		private BoxCollider2D _playerBodyCollider;
		private CircleCollider2D _playerFeetCollider;

		private LayerMask _layerMask;

		private List<Guard> _visibleGuards = new List<Guard>();
		private List<Guard> _incapacitatedGuards = new List<Guard>();

		#endregion

		#region Initialization Functions

		private void Start()
		{
			_guard = GetComponentInParent<Guard>();
			_visionCollider = GetComponent<PolygonCollider2D>();
			_attackCollider = GetComponent<CircleCollider2D>();
			
			_playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
			_playerBodyCollider = _playerController.GetComponent<BoxCollider2D>();
			_playerFeetCollider = _playerController.GetComponent<CircleCollider2D>();
			
			_layerMask = LayerMask.GetMask("Player", "Platforms", "Affected Platforms");
		}

		#endregion

		#region Update Functions

		private void Update()
		{
			SearchForPlayer();
			SearchForOtherGuards();
		}

		#endregion

		#region Player Detection Functions

		// AI - Check if the player is visible within the guard's vision cone using raycasts.
		private void SearchForPlayer()
		{
			if (_playerController.DisguisedAsGuard || _playerController.InShadowSink)
			{
				return;
			}

			if (!_boxVisible && !_circleVisible)
			{
				_guard.LostPlayer();
				return;
			}

			UpdatePlayerRangeStatus();
			CheckPlayerLineOfSight();
		}

		// AI - Update whether the player has entered or exited melee range.
		private void UpdatePlayerRangeStatus()
		{
			if (_inRange && !_wasInRange)
			{
				_guard.SetPlayerInMeleeRange();
				_wasInRange = true;
				return;
			}

			if (!_inRange && _wasInRange)
			{
				_guard.SetPlayerOutOfMeleeRange();
				_wasInRange = false;
			}
		}

		// AI - Raycast to the player to check if they're visible or if something is blocking the view.
		private void CheckPlayerLineOfSight()
		{
			Vector2 visionPosition = transform.position;
			Vector2 playerDirection = _playerController.transform.position - transform.position;
			float clampedDistance = Mathf.Clamp(Vector2.Distance(transform.position, _playerController.transform.position), 0f, 15f);
				
			Debug.DrawRay(visionPosition, playerDirection.normalized * clampedDistance, Color.yellow);
			RaycastHit2D playerRayHit = Physics2D.Raycast(visionPosition, playerDirection, clampedDistance, _layerMask);

			if (playerRayHit.collider == null || !playerRayHit.collider.tag.Equals(GameManager.GameConstants.PLAYER_TAG))
			{
				return;
			}

			_guard.CheckSeesPlayer(_playerController.VisibilityFactor);
		}

		#endregion

		#region Guard Detection Functions

		// AI - Check if any visible guards have been incapacitated and alert the searching guard.
		private void SearchForOtherGuards()
		{
			if (_visibleGuards.Count == 0)
			{
				return;
			}
			
			for (int fellowGuardIndex = 0; fellowGuardIndex < _visibleGuards.Count; fellowGuardIndex++)
			{
				Guard fellowGuard = _visibleGuards[fellowGuardIndex];
				
				if (fellowGuard.State != Guard.GuardState.Dead && fellowGuard.State != Guard.GuardState.Unconscious)
				{
					continue;
				}

				if (fellowGuard.transform.parent.parent != transform.parent.parent.parent)
				{
					continue;
				}
				
				if (_incapacitatedGuards.Contains(fellowGuard))
				{
					continue;
				}
				
				_incapacitatedGuards.Add(fellowGuard);
				_guard.FoundGuard(fellowGuard);
			}
		}

		#endregion

		#region Collision Functions

		// AI - Detect when the player or other guards enter the vision cone.
		private void OnTriggerEnter2D(Collider2D otherCollider)
		{
			if (otherCollider.tag.Equals(GameManager.GameConstants.PLAYER_TAG))
			{
				HandlePlayerEnterVision(otherCollider);
				return;
			}

			if (otherCollider.name.Equals(GameManager.GameConstants.GUARD_ACTUAL_NAME) && !_visibleGuards.Contains(otherCollider.GetComponent<Guard>()))
			{
				_visibleGuards.Add(otherCollider.GetComponent<Guard>());
			}
		}

		// AI - Track which player collider entered the vision cone and check if they're in attack range.
		private void HandlePlayerEnterVision(Collider2D otherCollider)
		{
			if (otherCollider.Equals(_playerBodyCollider))
			{
				_boxVisible = true;
			}
			else if (otherCollider.Equals(_playerFeetCollider))
			{
				_circleVisible = true;
			}
			
			_inRange = _attackCollider.IsTouching(_playerBodyCollider) || _attackCollider.IsTouching(_playerFeetCollider);
		}

		// AI - Detect when the player or other guards exit the vision cone.
		private void OnTriggerExit2D(Collider2D otherCollider)
		{
			if (otherCollider.tag.Equals(GameManager.GameConstants.PLAYER_TAG))
			{
				HandlePlayerExitVision(otherCollider);
				return;
			}

			if (otherCollider.name.Equals(GameManager.GameConstants.GUARD_ACTUAL_NAME) && _visibleGuards.Contains(otherCollider.GetComponent<Guard>()))
			{
				_visibleGuards.Remove(otherCollider.GetComponent<Guard>());
			}
		}

		// AI - Track which player collider exited the vision cone and check if they're still in attack range.
		private void HandlePlayerExitVision(Collider2D otherCollider)
		{
			if (otherCollider.Equals(_playerBodyCollider) && !_visionCollider.IsTouching(_playerBodyCollider))
			{
				_boxVisible = false;
			}
			else if (otherCollider.Equals(_playerFeetCollider))
			{
				_circleVisible = false;
			}

			_inRange = _attackCollider.IsTouching(_playerBodyCollider) || _attackCollider.IsTouching(_playerFeetCollider);
		}

		#endregion
	}
}
