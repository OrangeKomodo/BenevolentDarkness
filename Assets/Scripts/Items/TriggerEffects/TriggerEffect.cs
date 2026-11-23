using Player;
using UnityEngine;

namespace Items.TriggerEffects
{
	// AI - Abstract base class for trigger effects, following Open/Closed Principle.
	// AI - New trigger types can be added without modifying existing code.
	public abstract class TriggerEffect : MonoBehaviour
	{
		public bool SingleTrigger = false;
		protected bool Triggered = false;

		// AI - Execute the trigger effect. Returns true if effect was successfully triggered.
		public bool Execute(PlayerController player)
		{
			if (SingleTrigger && Triggered)
			{
				return false;
			}

			if (!CanExecute(player))
			{
				return false;
			}

			PerformEffect(player);
			Triggered = true;
			return true;
		}

		// AI - Override to check if the effect can execute (e.g., player has required item).
		protected virtual bool CanExecute(PlayerController player)
		{
			return true;
		}

		// AI - Override to implement the specific trigger effect.
		protected abstract void PerformEffect(PlayerController player);

		// AI - Reset the trigger state (useful for quick load).
		public virtual void ResetTrigger()
		{
			Triggered = false;
		}
	}
}
