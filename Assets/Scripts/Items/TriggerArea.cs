using GameManager;
using Player;
using UnityEngine;

namespace Items
{
	public class TriggerArea : Item
	{
		public int functionNumber;
		public bool singleTrigger = false;
		public bool triggered = false;

		PlayerController playerController;

		bool playerIn = false;

		void Start()
		{
			playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		}

		public void PerformFunction()
		{
			if (singleTrigger && !triggered || !singleTrigger)
			{
				switch (functionNumber)
				{
					case 0:
					{
						//Level 1 End
						if (playerController.inventory.Contains("MacGuffin"))
						{
							GameObject.FindGameObjectWithTag("GameController").GetComponent<ObjectiveSystem>()
								.SetObjectiveStatus(102, Objective.Status.completed);
							GameObject.FindGameObjectWithTag("GameController").GetComponent<MenuSwitcher>().LoadMenu(2);
							playerController.Freeze(true);
							triggered = true;
						}

						break;
					}
					case 1:
					{
						//Level 2 End
						if (playerController.inventory.Contains("MacGuffin2"))
						{
							GameObject.FindGameObjectWithTag("GameController").GetComponent<ObjectiveSystem>()
								.SetObjectiveStatus(202, Objective.Status.completed);
							GameObject.FindGameObjectWithTag("GameController").GetComponent<MenuSwitcher>().LoadMenu(2);
							playerController.Freeze(true);
							triggered = true;
						}

						break;
					}
					case 2:
					{
						//Level 3 End
						GameObject.FindGameObjectWithTag("GameController").GetComponent<MenuSwitcher>().LoadMenu(2);
						playerController.Freeze(true);
						triggered = true;
						break;
					}
					case 3:
					{
						//Death Area
						playerController.TakeHit(1000);
						triggered = true;
						break;
					}
					case 4:
					{
						//Falling Area
						playerController.IsFalling(true);
						triggered = true;
						break;
					}
				}
			}
		}

		void OnTriggerEnter2D(Collider2D collider)
		{
			if (collider.tag.Equals("Player") && !playerIn)
			{
				playerIn = true;
				PerformFunction();
			}
		}

		void OnTriggerExit2D(Collider2D collider)
		{
			if (collider.tag.Equals("Player"))
			{
				playerIn = false;
			}
		}
	}
}
