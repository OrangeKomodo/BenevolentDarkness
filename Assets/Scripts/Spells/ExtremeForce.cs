using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtremeForce : Spell {

	BoxCollider2D boxCollider;

	void Start () {
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInfo>().PlaySound("Extreme Force");

        boxCollider = GetComponent<BoxCollider2D> ();

        float wallDistance = Physics2D.Raycast(transform.position, transform.right, 200f, LayerMask.GetMask("Walls")).distance - 3f;
		boxCollider.size = new Vector2 (wallDistance, boxCollider.size.y);
		boxCollider.offset = new Vector2 (wallDistance / 2f, boxCollider.offset.y);

		GetComponent<AreaEffector2D> ().forceAngle = transform.rotation.y == 1f ? 180 : 0;
	}
}
