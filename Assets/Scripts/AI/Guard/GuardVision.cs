using System.Collections.Generic;
using Player;
using UnityEngine;

namespace AI.Guard
{
	public class GuardVision : MonoBehaviour
	{
		public bool BoxVisible = false;
		public bool CircleVisible = false;
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

		private void Update()
		{
			SearchForPlayer();

			SearchForOtherGuards();
		}

		private void SearchForPlayer()
		{
			if (_playerController.DisguisedAsGuard || _playerController.InShadowSink)
			{
				return;
			}

			if (!BoxVisible && !CircleVisible)
			{
				_guard.LostPlayer();
				return;
			}

			if (_inRange && !_wasInRange)
			{
				_guard.SetPlayerInMeleeRange();
				_wasInRange = true;
			}

			if (!_inRange && _wasInRange)
			{
				_guard.SetPlayerOutOfMeleeRange();
				_wasInRange = false;
			}

			if (!BoxVisible && !CircleVisible)
			{
				return;
			}
			
			Vector2 visionPosition = transform.position;
			Vector2 playerDirection = _playerController.transform.position - transform.position;
			float clampedDistance = Mathf.Clamp(Vector2.Distance(transform.position, _playerController.transform.position), 0f, 15f);
				
			Debug.DrawRay(visionPosition, playerDirection.normalized * clampedDistance, Color.yellow);
			RaycastHit2D playerRayHit = Physics2D.Raycast(visionPosition, playerDirection, clampedDistance, _layerMask);

			if (playerRayHit.collider != null && playerRayHit.collider.tag.Equals("Player"))
			{
				_guard.CheckSeesPlayer(_playerController.VisibilityFactor);
			}
		}

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

		private void OnTriggerEnter2D(Collider2D otherCollider)
		{
			if (otherCollider.tag.Equals("Player"))
			{
				if (otherCollider.Equals(_playerBodyCollider))
				{
					BoxVisible = true;
				}
				else if (otherCollider.Equals(_playerFeetCollider))
				{
					CircleVisible = true;
				}
				
				_inRange = _attackCollider.IsTouching(_playerBodyCollider) || _attackCollider.IsTouching(_playerFeetCollider);
			}

			if (otherCollider.name.Equals("Guard Actual") && !_visibleGuards.Contains(otherCollider.GetComponent<Guard>()))
			{
				_visibleGuards.Add(otherCollider.GetComponent<Guard>());
			}
		}

		private void OnTriggerExit2D(Collider2D otherCollider)
		{
			if (otherCollider.tag.Equals("Player"))
			{
				if (otherCollider.Equals(_playerBodyCollider) && !_visionCollider.IsTouching(_playerBodyCollider))
				{
					BoxVisible = false;
				}
				else if (otherCollider.Equals(_playerFeetCollider))
				{
					CircleVisible = false;
				}

				_inRange = _attackCollider.IsTouching(_playerBodyCollider) || _attackCollider.IsTouching(_playerFeetCollider);
			}

			if (otherCollider.name.Equals("Guard Actual") && _visibleGuards.Contains(otherCollider.GetComponent<Guard>()))
			{
				_visibleGuards.Remove(otherCollider.GetComponent<Guard>());
			}
		}
	}
}
