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
		public MissionItem[] MissionItems;
		public Door[] Doors;
		public ButtonItem[] Buttons;
		public HidingPlace[] HidingPlaces;
		public TriggerArea[] TriggerAreas;

		private List<MissionItemQuickSave> _missionItemQuickSave = new List<MissionItemQuickSave>();
		private List<DoorQuickSave> _doorQuickSave = new List<DoorQuickSave>();
		private List<ButtonQuickSave> _buttonQuickSave = new List<ButtonQuickSave>();
		private List<HidingPlaceQuickSave> _hidingPlaceQuickSave = new List<HidingPlaceQuickSave>();
		private List<TriggerAreaQuickSave> _triggerAreaQuickSave = new List<TriggerAreaQuickSave>();

		private bool _variablesSet = false;

		private void Start()
		{
			SetVariables();
		}

		private void SetVariables()
		{
			for (int missionItemIndex = 0; missionItemIndex < MissionItems.Length; ++missionItemIndex)
			{
				_missionItemQuickSave.Add(new MissionItemQuickSave());
			}
			for (int doorItemIndex = 0; doorItemIndex < Doors.Length; ++doorItemIndex)
			{
				_doorQuickSave.Add(new DoorQuickSave());
			}
			for (int buttonItemIndex = 0; buttonItemIndex < Buttons.Length; ++buttonItemIndex)
			{
				_buttonQuickSave.Add(new ButtonQuickSave());
			}
			for (int hidingPlaceIndex = 0; hidingPlaceIndex < HidingPlaces.Length; ++hidingPlaceIndex)
			{
				_hidingPlaceQuickSave.Add(new HidingPlaceQuickSave());
			}
			for (int triggerAreaIndex = 0; triggerAreaIndex < TriggerAreas.Length; ++triggerAreaIndex)
			{
				_triggerAreaQuickSave.Add(new TriggerAreaQuickSave());
			}

			_variablesSet = true;
		}

		public void QuickSave()
		{
			if (!_variablesSet)
			{
				SetVariables();
			}

			for (int missionItemIndex = 0; missionItemIndex < _missionItemQuickSave.Count; ++missionItemIndex)
			{
				_missionItemQuickSave[missionItemIndex].Active = MissionItems[missionItemIndex].gameObject.activeInHierarchy;
			}
			for (int doorItemIndex = 0; doorItemIndex < _doorQuickSave.Count; ++doorItemIndex)
			{
				_doorQuickSave[doorItemIndex].Locked = Doors[doorItemIndex].Locked;
			}
			for (int buttonItemIndex = 0; buttonItemIndex < _buttonQuickSave.Count; ++buttonItemIndex)
			{
				_buttonQuickSave[buttonItemIndex].Used = Buttons[buttonItemIndex].Used;
			}
			for (int hidingPlaceIndex = 0; hidingPlaceIndex < _hidingPlaceQuickSave.Count; ++hidingPlaceIndex)
			{
				_hidingPlaceQuickSave[hidingPlaceIndex].IsHiding = HidingPlaces[hidingPlaceIndex].IsHiding;
			}
			for (int triggerAreaIndex = 0; triggerAreaIndex < _triggerAreaQuickSave.Count; ++triggerAreaIndex)
			{
				_triggerAreaQuickSave[triggerAreaIndex].Triggered = TriggerAreas[triggerAreaIndex].Triggered;
			}
		}

		public void QuickLoad()
		{
			if (!_variablesSet)
			{
				SetVariables();
			}

			for (int missionItemIndex = 0; missionItemIndex < _missionItemQuickSave.Count; ++missionItemIndex)
			{
				MissionItems[missionItemIndex].gameObject.SetActive(_missionItemQuickSave[missionItemIndex].Active);
			}
			for (int doorItemIndex = 0; doorItemIndex < _doorQuickSave.Count; ++doorItemIndex)
			{
				Doors[doorItemIndex].Locked = _doorQuickSave[doorItemIndex].Locked;
			}
			for (int buttonItemIndex = 0; buttonItemIndex < _buttonQuickSave.Count; ++buttonItemIndex)
			{
				Buttons[buttonItemIndex].Used = _buttonQuickSave[buttonItemIndex].Used;
			}
			for (int hidingPlaceIndex = 0; hidingPlaceIndex < _hidingPlaceQuickSave.Count; ++hidingPlaceIndex)
			{
				HidingPlaces[hidingPlaceIndex].IsHiding = _hidingPlaceQuickSave[hidingPlaceIndex].IsHiding;
			}
			for (int triggerAreaIndex = 0; triggerAreaIndex < _triggerAreaQuickSave.Count; ++triggerAreaIndex)
			{
				TriggerAreas[triggerAreaIndex].Triggered = _triggerAreaQuickSave[triggerAreaIndex].Triggered;
			}
		}
	}
}