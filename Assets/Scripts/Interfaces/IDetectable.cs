namespace Interfaces
{
	// AI - Interface for entities that can be detected by other entities (e.g., player visibility to guards).
	public interface IDetectable
	{
		float VisibilityFactor { get; }
		bool IsHidden { get; }
	}
}
