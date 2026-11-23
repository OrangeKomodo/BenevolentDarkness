using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace GameManager
{
	public class InputManager : Singleton<InputManager>
	{
		#region Enums

		public enum InputMode
		{
			Keyboard,
			Gamepad
		}

		#endregion

		#region Fields

		[SerializeField] private float MouseMoveThreshold = 2f;
		[SerializeField, Range(0f, 1f)] private float StickDeadzone = 0.25f;
		[SerializeField] private bool NotifyOnStart = true;
		
		public static Action<InputMode> OnDeviceChanged;

		private static bool _usingGamepad;
		private static InputMode _currentInputMode;

		#endregion

		#region Properties

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

		#endregion

		#region Initialization Functions

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

		#endregion

		#region Update Functions

		private void Update()
		{
			InputMode newInputMode = ProcessInputMode();

			if (newInputMode != _currentInputMode)
			{
				SetNewInputMode(newInputMode);
			}
		}

		#endregion

		#region Input Detection Functions

		// AI - Check for new inputs and determine if the input mode has changed.
		private InputMode ProcessInputMode()
		{
			// AI - If currently using Keyboard and Mouse check for Gamepad inputs.
			if (_currentInputMode == InputMode.Keyboard && WasGamepadUsedThisFrame())
			{
				return InputMode.Gamepad;
			}
			
			// AI - If currently using Gamepad check for Keyboard and Mouse inputs.
			if (_currentInputMode == InputMode.Gamepad && WasKeyboardOrMouseUsedThisFrame())
			{
				return InputMode.Keyboard;
			}

			return _currentInputMode;
		}

		// AI - Detect if keyboard or mouse was used this frame.
		private bool WasKeyboardOrMouseUsedThisFrame()
		{
			if (WasKeyboardUsed())
			{
				return true;
			}

			if (WasMouseUsed())
			{
				return true;
			}

			return false;
		}

		// AI - Detect Keyboard inputs.
		private bool WasKeyboardUsed()
		{
			Keyboard keyboard = Keyboard.current;
			return keyboard != null && keyboard.anyKey.wasPressedThisFrame;
		}

		// AI - Detect Mouse inputs (buttons, movement, or scroll wheel).
		private bool WasMouseUsed()
		{
			Mouse mouse = Mouse.current;
			if (mouse == null)
			{
				return false;
			}

			// AI - Button presses.
			if (mouse.leftButton.wasPressedThisFrame
			    || mouse.rightButton.wasPressedThisFrame
			    || mouse.middleButton.wasPressedThisFrame)
			{
				return true;
			}

			// AI - Movement.
			if (mouse.delta.ReadValue().sqrMagnitude > MouseMoveThreshold * MouseMoveThreshold)
			{
				return true;
			}

			// AI - Scroll Wheel.
			if (mouse.scroll.ReadValue().sqrMagnitude > 0.01f)
			{
				return true;
			}

			return false;
		}

		// AI - Detect gamepad inputs from any connected gamepad.
		private bool WasGamepadUsedThisFrame()
		{
			foreach (Gamepad gamepad in Gamepad.all)
			{
				if (gamepad == null)
				{
					continue;
				}

				if (WasGamepadJoystickMoved(gamepad))
				{
					return true;
				}

				if (WasGamepadButtonPressed(gamepad))
				{
					return true;
				}
			}

			return false;
		}

		// AI - Detect joystick movement on a gamepad.
		private bool WasGamepadJoystickMoved(Gamepad gamepad)
		{
			float deadzoneSqr = StickDeadzone * StickDeadzone;
			return gamepad.leftStick.ReadValue().sqrMagnitude > deadzoneSqr
			       || gamepad.rightStick.ReadValue().sqrMagnitude > deadzoneSqr;
		}

		// AI - Detect button presses on a gamepad.
		private bool WasGamepadButtonPressed(Gamepad gamepad)
		{
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

			return false;
		}

		#endregion

		#region Input Mode Management Functions

		// AI - Set new Input Mode and invoke listening functions.
		private void SetNewInputMode(InputMode newInputMode)
		{
			OnDeviceChanged?.Invoke(newInputMode);
			_usingGamepad = newInputMode == InputMode.Gamepad;
			_currentInputMode = newInputMode;
		}

		#endregion
	}
}
