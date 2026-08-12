using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardStop : MonoBehaviour {

	public Guard guard;
	public bool stationary;
	public float idleTime = 0f;
	public Vector2 idleDirection;
	public Transform nextStop;

	void OnTriggerEnter2D(Collider2D collider){
		if (collider.gameObject.Equals (guard.gameObject) && guard.nextStop == transform) {
			ForceUpdate ();
		}
	}

	public void ForceUpdate () {
		if (stationary)
			guard.StopReached (Mathf.Infinity, idleDirection, transform);
		else
			guard.StopReached (idleTime, idleDirection, nextStop);
	}
}
