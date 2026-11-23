using System.Collections;
using UnityEngine;

namespace Spells
{
	public class ShadowSink : Spell
	{
		#region Fields

		public int ManaTickCost;
		public float TransitionTime = 0.5f;
		public Color VisibleColor;
		public Color HiddenColor;
		
		private float _manaDeductTick;
		public float ManaDeductTick => _manaDeductTick;

		private SpriteRenderer _playerSpriteRenderer;
		private Vector3 _playerHidePosition;
		private bool _hidden = false;

		#endregion

		#region Initialization Functions

		private void Start()
		{
			_manaDeductTick = SpellCaster.SpellLevel * 0.5f;

			_playerSpriteRenderer = PlayerController.PlayerSprite;
			_playerHidePosition = PlayerController.transform.position;

			PlayerController.ShadowSink(true);
			StartCoroutine(Transition(VisibleColor, HiddenColor, true));
		}

		#endregion

		#region Update Functions

		private void Update()
		{
			// AI - If we're not hidden yet, wait for transition to complete.
			if (!_hidden)
			{
				return;
			}
			
			// AI - End the spell if the player moves, presses exit, or casts again.
			if (Input.GetAxis("Use Item") == 1f
			    || Input.GetButtonDown("Exit")
			    || _playerHidePosition != PlayerController.transform.position)
			{
				SpellCaster.EndSpell(SpellCasting.SpellNames.ShadowSink);
			}
		}

		#endregion

		#region Shadow Sink Functions

		// AI - End the shadow sink effect and transition back to visible.
		public void EndShadowSink()
		{
			PlayerController.ShadowSink(false);
			StartCoroutine(Transition(HiddenColor, VisibleColor, false));
			Destroy(gameObject, TransitionTime);
		}

		// AI - Gradually transition player color and visibility over time.
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
				PlayerController.VisibilityFactor = rawVisibility * (hiding ? 1f - percent : percent);

				yield return null;
			}

			_hidden = hiding;
		}

		#endregion
	}
}
