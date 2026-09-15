using Player;
using UnityEngine;

namespace AI.Sentry
{
	public class SentryVision : MonoBehaviour
	{
		private Sentry _sentry;
		private PolygonCollider2D _visionCollider;
		
		private PlayerController _playerController;
		private BoxCollider2D _playerBodyCollider;
		private CircleCollider2D _playerFeetCollider;

		private bool _boxVisible = false;
		private bool _circleVisible = false;

		private LayerMask _layerMask;

		private void Start()
		{
			_sentry = GetComponentInParent<Sentry>();
			_visionCollider = GetComponentInParent<PolygonCollider2D>();
			
			_playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
			_playerBodyCollider = _playerController.GetComponent<BoxCollider2D>();
			_playerFeetCollider = _playerController.GetComponent<CircleCollider2D>();
			
			_layerMask = LayerMask.GetMask("Player", "Platforms", "Affected Platforms");
		}

		private void Update()
		{
			if (_playerController.DisguisedAsGuard)
			{
				return;
			}

			if (!_boxVisible && !_circleVisible)
			{
				_sentry.LostPlayer();
				return;
			}
			
			Vector2 visionPosition = transform.position;
			Vector2 playerDirection = _playerController.transform.position - transform.position;
			float clampedDistance = Mathf.Clamp(Vector2.Distance(transform.position, _playerController.transform.position), 0f, 15f);

			Debug.DrawRay(visionPosition, playerDirection.normalized * clampedDistance, Color.yellow);
			RaycastHit2D playerRayHit = Physics2D.Raycast(visionPosition, playerDirection, clampedDistance, _layerMask);

			if (playerRayHit.collider != null && playerRayHit.collider.tag.Equals("Player"))
			{
				_sentry.CheckSeesPlayer(_playerController.VisibilityFactor);
			}
		}

		private void OnTriggerEnter2D(Collider2D otherCollider)
		{
			if (otherCollider.tag.Equals("Player"))
			{
				if (otherCollider.Equals(_playerBodyCollider))
				{
					_boxVisible = true;
				}
				else if (otherCollider.Equals(_playerFeetCollider))
				{
					_circleVisible = true;
				}
			}
		}

		void OnTriggerExit2D(Collider2D otherCollider)
		{
			if (otherCollider.tag.Equals("Player"))
			{
				if (otherCollider.Equals(_playerBodyCollider))
				{
					_boxVisible = false;
				}
				else if (otherCollider.Equals(_playerFeetCollider))
				{
					_circleVisible = false;
				}
			}
		}
	}
}
