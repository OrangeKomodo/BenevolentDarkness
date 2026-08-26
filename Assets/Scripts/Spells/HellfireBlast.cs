using AI.Guard;
using GameManager;
using Player;
using UnityEngine;

namespace Spells
{
	public class HellfireBlast : Spell
	{
		public float DamageDone;
		public float Speed;

		private Rigidbody2D _rigidbody;

		public override void Init(PlayerController playerController, SpellCasting spellCaster)
		{
			base.Init(playerController, spellCaster);
			DamageDone *= spellCaster.SpellLevel;
		}

		private void Start()
		{
			AudioManager.Instance.PlaySound("Hellfire Blast");
			_rigidbody = GetComponent<Rigidbody2D>();

			_rigidbody.linearVelocity = transform.right * Speed;
		}

		private void OnTriggerEnter2D(Collider2D otherCollider)
		{
			if (otherCollider.tag.Equals("Enemy"))
			{
				otherCollider.GetComponent<Guard>().TakeHit(DamageDone);
				Destroy(gameObject);
			}
			else if (otherCollider.tag.Equals("Wall") || otherCollider.tag.Equals("Platform"))
			{
				Destroy(gameObject);
			}
		}
	}
}
