using System.Collections.Generic;
using UnityEngine;

namespace AI.Guard
{
	public class GuardConstraint : MonoBehaviour
	{
		public bool RightConstraint;
		
		private readonly List<GameObject> _constrainedGuards = new List<GameObject>();

		private void FixedUpdate()
		{
			if (_constrainedGuards.Count == 0)
			{
				return;
			}
				
			for (int i = 0; i < _constrainedGuards.Count; i++)
			{
				Transform guardTransform = _constrainedGuards[i].transform;
				
				if (RightConstraint)
				{
					if (guardTransform.position.x > transform.position.x)
					{
						guardTransform.position = new Vector2(transform.position.x, guardTransform.position.y);
					}
					
					continue;
				}
				
				if (guardTransform.position.x < transform.position.x)
				{
					guardTransform.position = new Vector2(transform.position.x, guardTransform.position.y);
				}
			}
		}

		private void OnTriggerEnter2D(Collider2D otherCollider)
		{
			if (otherCollider.name.Equals("Guard Actual") && !_constrainedGuards.Contains(otherCollider.gameObject))
			{
				_constrainedGuards.Add(otherCollider.gameObject);
			}
		}

		private void OnTriggerExit2D(Collider2D otherCollider)
		{
			if (otherCollider.name.Equals("Guard Actual") && _constrainedGuards.Contains(otherCollider.gameObject))
			{
				_constrainedGuards.Remove(otherCollider.gameObject);
			}
		}
	}
}
