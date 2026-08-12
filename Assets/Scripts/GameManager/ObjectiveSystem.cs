using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Objective {

	public enum Status {
		mandatory,
		optional,
		completed,
		impossible
	}

	public int objectiveNumber;
	public string objectiveText;
	public Status objectiveStatus;
	public bool isActive;
	public int[] nextObjectives;

	public void SetStatus (Status newStatus) {
		objectiveStatus = newStatus;
	}
}

public class ObjectiveSystem : MonoBehaviour {

	public Objective[] objectives;

	public void SetObjectiveStatus (int objectiveNumber, Objective.Status newStatus){
		int objectiveIndex = FindObjectiveIndex (objectiveNumber);
		objectives [objectiveIndex].SetStatus (newStatus);

		if (newStatus == Objective.Status.completed) {
			for (int i = 0; i < objectives [objectiveIndex].nextObjectives.Length; i++)
				objectives [FindObjectiveIndex (objectives [objectiveIndex].nextObjectives [i])].isActive = true;
			objectives [objectiveIndex].isActive = false;
			//Debug.Log ("Objective " + objectives [objectiveIndex].objectiveText + " completed!");
		}
	}

	int FindObjectiveIndex (int objectiveNumber) {
		int w = 0;
		while (objectiveNumber != objectives [w].objectiveNumber && w < objectives.Length)
			w++;
		return w < objectives.Length ? w : -1;
	}
}
