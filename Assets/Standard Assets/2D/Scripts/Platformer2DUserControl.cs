
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets._2D
{
    [RequireComponent(typeof (PlatformerCharacter2D))]
    public class Platformer2DUserControl : MonoBehaviour
    {
        private PlatformerCharacter2D m_Character;
        private bool m_Jump;

		private bool _crouch = false;
        private bool _sprint = false;

        private void Awake()
        {
            m_Character = GetComponent<PlatformerCharacter2D>();
        }


        private void Update()
        {
            if (!m_Jump)
            {
                // Read the jump input in Update so button presses aren't missed.
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }
        }


        private void FixedUpdate()
        {
            // Read the inputs.
			if (Input.GetKeyDown (KeyCode.LeftShift) && _crouch)
				_crouch = false;
			//else if (Input.GetKeyDown (KeyCode.C))
			//	crouch = !crouch;
			_sprint = Input.GetKey (KeyCode.LeftShift);

            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            // Pass all parameters to the character control script.
			m_Character.Move(h, _sprint, _crouch, m_Jump);
            m_Jump = false;
        }
    }
}
