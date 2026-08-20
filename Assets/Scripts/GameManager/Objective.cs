namespace GameManager
{
    [System.Serializable]
    public class Objective {

        public enum Status {
            mandatory,
            optional,
            completed,
            impossible
        }

        public int ObjectiveNumber;
        public string ObjectiveText;
        public Status ObjectiveStatus;
        public bool IsActive;
        public int[] NextObjectives;

        public void SetStatus (Status newStatus) {
            ObjectiveStatus = newStatus;
        }
    }
}
