namespace AI.Guard.States
{
	// AI - Abstract base class for Guard state machine. Each state handles its own logic.
	public abstract class GuardState
	{
		protected Guard Guard;

		// AI - Called once when entering this state.
		public virtual void Enter(Guard guard)
		{
			Guard = guard;
		}

		// AI - Called every frame while in this state.
		public abstract void Update();

		// AI - Called once when exiting this state.
		public virtual void Exit()
		{
		}

		// AI - Handle state-specific physics updates.
		public virtual void FixedUpdate()
		{
		}
	}
}
