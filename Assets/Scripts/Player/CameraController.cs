using UnityEngine;

namespace Player
{
	public class CameraController : MonoBehaviour
	{

		[Tooltip("A reference to the target that the camera will follow")]
		public Transform target;

		[Tooltip("Camera smoothing variable (lower is slower)")]
		public float smoothSpeed = 10f;

		[Tooltip("Camera offset from the target (X is right and left, and Y is up)")]
		public Vector2 offset;

		[Tooltip("Camera horizontal boundries in the world (X is left boundry, and Y right boundry)")]
		public Vector2 horizontalBoundries;

		Transform startingTarget;
		Vector2 startingOffset;
		float startingSmoothSpeed;

		float m_OffsetZ;
		Vector3 m_LookAheadPos;

		void Start()
		{
			startingTarget = target;
			startingOffset = offset;
			startingSmoothSpeed = smoothSpeed;

			//Finds the Z offset.
			m_OffsetZ = (transform.position - target.position).z;
		}

		void FixedUpdate()
		{
			//Gets the scale of the player. If it's positive, the player is facing right and vice versa.
			float targetScale = target.localScale.x;
			//Gets the absolute value of the target's scale.
			float targetScalePositive = Mathf.Abs(targetScale);

			//Finds how far ahead of the player the camera should be.
			m_LookAheadPos = offset.x * Vector3.right * (targetScale / targetScalePositive);
			//Finds the exact position the camera should be focused on.
			Vector3 aheadTargetPos = target.position + m_LookAheadPos + Vector3.forward * m_OffsetZ;
			//Lerps to that position.
			Vector3 newPos = Vector3.Lerp(transform.position, aheadTargetPos, smoothSpeed * Time.deltaTime);
			//Sets the camera's position with the Y offset.
			transform.position = new Vector3(Mathf.Clamp(newPos.x, horizontalBoundries.x, horizontalBoundries.y),
				target.position.y + offset.y, newPos.z);
		}

		public void NewTarget(Transform newTarget, Vector2 newOffset, float newSmoothSpeed)
		{
			target = newTarget;
			offset = newOffset;
			smoothSpeed = newSmoothSpeed;
		}

		public void ResetTarget()
		{
			target = startingTarget;
			offset = startingOffset;
			smoothSpeed = startingSmoothSpeed;
		}
	}
}
