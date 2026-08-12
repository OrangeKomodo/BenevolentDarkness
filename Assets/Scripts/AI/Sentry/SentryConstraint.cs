using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SentryConstraint : MonoBehaviour {

	public bool rightConstraint;
	public List<GameObject> sentries = new List<GameObject> ();

	void FixedUpdate () {
		if (sentries.Count > 0) {
			for (int i = 0; i < sentries.Count; i++) {
				if (rightConstraint) {
					if (sentries [i].transform.position.x > transform.position.x)
						sentries [i].transform.position = new Vector2 (transform.position.x, sentries [i].transform.position.y);
				} else {
					if (sentries [i].transform.position.x < transform.position.x)
						sentries [i].transform.position = new Vector2 (transform.position.x, sentries [i].transform.position.y);
				}
			}
		}
	}

	void OnTriggerEnter2D(Collider2D collider){
		if (collider.name.Equals ("Sentry Actual") && !sentries.Contains (collider.gameObject)) {
			sentries.Add (collider.gameObject);
		}
	}

	void OnTriggerExit2D (Collider2D collider) {
		if (collider.name.Equals ("Sentry Actual") && sentries.Contains (collider.gameObject)) {
			sentries.Remove (collider.gameObject);
		}
	}
}
