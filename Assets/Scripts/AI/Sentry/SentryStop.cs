using UnityEngine;

namespace AI.Sentry
{
	public class SentryStop : MonoBehaviour
	{
		public Sentry Sentry;
		public float IdleTime = 0f;
		public Transform NextStop;

		private void OnTriggerEnter2D(Collider2D otherCollider)
		{
			if (otherCollider.gameObject.Equals(Sentry.gameObject) && Sentry.NextStop == transform)
			{
				ForceUpdate();
			}
		}

		public void ForceUpdate()
		{
			Sentry.StopReached(IdleTime, NextStop);
		}
	}
}
