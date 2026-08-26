using UnityEngine;

namespace Player
{
	public class CameraController : MonoBehaviour
	{

		[Tooltip("A reference to the target that the camera will follow")]
		public Transform Target;

		[Tooltip("Camera smoothing variable (lower is slower)")]
		public float SmoothSpeed = 10f;

		[Tooltip("Camera offset from the target (X is right and left, and Y is up)")]
		public Vector2 Offset;

		[Tooltip("Camera horizontal boundaries in the world (X is left boundry, and Y right boundry)")]
		public Vector2 HorizontalBoundaries;

		private Transform _startingTarget;
		private Vector2 _startingOffset;
		private float _startingSmoothSpeed;

		private float _mOffsetZ;
		private Vector3 _mLookAheadPos;

		private void Start()
		{
			_startingTarget = Target;
			_startingOffset = Offset;
			_startingSmoothSpeed = SmoothSpeed;

			//Finds the Z offset.
			_mOffsetZ = (transform.position - Target.position).z;
		}

		private void FixedUpdate()
		{
			//Gets the scale of the player. If it's positive, the player is facing right and vice versa.
			float targetScale = Target.localScale.x;
			//Gets the absolute value of the target's scale.
			float targetScalePositive = Mathf.Abs(targetScale);

			//Finds how far ahead of the player the camera should be.
			_mLookAheadPos = Vector3.right * (Offset.x * (targetScale / targetScalePositive));
			//Finds the exact position the camera should be focused on.
			Vector3 aheadTargetPos = Target.position + _mLookAheadPos + Vector3.forward * _mOffsetZ;
			//Lerps to that position.
			Vector3 newPos = Vector3.Lerp(transform.position, aheadTargetPos, SmoothSpeed * Time.deltaTime);
			//Sets the camera's position with the Y offset.
			transform.position = new Vector3(Mathf.Clamp(newPos.x, HorizontalBoundaries.x, HorizontalBoundaries.y),
				Target.position.y + Offset.y, newPos.z);
		}

		public void NewTarget(Transform newTarget, Vector2 newOffset, float newSmoothSpeed)
		{
			Target = newTarget;
			Offset = newOffset;
			SmoothSpeed = newSmoothSpeed;
		}

		public void ResetTarget()
		{
			Target = _startingTarget;
			Offset = _startingOffset;
			SmoothSpeed = _startingSmoothSpeed;
		}
	}
}
