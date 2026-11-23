using GameManager;
using Player;

namespace Items.TriggerEffects
{
	// AI - Trigger effect that instantly kills the player (e.g., spike pit, void area).
	public class DeathTrigger : TriggerEffect
	{
		protected override void PerformEffect(PlayerController player)
		{
			player.TakeHit(GameConstants.INSTANT_KILL_DAMAGE);
		}
	}
}
