using Player;
using UnityEngine;

namespace DamageSystem
{
	public class LivingEntity : MonoBehaviour
	{
		[SerializeField]
		protected float StartingHealth;
		[SerializeField]
		protected float Health;
		[SerializeField]
		protected bool Dead;

		public event System.Action OnDeath;
		public event System.Action<float, float, float> OnHit;
		public event System.Action<float, float> OnHeal;

		protected virtual void Start()
		{
			Health = StartingHealth;
		}

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

		protected virtual void Die()
		{
			if (Dead)
			{
				return;
			}
			
			Dead = true;
			OnDeath?.Invoke();
		}

		public virtual bool GetStatus()
		{
			return !Dead;
		}
	}
}
