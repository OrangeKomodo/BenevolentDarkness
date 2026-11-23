using UnityEngine;

namespace Events
{
	// AI - Event system for player-related events.
	public static class PlayerEvents
	{
		// AI - Fired when player visibility changes significantly.
		public delegate void VisibilityChangedHandler(float visibilityFactor);
		public static event VisibilityChangedHandler OnVisibilityChanged;

		// AI - Fired when player enters/exits hiding.
		public delegate void HidingStateChangedHandler(bool isHiding);
		public static event HidingStateChangedHandler OnHidingStateChanged;

		// AI - Fired when player enters/exits disguise.
		public delegate void DisguiseStateChangedHandler(bool isDisguised);
		public static event DisguiseStateChangedHandler OnDisguiseStateChanged;

		// AI - Trigger visibility change event.
		public static void TriggerVisibilityChanged(float visibilityFactor)
		{
			OnVisibilityChanged?.Invoke(visibilityFactor);
		}

		// AI - Trigger hiding state change event.
		public static void TriggerHidingStateChanged(bool isHiding)
		{
			OnHidingStateChanged?.Invoke(isHiding);
		}

		// AI - Trigger disguise state change event.
		public static void TriggerDisguiseStateChanged(bool isDisguised)
		{
			OnDisguiseStateChanged?.Invoke(isDisguised);
		}
	}
}
