using GameManager;
using Player;
using UnityEngine;

namespace Items.TriggerEffects
{
	// AI - Trigger effect for completing a level when the player has a required item.
	public class LevelEndTrigger : TriggerEffect
	{
		[SerializeField] private string _requiredItem;
		[SerializeField] private int _objectiveNumber;

		protected override bool CanExecute(PlayerController player)
		{
			return player.Inventory.Contains(_requiredItem);
		}

		protected override void PerformEffect(PlayerController player)
		{
			ObjectiveSystem.Instance.SetObjectiveStatus(_objectiveNumber, Objective.Status.Completed);
			MenuSwitcher.Instance.LoadMenu(2);
			player.Freeze(true);
		}
	}
}
