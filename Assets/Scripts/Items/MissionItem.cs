
using GameManager;
using Player;
using UI;
using UnityEngine;

namespace Items
{
	public class MissionItem : Item
	{
		public string AudioCueName = "Swipe";
		
		public int ObjectiveNumber;
		public Objective.Status NewObjectiveStatus;
		public string MissionText;

		private PlayerController _playerController;

		private void Start()
		{
			_playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		}

		public override void Interact()
		{
			_playerController.PlaySound(AudioCueName);
			ObjectiveSystem.Instance.SetObjectiveStatus(ObjectiveNumber, NewObjectiveStatus);
			InGameManagement.Instance.LoadMissionText(MissionText);
		}
	}
}
