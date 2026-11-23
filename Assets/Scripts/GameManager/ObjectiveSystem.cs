namespace GameManager
{
	// AI - Note: This is a small system class and doesn't require extensive regions.
	public class ObjectiveSystem : Singleton<ObjectiveSystem>
	{
		public Objective[] Objectives;

		// AI - Update an objective's status and activate any subsequent objectives if completed.
		public void SetObjectiveStatus(int objectiveNumber, Objective.Status newStatus)
		{
			int objectiveIndex = FindObjectiveIndex(objectiveNumber);

			if (objectiveIndex == -1)
			{
				return;
			}
			
			Objectives[objectiveIndex].SetStatus(newStatus);

			if (newStatus == Objective.Status.Completed)
			{
				ActivateNextObjectives(objectiveIndex);
				Objectives[objectiveIndex].IsActive = false;
			}
		}

		// AI - Activate all objectives that follow the completed objective.
		private void ActivateNextObjectives(int completedObjectiveIndex)
		{
			int[] nextObjectives = Objectives[completedObjectiveIndex].NextObjectives;
			
			if (nextObjectives == null)
			{
				return;
			}

			for (int i = 0; i < nextObjectives.Length; i++)
			{
				int nextObjectiveIndex = FindObjectiveIndex(nextObjectives[i]);
				if (nextObjectiveIndex != -1)
				{
					Objectives[nextObjectiveIndex].IsActive = true;
				}
			}
		}

		// AI - Find the array index of an objective by its objective number.
		private int FindObjectiveIndex(int objectiveNumber)
		{
			for (int objectiveIndex = 0; objectiveIndex < Objectives.Length; ++objectiveIndex)
			{
				if (objectiveNumber == Objectives[objectiveIndex].ObjectiveNumber)
				{
					return objectiveIndex;
				}
			}
			
			return -1;
		}
	}
}
