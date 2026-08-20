using UnityEngine;

namespace GameManager
{
	public class ObjectiveSystem : MonoBehaviour
	{
		public Objective[] Objectives;

		public void SetObjectiveStatus(int objectiveNumber, Objective.Status newStatus)
		{
			int objectiveIndex = FindObjectiveIndex(objectiveNumber);
			Objectives[objectiveIndex].SetStatus(newStatus);

			if (newStatus == Objective.Status.completed)
			{
				for (int i = 0; i < Objectives[objectiveIndex].NextObjectives.Length; i++)
				{
					Objectives[FindObjectiveIndex(Objectives[objectiveIndex].NextObjectives[i])].IsActive = true;
				}
				Objectives[objectiveIndex].IsActive = false;
				//Debug.Log ("Objective " + objectives [objectiveIndex].objectiveText + " completed!");
			}
		}

		int FindObjectiveIndex(int objectiveNumber)
		{
			for (int objectiveIndex = 0; objectiveIndex < Objectives.Length; ++objectiveIndex)
			{
				if (objectiveNumber != Objectives[objectiveIndex].ObjectiveNumber)
				{
					continue;
				}
				
				return objectiveIndex;
			}
			
			return -1;
		}
	}
}
