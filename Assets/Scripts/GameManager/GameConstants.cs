namespace GameManager
{
	// AI - Centralized constants to replace magic numbers throughout the codebase.
	public static class GameConstants
	{
		#region Layer Constants

		public const int DROP_THROUGH_PLATFORM_LAYER = 12;
		public const int PLAYER_LAYER = 8;

		#endregion

		#region Damage Constants

		public const float INSTANT_KILL_DAMAGE = 1000f;

		#endregion

		#region Detection Constants

		public const float FULLY_SPOTTED_THRESHOLD = 1f;
		public const float HALF_SPOTTED_THRESHOLD = 0.5f;
		public const float NOT_SPOTTED_THRESHOLD = 0f;

		#endregion

		#region Tag Constants

		public const string PLAYER_TAG = "Player";
		public const string GUARD_ACTUAL_NAME = "Guard Actual";
		public const string BACKSIDE_NAME = "Backside";

		#endregion

		#region Spell Constants

		public const float FORCE_FIELD_KILL_VELOCITY = 30f;
		public const float FORCE_FIELD_KNOCKOUT_VELOCITY = 10f;

		#endregion
	}
}
