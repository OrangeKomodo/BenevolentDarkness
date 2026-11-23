using UnityEngine;

namespace AI.Sentry
{
	// AI - Note: This is a small class and doesn't require regions.
	public class SentryStop : MonoBehaviour
	{
		public Sentry Sentry;
		public float IdleTime = 0f;
		public Transform NextStop;

		// AI - Trigger the sentry's stop behavior when they reach this stop point.
		private void OnTriggerEnter2D(Collider2D otherCollider)
		{
			if (!otherCollider.gameObject.Equals(Sentry.gameObject) || Sentry.NextStop != transform)
			{
				return;
			}

			ForceUpdate();
		}

		// AI - Force the sentry to process reaching this stop.
		public void ForceUpdate()
		{
			Sentry.StopReached(IdleTime, NextStop);
		}
	}
}
