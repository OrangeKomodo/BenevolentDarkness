using UnityEngine;

namespace DamageSystem
{
	public class LivingEntity : MonoBehaviour, IDamageable
	{

		public float startingHealth;
		[SerializeField] protected float health;
		public bool dead;

		public event System.Action OnDeath;
		public event System.Action<float, float, float> OnHit;
		//public event System.Action<float, float> OnHeal;

		protected virtual void Start()
		{
			health = startingHealth;
		}

		public void TakeHit(float damage)
		{
			health -= damage;
			if (gameObject.tag == "Player")
			{
				//FindObjectOfType<AudioManager> ().PlaySound ("Grunt_" + Random.Range (1, 10));
				OnHit(Time.time, startingHealth, health);
			}

			if (gameObject.name.Contains("Guard"))
			{
				OnHit(Time.time, startingHealth, health);
			}

			if (health <= 0 && !dead)
			{
				Die();
			}
		}

		public void Heal(float heals)
		{
			if (gameObject.tag == "Player" && health != startingHealth)
			{
				if (heals + health >= startingHealth)
					health = startingHealth;
				else
					health += heals;
			}
		}

		public bool GetStatus()
		{
			return !dead;
		}

		protected void Die()
		{
			if (!dead)
			{
				if (OnDeath != null)
				{
					OnDeath();
				}
			}
		}
	}
}
