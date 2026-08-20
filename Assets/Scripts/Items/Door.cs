using System.Collections.Generic;
using UnityEngine;

namespace Items
{
	public class Door : Item
	{

		public Door adjoiningDoor;
		public Transform center;
		public bool locked;
		public string keyName;

		GameObject player;

		void Start()
		{
			player = GameObject.FindGameObjectWithTag("Player");
			center = transform.GetChild(0);
		}

		public void UseDoor(List<string> inventory)
		{
			if (adjoiningDoor == null)
			{
				//Debug.LogError ("There is no other door");
				return;
			}

			if (locked && inventory.Contains(keyName))
			{
				locked = false;
				adjoiningDoor.locked = false;
				Vector3 offset = new Vector3(player.transform.position.x - center.position.x,
					player.transform.position.y - center.position.y, 0f);
				player.transform.position = adjoiningDoor.center.position + offset;
			}
			else if (!locked)
			{
				Vector3 offset = new Vector3(player.transform.position.x - center.position.x, player.transform.position.y - center.position.y, 0f);
				player.transform.position = adjoiningDoor.center.position + offset;
			}
		}
	}
}
