using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShadowSink : Spell {

	public int manaTickCost;
	public float manaDeductTick;

	public float transitionTime = 0.5f;
	public Color visibleColor;
	public Color hiddenColor;

	GameObject player;
	PlayerInfo playerInfo;

	Vector3 playerHidePosition;
	bool hidden = false;

	void Start () {
		manaDeductTick = FindObjectOfType<SpellCasting> ().spellLevel * 0.5f;
		player = GameObject.FindGameObjectWithTag ("Player");
		playerInfo = player.GetComponent<PlayerInfo> ();
		playerHidePosition = player.transform.position;

		playerInfo.InShadowSink (true);
		StartCoroutine (Transition (visibleColor, hiddenColor, true));
	}

	void Update () {
		if ((Input.GetAxis ("Use Item") == 1f && hidden) || playerHidePosition != player.transform.position || Input.GetButtonDown ("Exit"))
			FindObjectOfType<SpellCasting> ().EndSpell (SpellCasting.SpellNames.shadowSink);
	}

	public void EndShadowSink () {
		playerInfo.InShadowSink (false);
		StartCoroutine (Transition (hiddenColor, visibleColor, false));
		Destroy (gameObject, transitionTime);
	}

	IEnumerator Transition (Color start, Color end, bool hiding) {
        playerInfo.PlaySound("Shadow Sink");
		float rawVisibility = playerInfo.rawVisibilityFactor;

		float startTime = Time.time;
		float percent = 0;
		while (percent < 1) {
			percent=(Time.time-startTime)/transitionTime;

			player.GetComponent<SpriteRenderer> ().color = Color.Lerp (start, end, percent);

			playerInfo.visibilityFactor = hiding ? rawVisibility * (1f - percent) : rawVisibility * percent;

			yield return null;
		}
		hidden = hiding;
	}
}
