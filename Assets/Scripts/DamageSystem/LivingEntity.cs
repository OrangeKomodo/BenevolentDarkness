using Interfaces;
using UnityEngine;

namespace DamageSystem
{
	public class LivingEntity : MonoBehaviour, IAttackable
	{
		#region Fields

		[SerializeField]
		protected float StartingHealth;
		[SerializeField]
		protected float Health;
		[SerializeField]
		protected bool Dead;

		public event System.Action OnDeath;
		public event System.Action<float, float, float> OnHit;
		public event System.Action<float, float> OnHeal;

		#endregion

		#region Properties

		// AI - IAttackable implementation
		public bool IsDead => Dead;
		float IAttackable.Health => Health;

		#endregion

		#region Initialization Functions

		protected virtual void Start()
		{
			Health = StartingHealth;
		}

		#endregion

		#region Damage Functions

		// AI - Apply damage to the entity. If health reaches zero or below, the entity dies.
		public virtual void TakeHit(float damage)
		{
			if (Dead)
			{
				return;
			}

			Health -= damage;
			OnHit?.Invoke(Time.time, StartingHealth, Health);

			if (Health <= 0)
			{
				Die();
			}
		}

		#endregion

		#region Healing Functions

		// AI - Restore health to the entity. Health cannot exceed the starting maximum.
		public virtual void Heal(float heals)
		{
			if (Dead)
			{
				return;
			}

			if (Health >= StartingHealth)
			{
				return;
			}

			OnHeal?.Invoke(StartingHealth, Health);
			
			if (heals + Health >= StartingHealth)
			{
				Health = StartingHealth;
				return;
			}
			
			Health += heals;
		}

		#endregion

		#region Death Functions

		// AI - Mark the entity as dead and invoke the death event.
		protected virtual void Die()
		{
			if (Dead)
			{
				return;
			}
			
			Dead = true;
			OnDeath?.Invoke();
		}

		#endregion

		#region Status Functions

		// AI - Returns true if the entity is alive, false if dead.
		public virtual bool GetStatus()
		{
			return !Dead;
		}

		#endregion
	}
}
