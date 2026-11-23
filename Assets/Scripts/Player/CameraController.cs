using UnityEngine;

namespace Player
{
	public class CameraController : MonoBehaviour
	{
		#region Fields

		[Tooltip("A reference to the target that the camera will follow")]
		public Transform Target;

		[Tooltip("Camera smoothing variable (lower is slower)")]
		public float SmoothSpeed = 10f;

		[Tooltip("Camera offset from the target (X is right and left, and Y is up)")]
		public Vector2 Offset;

		[Tooltip("Camera horizontal boundaries in the world (X is left boundary, and Y right boundary)")]
		public Vector2 HorizontalBoundaries;

		private Transform _startingTarget;
		private Vector2 _startingOffset;
		private float _startingSmoothSpeed;

		private float _mOffsetZ;
		private Vector3 _mLookAheadPos;

		#endregion

		#region Initialization Functions

		private void Start()
		{
			_startingTarget = Target;
			_startingOffset = Offset;
			_startingSmoothSpeed = SmoothSpeed;

			// AI - Calculate the Z offset from the target.
			_mOffsetZ = (transform.position - Target.position).z;
		}

		#endregion

		#region Update Functions

		private void FixedUpdate()
		{
			FollowTarget();
		}

		// AI - Smoothly follow the target with a look-ahead based on the target's facing direction.
		private void FollowTarget()
		{
			// AI - Get the scale of the player. Positive = facing right, negative = facing left.
			float targetScale = Target.localScale.x;
			float targetScalePositive = Mathf.Abs(targetScale);

			// AI - Calculate how far ahead of the player the camera should look.
			_mLookAheadPos = Vector3.right * (Offset.x * (targetScale / targetScalePositive));
			
			// AI - Find the exact position the camera should focus on.
			Vector3 aheadTargetPos = Target.position + _mLookAheadPos + Vector3.forward * _mOffsetZ;
			
			// AI - Smoothly interpolate to that position.
			Vector3 newPos = Vector3.Lerp(transform.position, aheadTargetPos, SmoothSpeed * Time.deltaTime);
			
			// AI - Apply position with clamped horizontal boundaries and Y offset.
			transform.position = new Vector3(
				Mathf.Clamp(newPos.x, HorizontalBoundaries.x, HorizontalBoundaries.y),
				Target.position.y + Offset.y,
				newPos.z);
		}

		#endregion

		#region Target Management Functions

		// AI - Switch the camera to follow a new target with custom offset and smoothing.
		public void NewTarget(Transform newTarget, Vector2 newOffset, float newSmoothSpeed)
		{
			Target = newTarget;
			Offset = newOffset;
			SmoothSpeed = newSmoothSpeed;
		}

		// AI - Restore the camera to its original target and settings.
		public void ResetTarget()
		{
			Target = _startingTarget;
			Offset = _startingOffset;
			SmoothSpeed = _startingSmoothSpeed;
		}

		#endregion
	}
}
