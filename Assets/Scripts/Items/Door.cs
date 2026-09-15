using Player;
using UnityEngine;

namespace Items
{
	public class Door : Item
	{
		public string AudioCueName = "Door";

		public Door AdjoiningDoor;
		public Transform Center;
		public bool Locked;
		public string KeyName;

		private PlayerController _playerController;

		private void Start()
		{
			_playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		}

		public override void Interact()
		{
			if (AdjoiningDoor == null)
			{
				//Debug.LogError("There is no other door");
				return;
			}

			if (Locked && !_playerController.Inventory.Contains(KeyName))
			{
				//TODO: Add an audio cue for a locked door
				return;
			}
			
			_playerController.PlaySound(AudioCueName);

			Locked = false;
			AdjoiningDoor.Locked = false;

			Vector3 playerPosition = _playerController.transform.position;
			Vector3 offset = new Vector3(playerPosition.x - Center.position.x, playerPosition.y - Center.position.y);
			
			_playerController.transform.position = AdjoiningDoor.Center.position + offset;
		}
	}
}
