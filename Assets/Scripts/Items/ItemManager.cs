using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MissionItemQuickSave {
	public bool active;
}

[System.Serializable]
public class DoorQuickSave {
	public bool locked;
}

[System.Serializable]
public class ButtonQuickSave {
	public bool used;
}

[System.Serializable]
public class HidingPlaceQuickSave {
	public bool isHiding;
}

[System.Serializable]
public class TriggerAreaQuickSave {
	public bool triggered;
}

public class ItemManager : MonoBehaviour {

	public List<MissionItemQuickSave> missionItemQuickSave = new List<MissionItemQuickSave> ();
	public List<DoorQuickSave> doorQuickSave = new List<DoorQuickSave> ();
	public List<ButtonQuickSave> buttonQuickSave = new List<ButtonQuickSave> ();
	public List<HidingPlaceQuickSave> hidingPlaceQuickSave = new List<HidingPlaceQuickSave> ();
	public List<TriggerAreaQuickSave> triggerAreaQuickSave = new List<TriggerAreaQuickSave> ();

	Transform missionItemHolder;
	Transform doorHolder;
	Transform buttonHolder;
	Transform hidingPlaceHolder;
	Transform triggerAreaHolder;

	bool variablesSet = false;

	void Start () {
		SetVariables ();
	}

	void SetVariables () {
		missionItemHolder = transform.GetChild (0);
		doorHolder = transform.GetChild (1);
		buttonHolder = transform.GetChild (2);
		hidingPlaceHolder = transform.GetChild (3);
		triggerAreaHolder = transform.GetChild (4);

		for (int i = 0; i < missionItemHolder.childCount; i++)
			missionItemQuickSave.Add (new MissionItemQuickSave ());
		for (int i = 0; i < doorHolder.childCount; i++)
			doorQuickSave.Add (new DoorQuickSave ());
		for (int i = 0; i < buttonHolder.childCount; i++)
			buttonQuickSave.Add (new ButtonQuickSave ());
		for (int i = 0; i < hidingPlaceHolder.childCount; i++)
			hidingPlaceQuickSave.Add (new HidingPlaceQuickSave ());
		for (int i = 0; i < triggerAreaHolder.childCount; i++)
			triggerAreaQuickSave.Add (new TriggerAreaQuickSave ());

		variablesSet = true;
	}

	public void QuickSave () {
		if (!variablesSet)
			SetVariables ();

		for (int i = 0; i < missionItemQuickSave.Count; i++)
			missionItemQuickSave [i].active = missionItemHolder.GetChild (i).gameObject.activeInHierarchy;
		for (int i = 0; i < doorQuickSave.Count; i++)
			doorQuickSave [i].locked = doorHolder.GetChild (i).GetChild (0).GetComponent<Door> ().locked;
		for (int i = 0; i < buttonQuickSave.Count; i++)
			buttonQuickSave [i].used = buttonHolder.GetChild (i).GetComponent<ButtonItem> ().used;
		for (int i = 0; i < hidingPlaceQuickSave.Count; i++)
			hidingPlaceQuickSave [i].isHiding = hidingPlaceHolder.GetChild (i).GetComponent<HidingPlace> ().isHiding;
		for (int i = 0; i < triggerAreaQuickSave.Count; i++)
			triggerAreaQuickSave [i].triggered = triggerAreaHolder.GetChild (i).GetComponent<TriggerArea> ().triggered;
	}

	public void QuickLoad () {
		if (!variablesSet)
			SetVariables ();

		for (int i = 0; i < missionItemQuickSave.Count; i++)
			missionItemHolder.GetChild (i).gameObject.SetActive (missionItemQuickSave [i].active);
		for (int i = 0; i < doorQuickSave.Count; i++) {
			doorHolder.GetChild (i).GetChild (0).GetComponent<Door> ().locked = doorQuickSave [i].locked;
			doorHolder.GetChild (i).GetChild (1).GetComponent<Door> ().locked = doorQuickSave [i].locked;
		}
		for (int i = 0; i < buttonQuickSave.Count; i++)
			buttonHolder.GetChild (i).GetComponent<ButtonItem> ().used = buttonQuickSave [i].used;
		for (int i = 0; i < hidingPlaceQuickSave.Count; i++)
			hidingPlaceHolder.GetChild (i).GetComponent<HidingPlace> ().isHiding = hidingPlaceQuickSave [i].isHiding;
		for (int i = 0; i < triggerAreaQuickSave.Count; i++)
			triggerAreaHolder.GetChild (i).GetComponent<TriggerArea> ().triggered = triggerAreaQuickSave [i].triggered;
	}
}
