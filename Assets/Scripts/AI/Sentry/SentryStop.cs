using System.Collections;
using System.Collections.Generic;
using AI.Sentry;
using UnityEngine;

public class SentryStop : MonoBehaviour
{

	public Sentry sentry;
	public float idleTime = 0f;
	public Transform nextStop;

	void OnTriggerEnter2D(Collider2D collider)
	{
		if (collider.gameObject.Equals(sentry.gameObject) && sentry.nextStop == transform)
		{
			ForceUpdate();
		}
	}

	public void ForceUpdate()
	{
		sentry.StopReached(idleTime, nextStop);
	}
}
