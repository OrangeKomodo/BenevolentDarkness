using System.Collections.Generic;
using UnityEngine;

namespace Items
{
	[System.Serializable]
	public class MissionItemQuickSave {
		public bool Active;
	}

	[System.Serializable]
	public class DoorQuickSave {
		public bool Locked;
	}

	[System.Serializable]
	public class ButtonQuickSave {
		public bool Used;
	}

	[System.Serializable]
	public class HidingPlaceQuickSave {
		public bool IsHiding;
	}

	[System.Serializable]
	public class TriggerAreaQuickSave {
		public bool Triggered;
	}

	public class ItemManager : MonoBehaviour
	{
		public List<MissionItemQuickSave> MissionItemQuickSave = new List<MissionItemQuickSave>();
		public List<DoorQuickSave> DoorQuickSave = new List<DoorQuickSave>();
		public List<ButtonQuickSave> ButtonQuickSave = new List<ButtonQuickSave>();
		public List<HidingPlaceQuickSave> HidingPlaceQuickSave = new List<HidingPlaceQuickSave>();
		public List<TriggerAreaQuickSave> TriggerAreaQuickSave = new List<TriggerAreaQuickSave>();

		public MissionItem[] MissionItems;
		public Door[] Doors;
		public ButtonItem[] Buttons;
		public HidingPlace[] HidingPlaces;
		public TriggerArea[] TriggerAreas;

		private bool _variablesSet = false;

		private void Start()
		{
			SetVariables();
		}

		private void SetVariables()
		{
			for (int missionItemIndex = 0; missionItemIndex < MissionItems.Length; ++missionItemIndex)
			{
				MissionItemQuickSave.Add(new MissionItemQuickSave());
			}
			for (int doorItemIndex = 0; doorItemIndex < Doors.Length; ++doorItemIndex)
			{
				DoorQuickSave.Add(new DoorQuickSave());
			}
			for (int buttonItemIndex = 0; buttonItemIndex < Buttons.Length; ++buttonItemIndex)
			{
				ButtonQuickSave.Add(new ButtonQuickSave());
			}
			for (int hidingPlaceIndex = 0; hidingPlaceIndex < HidingPlaces.Length; ++hidingPlaceIndex)
			{
				HidingPlaceQuickSave.Add(new HidingPlaceQuickSave());
			}
			for (int triggerAreaIndex = 0; triggerAreaIndex < TriggerAreas.Length; ++triggerAreaIndex)
			{
				TriggerAreaQuickSave.Add(new TriggerAreaQuickSave());
			}

			_variablesSet = true;
		}

		public void QuickSave()
		{
			if (!_variablesSet)
			{
				SetVariables();
			}

			for (int missionItemIndex = 0; missionItemIndex < MissionItemQuickSave.Count; ++missionItemIndex)
			{
				MissionItemQuickSave[missionItemIndex].Active = MissionItems[missionItemIndex].gameObject.activeInHierarchy;
			}
			for (int doorItemIndex = 0; doorItemIndex < DoorQuickSave.Count; ++doorItemIndex)
			{
				DoorQuickSave[doorItemIndex].Locked = Doors[doorItemIndex].Locked;
			}
			for (int buttonItemIndex = 0; buttonItemIndex < ButtonQuickSave.Count; ++buttonItemIndex)
			{
				ButtonQuickSave[buttonItemIndex].Used = Buttons[buttonItemIndex].Used;
			}
			for (int hidingPlaceIndex = 0; hidingPlaceIndex < HidingPlaceQuickSave.Count; ++hidingPlaceIndex)
			{
				HidingPlaceQuickSave[hidingPlaceIndex].IsHiding = HidingPlaces[hidingPlaceIndex].IsHiding;
			}
			for (int triggerAreaIndex = 0; triggerAreaIndex < TriggerAreaQuickSave.Count; ++triggerAreaIndex)
			{
				TriggerAreaQuickSave[triggerAreaIndex].Triggered = TriggerAreas[triggerAreaIndex].Triggered;
			}
		}

		public void QuickLoad()
		{
			if (!_variablesSet)
			{
				SetVariables();
			}

			for (int missionItemIndex = 0; missionItemIndex < MissionItemQuickSave.Count; ++missionItemIndex)
			{
				MissionItems[missionItemIndex].gameObject.SetActive(MissionItemQuickSave[missionItemIndex].Active);
			}
			for (int doorItemIndex = 0; doorItemIndex < DoorQuickSave.Count; ++doorItemIndex)
			{
				Doors[doorItemIndex].Locked = DoorQuickSave[doorItemIndex].Locked;
			}
			for (int buttonItemIndex = 0; buttonItemIndex < ButtonQuickSave.Count; ++buttonItemIndex)
			{
				Buttons[buttonItemIndex].Used = ButtonQuickSave[buttonItemIndex].Used;
			}
			for (int hidingPlaceIndex = 0; hidingPlaceIndex < HidingPlaceQuickSave.Count; ++hidingPlaceIndex)
			{
				HidingPlaces[hidingPlaceIndex].IsHiding = HidingPlaceQuickSave[hidingPlaceIndex].IsHiding;
			}
			for (int triggerAreaIndex = 0; triggerAreaIndex < TriggerAreaQuickSave.Count; ++triggerAreaIndex)
			{
				TriggerAreas[triggerAreaIndex].Triggered = TriggerAreaQuickSave[triggerAreaIndex].Triggered;
			}
		}
	}
}