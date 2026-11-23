using UnityEngine;

namespace Events
{
	// AI - Event system for combat interactions, reducing direct coupling between entities.
	public static class CombatEvents
	{
		// AI - Fired when any entity takes damage from any source.
		public delegate void DamageDealtHandler(MonoBehaviour attacker, MonoBehaviour target, float damage);
		public static event DamageDealtHandler OnDamageDealt;

		// AI - Fired when an entity heals.
		public delegate void HealingAppliedHandler(MonoBehaviour target, float healAmount);
		public static event HealingAppliedHandler OnHealingApplied;

		// AI - Fired when an entity dies.
		public delegate void EntityDiedHandler(MonoBehaviour entity);
		public static event EntityDiedHandler OnEntityDied;

		// AI - Trigger damage event.
		public static void TriggerDamage(MonoBehaviour attacker, MonoBehaviour target, float damage)
		{
			OnDamageDealt?.Invoke(attacker, target, damage);
		}

		// AI - Trigger healing event.
		public static void TriggerHealing(MonoBehaviour target, float healAmount)
		{
			OnHealingApplied?.Invoke(target, healAmount);
		}

		// AI - Trigger death event.
		public static void TriggerDeath(MonoBehaviour entity)
		{
			OnEntityDied?.Invoke(entity);
		}
	}
}
