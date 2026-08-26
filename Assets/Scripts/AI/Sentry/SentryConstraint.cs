using System.Collections.Generic;
using UnityEngine;

namespace AI.Sentry
{
	public class SentryConstraint : MonoBehaviour
	{
		public bool RightConstraint;
		public List<GameObject> Sentries = new List<GameObject>();

		private void FixedUpdate()
		{
			if (Sentries.Count > 0)
			{
				for (int i = 0; i < Sentries.Count; i++)
				{
					if (RightConstraint)
					{
						if (Sentries[i].transform.position.x > transform.position.x)
						{
							Sentries[i].transform.position =
								new Vector2(transform.position.x, Sentries[i].transform.position.y);
						}
					}
					else
					{
						if (Sentries[i].transform.position.x < transform.position.x)
						{
							Sentries[i].transform.position =
								new Vector2(transform.position.x, Sentries[i].transform.position.y);
						}
					}
				}
			}
		}

		private void OnTriggerEnter2D(Collider2D collider)
		{
			if (collider.name.Equals("Sentry Actual") && !Sentries.Contains(collider.gameObject))
			{
				Sentries.Add(collider.gameObject);
			}
		}

		void OnTriggerExit2D(Collider2D collider)
		{
			if (collider.name.Equals("Sentry Actual") && Sentries.Contains(collider.gameObject))
			{
				Sentries.Remove(collider.gameObject);
			}
		}
	}
}
