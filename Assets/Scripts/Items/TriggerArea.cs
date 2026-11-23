using Items.TriggerEffects;
using Player;
using UnityEngine;

namespace Items
{
	// AI - Refactored to use Strategy pattern with TriggerEffect components.
	// AI - To add new trigger types, create a new TriggerEffect subclass and attach it to the GameObject.
	public class TriggerArea : MonoBehaviour
	{
		#region Fields

		private PlayerController _playerController;
		private bool _playerIn = false;
		private TriggerEffect[] _triggerEffects;

		#endregion

		#region Initialization Functions

		private void Start()
		{
			_playerController = GameObject.FindGameObjectWithTag(GameManager.GameConstants.PLAYER_TAG).GetComponent<PlayerController>();
			_triggerEffects = GetComponents<TriggerEffect>();
		}

		#endregion

		#region Trigger Functions

		// AI - Detect when the player enters the trigger area.
		private void OnTriggerEnter2D(Collider2D otherCollider)
		{
			if (!otherCollider.tag.Equals(GameManager.GameConstants.PLAYER_TAG) || _playerIn)
			{
				return;
			}

			_playerIn = true;
			ExecuteEffects();
		}

		// AI - Detect when the player exits the trigger area.
		private void OnTriggerExit2D(Collider2D otherCollider)
		{
			if (!otherCollider.tag.Equals(GameManager.GameConstants.PLAYER_TAG) || !_playerIn)
			{
				return;
			}

			_playerIn = false;
		}

		#endregion

		#region Effect Execution

		// AI - Execute all attached trigger effects in sequence.
		private void ExecuteEffects()
		{
			if (_triggerEffects == null || _triggerEffects.Length == 0)
			{
				return;
			}

			for (int i = 0; i < _triggerEffects.Length; i++)
			{
				if (_triggerEffects[i] != null)
				{
					_triggerEffects[i].Execute(_playerController);
				}
			}
		}

		#endregion

		#region Quick Save Functions

		// AI - Save the state of all trigger effects.
		public void QuickSave()
		{
			// AI - Individual TriggerEffect components manage their own state.
			// AI - No action needed here as effects track their Triggered state internally.
		}

		// AI - Load the state of all trigger effects.
		public void QuickLoad()
		{
			if (_triggerEffects == null)
			{
				return;
			}

			for (int i = 0; i < _triggerEffects.Length; i++)
			{
				if (_triggerEffects[i] != null)
				{
					_triggerEffects[i].ResetTrigger();
				}
			}
		}

		#endregion
	}
}
