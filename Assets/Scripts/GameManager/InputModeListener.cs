using UnityEngine;

namespace GameManager
{
	// AI - Abstract base class for components that need to respond to input mode changes.
	public abstract class InputModeListener : MonoBehaviour
	{
		#region Properties

		// AI - Shortcut to InputManager.
		protected bool UsingGamepad => InputManager.UsingGamepad;

		#endregion

		#region Lifecycle Functions

		// AI - Subscribe to Device Changes on Awake.
		protected virtual void Awake()
		{
			InputManager.OnDeviceChanged += UpdateInputMode;
		}

		// AI - Update the Input Mode for child components on GameObject Enable.
		protected virtual void OnEnable()
		{
			UpdateInputMode(InputManager.CurrentInputMode);
		}

		// AI - Unsubscribe from Device Changes on Destroy.
		protected virtual void OnDestroy()
		{
			InputManager.OnDeviceChanged -= UpdateInputMode;
		}

		#endregion

		#region Virtual Functions

		// AI - Overridable function to Update the Input Mode.
		protected virtual void UpdateInputMode(InputManager.InputMode inputMode) { }

		#endregion
	}
}
