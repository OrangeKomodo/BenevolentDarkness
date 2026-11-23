using Player;
using UnityEngine;

namespace Items
{
	public class Door : Item
	{
		#region Fields

		public string AudioCueName = "Door";

		public Door AdjoiningDoor;
		public Transform Center;
		public bool Locked;
		public string KeyName;

		private PlayerController _playerController;

		#endregion

		#region Initialization Functions

		private void Start()
		{
			_playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		}

		#endregion

		#region Interaction Functions

		// AI - Teleport the player through the door to the adjoining door, maintaining relative position.
		// AI - If the door is locked, check for the required key in the player's inventory first.
		public override void Interact()
		{
			if (AdjoiningDoor == null)
			{
				// AI - No adjoining door configured.
				return;
			}

			if (Locked && !_playerController.Inventory.Contains(KeyName))
			{
				// AI - TODO: Add an audio cue for a locked door.
				return;
			}
			
			_playerController.PlaySound(AudioCueName);

			Locked = false;
			AdjoiningDoor.Locked = false;

			Vector3 playerPosition = _playerController.transform.position;
			Vector3 offset = new Vector3(playerPosition.x - Center.position.x, playerPosition.y - Center.position.y);
			
			_playerController.transform.position = AdjoiningDoor.Center.position + offset;
		}

		#endregion
	}
}
