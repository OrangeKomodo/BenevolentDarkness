using System.Collections.Generic;
using UnityEngine;

namespace Items
{
	public class Door : Item
	{
		public Door AdjoiningDoor;
		public Transform Center;
		public bool Locked;
		public string KeyName;

		private GameObject _player;

		private void Start()
		{
			_player = GameObject.FindGameObjectWithTag("Player");
		}

		public void UseDoor(List<string> inventory)
		{
			if (AdjoiningDoor == null)
			{
				//Debug.LogError ("There is no other door");
				return;
			}

			if (Locked && inventory.Contains(KeyName))
			{
				Locked = false;
				AdjoiningDoor.Locked = false;
				Vector3 offset = new Vector3(_player.transform.position.x - Center.position.x,
					_player.transform.position.y - Center.position.y, 0f);
				_player.transform.position = AdjoiningDoor.Center.position + offset;
			}
			else if (!Locked)
			{
				Vector3 offset = new Vector3(_player.transform.position.x - Center.position.x, _player.transform.position.y - Center.position.y, 0f);
				_player.transform.position = AdjoiningDoor.Center.position + offset;
			}
		}
	}
}
