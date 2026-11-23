namespace Interfaces
{
	// AI - Interface for entities that can receive damage and die.
	public interface IAttackable
	{
		void TakeHit(float damage);
		void Heal(float heals);
		bool IsDead { get; }
		float Health { get; }
	}
}
