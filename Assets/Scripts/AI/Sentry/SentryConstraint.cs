using System.Collections.Generic;
using UnityEngine;

namespace AI.Sentry
{
	public class SentryConstraint : MonoBehaviour
	{
		public bool RightConstraint;
		
		private List<Sentry> _constrainedSentries = new List<Sentry>();

		private void Update()
		{
			if (_constrainedSentries.Count == 0)
			{
				return;
			}
			
			for (int sentryIndex = 0; sentryIndex < _constrainedSentries.Count; ++sentryIndex)
			{
				Sentry constrainedSentry = _constrainedSentries[sentryIndex];
				Transform sentryTransform = constrainedSentry.transform;
				
				if (RightConstraint)
				{
					if (sentryTransform.position.x > transform.position.x)
					{
						constrainedSentry.ConstrainEnemy(new Vector2(transform.position.x, sentryTransform.position.y));
					}

					continue;
				}
				
				if (sentryTransform.position.x < transform.position.x)
				{
					constrainedSentry.ConstrainEnemy(new Vector2(transform.position.x, sentryTransform.position.y));
				}
			}
		}

		private void OnTriggerEnter2D(Collider2D otherCollider)
		{
			if (otherCollider.name.Equals("Sentry Actual") && !_constrainedSentries.Contains(otherCollider.GetComponent<Sentry>()))
			{
				_constrainedSentries.Add(otherCollider.GetComponent<Sentry>());
			}
		}

		void OnTriggerExit2D(Collider2D otherCollider)
		{
			if (otherCollider.name.Equals("Sentry Actual") && _constrainedSentries.Contains(otherCollider.GetComponent<Sentry>()))
			{
				_constrainedSentries.Remove(otherCollider.GetComponent<Sentry>());
			}
		}
	}
}
