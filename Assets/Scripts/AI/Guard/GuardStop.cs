using UnityEngine;

namespace AI.Guard
{
	// AI - Note: This is a small class and doesn't require regions.
	public class GuardStop : MonoBehaviour
	{
		public Guard Guard;
		public bool Stationary;
		public float IdleTime = 0f;
		public Vector2 IdleDirection;
		public Transform NextStop;

		// AI - Trigger the guard's stop behavior when they reach this stop point.
		private void OnTriggerEnter2D(Collider2D otherCollider)
		{
			if (!otherCollider.gameObject.Equals(Guard.gameObject) || Guard.NextStop != transform)
			{
				return;
			}

			ForceUpdate();
		}

		// AI - Force the guard to process reaching this stop, either staying stationary or moving to the next.
		public void ForceUpdate()
		{
			if (Stationary)
			{
				Guard.StopReached(Mathf.Infinity, IdleDirection, transform);
				return;
			}
			
			Guard.StopReached(IdleTime, IdleDirection, NextStop);
		}
	}
}
