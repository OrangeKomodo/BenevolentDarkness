using System.Collections.Generic;
using Player;
using UnityEngine;

namespace AI.Guard
{
	public class GuardVision : MonoBehaviour
	{
		private Guard _guard;
		private GameObject _player;
		private PlayerController _playerController;

		public bool BoxVisible = false;
		public bool CircleVisible = false;
		private bool _inRange = false;
		private bool _wasInRange = false;

		private LayerMask _layerMask;

		private List<Guard> _visibleGuards = new List<Guard>();
		private List<Guard> _incapacitatedGuards = new List<Guard>();

		private void Start()
		{
			_guard = gameObject.transform.parent.GetComponent<Guard>();
			_player = GameObject.FindGameObjectWithTag("Player");
			_playerController = _player.GetComponent<PlayerController>();
			_layerMask = LayerMask.GetMask("Player", "Platforms", "Affected Platforms");
		}

		private void FixedUpdate()
		{
			if (!_playerController.DisguisedAsGuard)
			{
				if (BoxVisible || CircleVisible)
				{
					Debug.DrawRay(transform.position,
						(_player.transform.position - transform.position).normalized
						* Mathf.Clamp(Vector2.Distance(transform.position, _player.transform.position), 0f, 15f),
						Color.yellow);
					RaycastHit2D playerRayHit = Physics2D.Raycast(transform.position,
						_player.transform.position - transform.position,
						Mathf.Clamp(Vector2.Distance(transform.position, _player.transform.position), 0f, 15f), _layerMask);

					if (playerRayHit.collider != null && playerRayHit.collider.tag.Equals("Player"))
					{
						_guard.CheckSeesPlayer(_player.GetComponent<PlayerController>().VisibilityFactor);
					}
				}

				if (!BoxVisible && !CircleVisible && _guard.SeesPlayer)
				{
					_guard.LostPlayer();
				}

				if (!_playerController.InShadowSink && (!_wasInRange && _inRange || _wasInRange && !_inRange))
				{
					_guard.PlayerInMeleeRange(_inRange);
					_wasInRange = !_wasInRange;
				}
			}

			if (_visibleGuards.Count > 0)
			{
				for (int i = 0; i < _visibleGuards.Count; i++)
				{
					Guard fellowGuard = _visibleGuards[i];
					if ((fellowGuard.State == Guard.GuardState.Dead || fellowGuard.State == Guard.GuardState.Unconscious)
					    && !_incapacitatedGuards.Contains(fellowGuard)
					    && fellowGuard.transform.parent.parent == transform.parent.parent.parent)
					{
						_incapacitatedGuards.Add(fellowGuard);
						_guard.FoundGuard(fellowGuard);
					}
				}
			}
		}

		private void OnTriggerEnter2D(Collider2D otherCollider)
		{
			if (otherCollider.tag.Equals("Player"))
			{
				if (otherCollider.Equals(_player.GetComponent<BoxCollider2D>()))
				{
					BoxVisible = true;
				}
				else if (otherCollider.Equals(_player.GetComponent<CircleCollider2D>()))
				{
					CircleVisible = true;
				}
				
				_inRange = GetComponent<CircleCollider2D>().IsTouching(_player.GetComponent<BoxCollider2D>())
				          || GetComponent<CircleCollider2D>().IsTouching(_player.GetComponent<CircleCollider2D>());
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
				if (otherCollider.Equals(_player.GetComponent<BoxCollider2D>()) && !GetComponent<PolygonCollider2D>()
					    .IsTouching(_player.GetComponent<BoxCollider2D>()))
				{
					BoxVisible = false;
				}
				else if (otherCollider.Equals(_player.GetComponent<CircleCollider2D>()))
				{
					CircleVisible = false;
				}

				_inRange = GetComponent<CircleCollider2D>().IsTouching(_player.GetComponent<BoxCollider2D>())
				           || GetComponent<CircleCollider2D>().IsTouching(_player.GetComponent<CircleCollider2D>());
			}

			if (otherCollider.name.Equals("Guard Actual") && _visibleGuards.Contains(otherCollider.GetComponent<Guard>()))
			{
				_visibleGuards.Remove(otherCollider.GetComponent<Guard>());
			}
		}
	}
}
