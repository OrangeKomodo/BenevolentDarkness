using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HellfireBlast : Spell {

	public float damageDone;
	public float speed;

	Rigidbody2D rb;

	void Start () {
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInfo>().PlaySound("Hellfire Blast");
		damageDone = FindObjectOfType<SpellCasting> ().spellLevel * 20f;
		rb = GetComponent<Rigidbody2D> ();

		rb.velocity = transform.right * speed;
	}

	void OnTriggerEnter2D(Collider2D collider){
		if (collider.tag.Equals ("Enemy")) {
			collider.GetComponent<Guard> ().TakeHit (damageDone);
			Destroy (gameObject);
		} else if (collider.tag.Equals ("Wall") || collider.tag.Equals ("Platform")) {
			Destroy (gameObject);
		}
	}
}
