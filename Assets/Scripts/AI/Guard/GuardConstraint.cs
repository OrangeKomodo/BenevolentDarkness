using System.Collections.Generic;
using UnityEngine;

namespace AI.Guard
{
	public class GuardConstraint : MonoBehaviour
	{
		#region Fields

		public bool RightConstraint;
		
		private List<Guard> _constrainedGuards = new List<Guard>();

		#endregion

		#region Update Functions

		private void Update()
		{
			if (_constrainedGuards.Count == 0)
			{
				return;
			}
				
			for (int guardIndex = 0; guardIndex < _constrainedGuards.Count; ++guardIndex)
			{
				ConstrainGuard(_constrainedGuards[guardIndex]);
			}
		}

		// AI - Prevent a guard from moving beyond this constraint boundary.
		private void ConstrainGuard(Guard constrainedGuard)
		{
			Transform guardTransform = constrainedGuard.transform;
			
			if (RightConstraint)
			{
				if (guardTransform.position.x > transform.position.x)
				{
					constrainedGuard.ConstrainEnemy(new Vector2(transform.position.x, guardTransform.position.y));
				}
				return;
			}
			
			if (guardTransform.position.x < transform.position.x)
			{
				constrainedGuard.ConstrainEnemy(new Vector2(transform.position.x, guardTransform.position.y));
			}
		}

		#endregion

		#region Collision Functions

		// AI - Add a guard to the constrained list when they enter the trigger.
		private void OnTriggerEnter2D(Collider2D otherCollider)
		{
			if (!otherCollider.name.Equals(GameManager.GameConstants.GUARD_ACTUAL_NAME) || _constrainedGuards.Contains(otherCollider.GetComponent<Guard>()))
			{
				return;
			}

			_constrainedGuards.Add(otherCollider.GetComponent<Guard>());
		}

		// AI - Remove a guard from the constrained list when they exit the trigger.
		private void OnTriggerExit2D(Collider2D otherCollider)
		{
			if (!otherCollider.name.Equals(GameManager.GameConstants.GUARD_ACTUAL_NAME) || !_constrainedGuards.Contains(otherCollider.GetComponent<Guard>()))
			{
				return;
			}

			_constrainedGuards.Remove(otherCollider.GetComponent<Guard>());
		}

		#endregion
	}
}
