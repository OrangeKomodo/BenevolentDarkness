using System.Collections;
using Player;
using UnityEngine;

namespace Spells
{
	public class ShadowSink : Spell
	{
		public int manaTickCost;
		public float manaDeductTick;

		public float transitionTime = 0.5f;
		public Color visibleColor;
		public Color hiddenColor;

		GameObject player;
		PlayerController playerController;
		SpriteRenderer playerSpriteRenderer;

		Vector3 playerHidePosition;
		bool hidden = false;

		void Start()
		{
			manaDeductTick = FindObjectOfType<SpellCasting>().spellLevel * 0.5f;

			player = GameObject.FindGameObjectWithTag("Player");
			playerController = player.GetComponent<PlayerController>();
			playerSpriteRenderer = player.GetComponent<SpriteRenderer>();

			playerHidePosition = player.transform.position;

			playerController.InShadowSink(true);
			StartCoroutine(Transition(visibleColor, hiddenColor, true));
		}

		void Update()
		{
			if ((Input.GetAxis("Use Item") == 1f && hidden) || playerHidePosition != player.transform.position
			                                                || Input.GetButtonDown("Exit"))
			{
				FindObjectOfType<SpellCasting>().EndSpell(SpellCasting.SpellNames.shadowSink);
			}
		}

		public void EndShadowSink()
		{
			playerController.InShadowSink(false);
			StartCoroutine(Transition(hiddenColor, visibleColor, false));
			Destroy(gameObject, transitionTime);
		}

		IEnumerator Transition(Color start, Color end, bool hiding)
		{
			playerController.PlaySound("Shadow Sink");
			float rawVisibility = playerController.rawVisibilityFactor;

			float startTime = Time.time;
			float percent = 0;
			while (percent < 1)
			{
				percent = (Time.time - startTime) / transitionTime;

				playerSpriteRenderer.color = Color.Lerp(start, end, percent);

				playerController.visibilityFactor = hiding ? rawVisibility * (1f - percent) : rawVisibility * percent;

				yield return null;
			}

			hidden = hiding;
		}
	}
}
