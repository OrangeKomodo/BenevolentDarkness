using AI.Guard;
using GameManager;
using Player;
using UnityEngine;

namespace Spells
{
	// AI - Note: This is a small class and doesn't require extensive regions.
	public class HellfireBlast : Spell
	{
		#region Fields

		public float DamageDone;
		public float Speed;

		private Rigidbody2D _rigidbody;

		#endregion

		#region Initialization Functions

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

		#endregion

		#region Collision Functions

		// AI - Damage enemies or destroy on contact with walls/platforms.
		private void OnTriggerEnter2D(Collider2D otherCollider)
		{
			if (otherCollider == null)
			{
				return;
			}

			if (otherCollider.tag.Equals("Enemy"))
			{
				HandleEnemyHit(otherCollider);
				return;
			}
			
			if (otherCollider.tag.Equals("Wall") || otherCollider.tag.Equals("Platform"))
			{
				Destroy(gameObject);
			}
		}

		// AI - Apply damage to the guard if they're alive.
		private void HandleEnemyHit(Collider2D enemyCollider)
		{
			Guard guard = enemyCollider.GetComponent<Guard>();

			if (guard == null || guard.State == Guard.GuardState.Dead)
			{
				return;
			}
			
			guard.TakeHit(DamageDone);
			Destroy(gameObject);
		}

		#endregion
	}
}
