using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace GameManager
{
    public class InputManager : Singleton<InputManager>
    {
        public enum InputMode
        {
            Keyboard,
            Gamepad
        }

        [SerializeField] private float MouseMoveThreshold = 2f;
        [SerializeField, Range(0f, 1f)] private float StickDeadzone = 0.25f;
        [SerializeField] private bool NotifyOnStart = true;
        
        public static Action<InputMode> OnDeviceChanged;

        private static bool _usingGamepad;
        public static bool UsingGamepad
        {
            get
            {
                if (Instance == null)
                {
                    return false;
                }

                return _usingGamepad;
            }
        }
        
        private static InputMode _currentInputMode;
        public static InputMode CurrentInputMode
        {
            get
            {
                if (Instance == null)
                {
                    return InputMode.Keyboard;
                }
                
                return _currentInputMode;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            
            _currentInputMode = InputMode.Keyboard;
        }

        private void Start()
        {
            if (NotifyOnStart)
            {
                SetNewInputMode(_currentInputMode);
            }
        }

        private void Update()
        {
            InputMode newInputMode = ProcessInputMode();

            if (newInputMode != _currentInputMode)
            {
                SetNewInputMode(newInputMode);
            }
        }

        // Check for new inputs
        private InputMode ProcessInputMode()
        {
            // If currently using Keyboard and Mouse check for Gamepad inputs
            if (_currentInputMode == InputMode.Keyboard && WasGamepadUsedThisFrame())
            {
                return InputMode.Gamepad;
            }
            
            // If currently using Gamepad check for Keyboard and Mouse inputs
            if (_currentInputMode == InputMode.Gamepad && WasKeyboardOrMouseUsedThisFrame())
            {
                return InputMode.Keyboard;
            }

            // Return the current Input Mode if nothing has changed
            return _currentInputMode;
        }

        private bool WasKeyboardOrMouseUsedThisFrame()
        {
            // Detect Keyboard inputs
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.anyKey.wasPressedThisFrame)
            {
                return true;
            }

            // Detect Mouse Inputs
            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                // Button presses
                if (mouse.leftButton.wasPressedThisFrame
                    || mouse.rightButton.wasPressedThisFrame
                    || mouse.middleButton.wasPressedThisFrame)
                {
                    return true;
                }

                // Movement
                if (mouse.delta.ReadValue().sqrMagnitude > MouseMoveThreshold * MouseMoveThreshold)
                {
                    return true;
                }

                // Scroll Wheel
                if (mouse.scroll.ReadValue().sqrMagnitude > 0.01f)
                {
                    return true;
                }
            }

            return false;
        }

        private bool WasGamepadUsedThisFrame()
        {
            // Cycle through all Gamepads
            foreach (Gamepad gamepad in Gamepad.all)
            {
                // Detect joystick movement
                float deadzoneSqr = StickDeadzone * StickDeadzone;
                if (gamepad.leftStick.ReadValue().sqrMagnitude > deadzoneSqr
                    || gamepad.rightStick.ReadValue().sqrMagnitude > deadzoneSqr)
                {
                    return true;
                }

                // Detect button presses
                foreach (InputControl control in gamepad.allControls)
                {
                    ButtonControl buttonControl = control as ButtonControl;

                    if (buttonControl == null)
                    {
                        continue;
                    }
                    
                    if (!buttonControl.synthetic && buttonControl.wasPressedThisFrame)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        // Set new Input Mode and invoke listening functions
        private void SetNewInputMode(InputMode newInputMode)
        {
            OnDeviceChanged?.Invoke(newInputMode);
            _usingGamepad = newInputMode == InputMode.Gamepad;
            _currentInputMode = newInputMode;
        }
    }
}
