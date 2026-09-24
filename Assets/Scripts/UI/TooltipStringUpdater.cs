using GameManager;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Text))]
    public class TooltipStringUpdater : InputModeListener
    {
        public string KeyboardTooltip;
        public string ControllerTooltip;
        
        private Text _textField;

        // Find the Text Field
        protected override void Awake()
        {
            base.Awake();
            
            _textField = GetComponent<Text>();
        }

        // Set the proper tooltip for the updated Input Mode
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
