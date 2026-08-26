using System.Collections;
using UnityEngine;

namespace Spells
{
	public class ShadowSink : Spell
	{
		public int ManaTickCost;
		public float ManaDeductTick;

		public float TransitionTime = 0.5f;
		public Color VisibleColor;
		public Color HiddenColor;

		private SpriteRenderer _playerSpriteRenderer;

		private Vector3 _playerHidePosition;
		private bool _hidden = false;

		private void Start()
		{
			ManaDeductTick = SpellCaster.SpellLevel * 0.5f;

			_playerSpriteRenderer = PlayerController.PlayerSprite;
			_playerHidePosition = PlayerController.transform.position;

			PlayerController.ShadowSink(true);
			StartCoroutine(Transition(VisibleColor, HiddenColor, true));
		}

		private void Update()
		{
			if ((Input.GetAxis("Use Item") == 1f && _hidden)
			    || _playerHidePosition != PlayerController.transform.position || Input.GetButtonDown("Exit"))
			{
				SpellCaster.EndSpell(SpellCasting.SpellNames.ShadowSink);
			}
		}

		public void EndShadowSink()
		{
			PlayerController.ShadowSink(false);
			StartCoroutine(Transition(HiddenColor, VisibleColor, false));
			Destroy(gameObject, TransitionTime);
		}

		private IEnumerator Transition(Color start, Color end, bool hiding)
		{
			PlayerController.PlaySound("Shadow Sink");
			float rawVisibility = PlayerController.RawVisibilityFactor;

			float startTime = Time.time;
			float percent = 0;
			while (percent < 1)
			{
				percent = (Time.time - startTime) / TransitionTime;

				_playerSpriteRenderer.color = Color.Lerp(start, end, percent);

				PlayerController.VisibilityFactor = hiding ? rawVisibility * (1f - percent) : rawVisibility * percent;

				yield return null;
			}

			_hidden = hiding;
		}
	}
}
