using AI.Sentry;
using Player;
using UnityEngine;

namespace Items
{
	public class ButtonItem : Item
	{
		public Sprite ButtonPressed;
		public string AudioCueName = "Use Button";

		public Sentry[] SentriesToDisable;
		public Transform Trapdoor;
		public bool SingleUse = false;
		public bool Used = false;

		private PlayerController _playerController;
		
		private SpriteRenderer _spriteRenderer;
		private BoxCollider2D _boxCollider2D;

		private void Start()
		{
			_playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
			
			_spriteRenderer = GetComponent<SpriteRenderer>();
			_boxCollider2D = GetComponent<BoxCollider2D>();
		}

		public override void Interact()
		{
			if (SingleUse && Used)
			{
				return;
			}
			
			_playerController.PlaySound(AudioCueName);
			
			_spriteRenderer.sprite = ButtonPressed;
			_boxCollider2D.enabled = false;

			if (Trapdoor != null)
			{
				Trapdoor.gameObject.SetActive(false);
			}
			
			for (int sentryIndex = 0; sentryIndex < SentriesToDisable.Length; ++sentryIndex)
			{
				SentriesToDisable[sentryIndex].TakeHit(100);
			}

			Used = true;
		}
	}
}
