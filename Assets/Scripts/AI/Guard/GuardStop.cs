using UnityEngine;

namespace AI.Guard
{
	public class GuardStop : MonoBehaviour
	{
		public Guard Guard;
		public bool Stationary;
		public float IdleTime = 0f;
		public Vector2 IdleDirection;
		public Transform NextStop;

		private void OnTriggerEnter2D(Collider2D otherCollider)
		{
			if (otherCollider.gameObject.Equals(Guard.gameObject) && Guard.NextStop == transform)
			{
				ForceUpdate();
			}
		}

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
