namespace GameManager
{
    // AI - Note: This is a small data class and doesn't require regions.
    [System.Serializable]
    public class Objective
    {
        public enum Status
        {
            Mandatory,
            Optional,
            Completed,
            Impossible
        }

        public int ObjectiveNumber;
        public string ObjectiveText;
        public Status ObjectiveStatus;
        public bool IsActive;
        public int[] NextObjectives;

        // AI - Update the objective's current status.
        public void SetStatus(Status newStatus)
        {
            ObjectiveStatus = newStatus;
        }
    }
}
