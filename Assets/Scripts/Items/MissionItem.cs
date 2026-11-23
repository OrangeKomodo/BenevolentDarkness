using GameManager;
using Player;
using UI;
using UnityEngine;

namespace Items
{
	// AI - Note: This is a small class and doesn't require regions.
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

		// AI - Collect the mission item, play audio feedback, update objectives, and display mission text.
		public override void Interact()
		{
			_playerController.PlaySound(AudioCueName);
			ObjectiveSystem.Instance.SetObjectiveStatus(ObjectiveNumber, NewObjectiveStatus);
			InGameManagement.Instance.LoadMissionText(MissionText);
		}
	}
}
