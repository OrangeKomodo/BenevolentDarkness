using GameManager;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	// AI - Note: This is a small utility class and doesn't require regions.
	[RequireComponent(typeof(Text))]
	public class TooltipStringUpdater : InputModeListener
	{
		public string KeyboardTooltip;
		public string ControllerTooltip;
		
		private Text _textField;

		// AI - Find the Text Field.
		protected override void Awake()
		{
			base.Awake();
			_textField = GetComponent<Text>();
		}

		// AI - Set the proper tooltip for the updated Input Mode.
		protected override void UpdateInputMode(InputManager.InputMode inputMode)
		{
			if (_textField == null)
			{
				return;
			}

			_textField.text = inputMode == InputManager.InputMode.Gamepad ? ControllerTooltip : KeyboardTooltip;
		}
	}
}
