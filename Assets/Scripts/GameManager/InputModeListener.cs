using UnityEngine;

namespace GameManager
{
    public abstract class InputModeListener : MonoBehaviour
    {
        // Shortcut to InputManager
        protected bool UsingGamepad => InputManager.UsingGamepad;
        
        // Subscribe to Device Changes on Awake
        protected virtual void Awake()
        {
            InputManager.OnDeviceChanged += UpdateInputMode;
        }

        // Update the Input Mode for child components on GameObject Enable
        protected virtual void OnEnable()
        {
            UpdateInputMode(InputManager.CurrentInputMode);
        }

        // Unsubscribe from Device Changes on Destroy
        protected virtual void OnDestroy()
        {
            InputManager.OnDeviceChanged -= UpdateInputMode;
        }

        // Overridable function to Update the Input Mode
        protected virtual void UpdateInputMode(InputManager.InputMode inputMode) { }
    }
}
