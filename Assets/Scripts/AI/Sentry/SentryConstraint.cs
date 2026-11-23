using System.Collections.Generic;
using UnityEngine;

namespace AI.Sentry
{
	public class SentryConstraint : MonoBehaviour
	{
		#region Fields

		public bool RightConstraint;
		
		private List<Sentry> _constrainedSentries = new List<Sentry>();

		private const string SENTRY_ACTUAL_NAME = "Sentry Actual";

		#endregion

		#region Update Functions

		private void Update()
		{
			if (_constrainedSentries.Count == 0)
			{
				return;
			}
			
			for (int sentryIndex = 0; sentryIndex < _constrainedSentries.Count; ++sentryIndex)
			{
				ConstrainSentry(_constrainedSentries[sentryIndex]);
			}
		}

		// AI - Prevent a sentry from moving beyond this constraint boundary.
		private void ConstrainSentry(Sentry constrainedSentry)
		{
			if (constrainedSentry == null)
			{
				return;
			}

			Transform sentryTransform = constrainedSentry.transform;
			
			if (RightConstraint)
			{
				if (sentryTransform.position.x > transform.position.x)
				{
					constrainedSentry.ConstrainEnemy(new Vector2(transform.position.x, sentryTransform.position.y));
				}
				return;
			}
			
			if (sentryTransform.position.x < transform.position.x)
			{
				constrainedSentry.ConstrainEnemy(new Vector2(transform.position.x, sentryTransform.position.y));
			}
		}

		#endregion

		#region Collision Functions

		// AI - Add a sentry to the constrained list when they enter the trigger.
		private void OnTriggerEnter2D(Collider2D otherCollider)
		{
			if (!otherCollider.name.Equals(SENTRY_ACTUAL_NAME))
			{
				return;
			}

			Sentry sentry = otherCollider.GetComponent<Sentry>();
			if (sentry != null && !_constrainedSentries.Contains(sentry))
			{
				_constrainedSentries.Add(sentry);
			}
		}

		// AI - Remove a sentry from the constrained list when they exit the trigger.
		private void OnTriggerExit2D(Collider2D otherCollider)
		{
			if (!otherCollider.name.Equals(SENTRY_ACTUAL_NAME))
			{
				return;
			}

			Sentry sentry = otherCollider.GetComponent<Sentry>();
			if (sentry != null && _constrainedSentries.Contains(sentry))
			{
				_constrainedSentries.Remove(sentry);
			}
		}

		#endregion
	}
}
