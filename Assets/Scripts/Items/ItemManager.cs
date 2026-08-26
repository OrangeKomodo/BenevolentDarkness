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

		public Item[] MissionItems;
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
			for (int i = 0; i < MissionItems.Length; i++)
			{
				MissionItemQuickSave.Add(new MissionItemQuickSave());
			}
			for (int i = 0; i < Doors.Length; i++)
			{
				DoorQuickSave.Add(new DoorQuickSave());
			}
			for (int i = 0; i < Buttons.Length; i++)
			{
				ButtonQuickSave.Add(new ButtonQuickSave());
			}
			for (int i = 0; i < HidingPlaces.Length; i++)
			{
				HidingPlaceQuickSave.Add(new HidingPlaceQuickSave());
			}
			for (int i = 0; i < TriggerAreas.Length; i++)
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

			for (int i = 0; i < MissionItemQuickSave.Count; i++)
			{
				MissionItemQuickSave[i].Active = MissionItems[i].gameObject.activeInHierarchy;
			}
			for (int i = 0; i < DoorQuickSave.Count; i++)
			{
				DoorQuickSave[i].Locked = Doors[i].Locked;
			}
			for (int i = 0; i < ButtonQuickSave.Count; i++)
			{
				ButtonQuickSave[i].Used = Buttons[i].Used;
			}
			for (int i = 0; i < HidingPlaceQuickSave.Count; i++)
			{
				HidingPlaceQuickSave[i].IsHiding = HidingPlaces[i].IsHiding;
			}
			for (int i = 0; i < TriggerAreaQuickSave.Count; i++)
			{
				TriggerAreaQuickSave[i].Triggered = TriggerAreas[i].Triggered;
			}
		}

		public void QuickLoad()
		{
			if (!_variablesSet)
				SetVariables();

			for (int i = 0; i < MissionItemQuickSave.Count; i++)
			{
				MissionItems[i].gameObject.SetActive(MissionItemQuickSave[i].Active);
			}
			for (int i = 0; i < DoorQuickSave.Count; i++)
			{
				Doors[i].Locked = DoorQuickSave[i].Locked;
			}

			for (int i = 0; i < ButtonQuickSave.Count; i++)
			{
				Buttons[i].Used = ButtonQuickSave[i].Used;
			}
			for (int i = 0; i < HidingPlaceQuickSave.Count; i++)
			{
				HidingPlaces[i].IsHiding = HidingPlaceQuickSave[i].IsHiding;
			}
			for (int i = 0; i < TriggerAreaQuickSave.Count; i++)
			{
				TriggerAreas[i].Triggered = TriggerAreaQuickSave[i].Triggered;
			}
		}
	}
}