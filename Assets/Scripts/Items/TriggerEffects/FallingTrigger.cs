using Player;

namespace Items.TriggerEffects
{
	// AI - Trigger effect that marks the player as falling (killed when they hit ground).
	public class FallingTrigger : TriggerEffect
	{
		protected override void PerformEffect(PlayerController player)
		{
			player.IsFalling(true);
		}
	}
}
