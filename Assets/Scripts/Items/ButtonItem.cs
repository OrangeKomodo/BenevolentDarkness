using AI.Sentry;
using UnityEngine;

namespace Items
{
	public class ButtonItem : Item
	{
		public Sprite ButtonPressed;

		public int Function;
		public Sentry[] SentriesToDisable;
		public Transform Trapdoor;
		public bool SingleUse = false;
		public bool Used = false;
		
		private SpriteRenderer _spriteRenderer;
		private BoxCollider2D _boxCollider2D;

		private void Start()
		{
			_spriteRenderer = GetComponent<SpriteRenderer>();
			_boxCollider2D = GetComponent<BoxCollider2D>();
		}

		public void UseButton()
		{
			if (SingleUse && Used)
			{
				return;
			}
			
			_spriteRenderer.sprite = ButtonPressed;
			_boxCollider2D.enabled = false;

			switch (Function)
			{
				case 0:
				{
					//Disables the sentries in Level 1
					//Debug.Log ("Sentry disabled!");
					for (int i = 0; i < SentriesToDisable.Length; i++)
					{
						SentriesToDisable[i].TakeHit(10);
					}

					break;
				}
				case 1:
				{
					//Debug.Log ("Trapdoor Opened!");
					Trapdoor.gameObject.SetActive(false);
					for (int i = 0; i < SentriesToDisable.Length; i++)
					{
						SentriesToDisable[i].TakeHit(10);
					}

					break;
				}
			}

			Used = true;
		}
	}
}
