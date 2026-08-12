using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonItem : Item {

	public Sprite buttonPressed;

	public int function;
	public Transform[] sentriesToDisable;
	public Transform trapdoor;
	public bool singleUse = false;
	public bool used = false;

    public void UseButton () {
		if (singleUse && !used || !singleUse) {
            if (singleUse && !used) {
                GetComponent<SpriteRenderer>().sprite = buttonPressed;
                GetComponent<BoxCollider2D>().enabled = false;
            }
			
			switch (function) {
			case 0:
				{
					//Disables the sentries in Level 1
					//Debug.Log ("Sentry disabled!");
					for (int i = 0; i < sentriesToDisable.Length; i++) {
						sentriesToDisable [i].GetComponent<Sentry> ().TakeHit (10);
					}
					break;
				}
			case 1:
				{
					//Debug.Log ("Trapdoor Opened!");
					trapdoor.gameObject.SetActive (false);
					for (int i = 0; i < sentriesToDisable.Length; i++) {
						sentriesToDisable [i].GetComponent<Sentry> ().TakeHit (10);
					}
					break;
				}
			}
			used = true;
		}
	}
}
