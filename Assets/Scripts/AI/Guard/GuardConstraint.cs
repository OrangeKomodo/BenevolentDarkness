using System.Collections.Generic;
using UnityEngine;

namespace AI.Guard
{
	public class GuardConstraint : MonoBehaviour
	{
		public bool RightConstraint;
		
		private List<Guard> _constrainedGuards = new List<Guard>();

		private void Update()
		{
			if (_constrainedGuards.Count == 0)
			{
				return;
			}
				
			for (int guardIndex = 0; guardIndex < _constrainedGuards.Count; ++guardIndex)
			{
				Guard constrainedGuard = _constrainedGuards[guardIndex];
				Transform guardTransform = constrainedGuard.transform;
				
				if (RightConstraint)
				{
					if (guardTransform.position.x > transform.position.x)
					{
						constrainedGuard.ConstrainEnemy(new Vector2(transform.position.x, guardTransform.position.y));
					}
					
					continue;
				}
				
				if (guardTransform.position.x < transform.position.x)
				{
					constrainedGuard.ConstrainEnemy(new Vector2(transform.position.x, guardTransform.position.y));
				}
			}
		}

		private void OnTriggerEnter2D(Collider2D otherCollider)
		{
			if (otherCollider.name.Equals("Guard Actual") && !_constrainedGuards.Contains(otherCollider.GetComponent<Guard>()))
			{
				_constrainedGuards.Add(otherCollider.GetComponent<Guard>());
			}
		}

		private void OnTriggerExit2D(Collider2D otherCollider)
		{
			if (otherCollider.name.Equals("Guard Actual") && _constrainedGuards.Contains(otherCollider.GetComponent<Guard>()))
			{
				_constrainedGuards.Remove(otherCollider.GetComponent<Guard>());
			}
		}
	}
}
