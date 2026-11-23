using UnityEngine;

namespace Interfaces
{
	// AI - Interface for AI entities that can track and respond to player detection.
	public interface IAwareEntity
	{
		void OnPlayerSpotted(Vector3 location);
		void OnPlayerLost();
		void OnAlertedByAlly(Vector3 lastKnownLocation);
	}
}
