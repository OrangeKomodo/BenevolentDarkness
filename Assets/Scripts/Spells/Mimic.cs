using AI.Guard;
using GameManager;
using UnityEngine;

namespace Spells
{
	public class Mimic : Spell
	{
		public SpriteRenderer MimicMarker;
		
		public int ManaTickCost;
		public float ManaDeductTick;

		[Range(0, 100)]
		public float Sensitivity;
		public float MaxTransferDistance;
		public float TransferTime;
		public bool Disguised;
		public Color CanMimic = Color.white;
		public Color CanNotMimic = Color.gray;
		
		private Camera _camera;

		private SpriteRenderer _spriteRenderer;
		
		private float _sensitivity => Sensitivity / 50f;
		
		private float _markerUpdateTime = 0.02f;
		private float _nextMarkerUpdate;

		private bool _canTransfer = false;
		private bool _isTransferring = false;
		private float _transferStartTime;
		private float _percentTransferred;

		private readonly float _markerSizeMin = 0.5f;
		private readonly float _markerSizeMax = 1.0f;

		private void Start()
		{
			ManaDeductTick = (SpellCaster.SpellLevel - 1f) * 0.5f;
			
			_spriteRenderer = MimicMarker.GetComponent<SpriteRenderer>();

			MimicMarker.gameObject.SetActive(true);
			PlayerController.CanAttack = false;
			
			_camera = Camera.main;

			if (!InputManager.UsingGamepad)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}

		private void Update()
		{
			if (Disguised)
			{
				return;
			}

			if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Exit"))
			{
				SpellCaster.EndSpell(SpellCasting.SpellNames.Mimic);
				return;
			}

			if (Input.GetAxis("Use Item") < 0.95f)
			{
				SpellCaster.EndSpell(SpellCasting.SpellNames.Mimic);
				return;
			}

			// Ensure the Marker updates only so often, no matter the framerate.
			if (Time.time < _nextMarkerUpdate)
			{
				return;
			}
			
			_nextMarkerUpdate = Time.time + _markerUpdateTime;
			
			UpdateMarkerPosition();

			_percentTransferred = Mathf.Clamp(_percentTransferred, 0, 1);

			if (_percentTransferred < 1f)
			{
				return;
			}
			
			PerformMimic();
		}

		// Move the Marker according to the Player's input
		private void UpdateMarkerPosition()
		{
			// Get cursor position for the mouse or the controller
			Vector2 cursorPosition = GetUpdatedCursorPosition();

			RaycastHit2D cursorRayHit = Physics2D.Raycast(cursorPosition, Vector2.zero, 100f);

			// If the cursor is not in a valid location, abort
			if (!cursorRayHit)
			{
				return;
			}

			RaycastHit2D playerRayHit;
			LayerMask layerMask = LayerMask.GetMask("Enemies", "Platforms", "Walls");
				
			Vector2 targetPosition = cursorRayHit.point;
			Vector2 playerPosition = PlayerController.transform.position;
			Vector2 targetDirection = targetPosition - playerPosition;
			Vector2 normalizedTargetDirection = targetDirection.normalized;
			float targetDistance = Vector2.Distance(playerPosition, targetPosition);
			float clampedDistance = Mathf.Clamp(targetDistance, 0f, MaxTransferDistance);

			Debug.DrawRay(playerPosition, normalizedTargetDirection * clampedDistance, Color.blue);
			playerRayHit = Physics2D.Raycast(playerPosition, targetDirection, clampedDistance, layerMask);

			if (playerRayHit.collider?.gameObject.layer == 9 && cursorRayHit.collider?.gameObject.layer == 9)
			{
				Guard guard = playerRayHit.collider.GetComponentInParent<Guard>();
				TargetGuard(guard);
				
				return;
			}

			ResetTransfer();
			MimicMarker.transform.position = playerPosition + normalizedTargetDirection * clampedDistance;
		}

		// Get the cursor position depending on whether we're using a mouse or a controller
		private Vector3 GetUpdatedCursorPosition()
		{
			if (InputManager.UsingGamepad)
			{
				float xPositionDelta = Input.GetAxis("Mouse X") * _sensitivity;
				float yPositionDelta = Input.GetAxis("Mouse Y") * _sensitivity;
				Vector3 cursorPositionDelta = new Vector3(xPositionDelta, yPositionDelta, 0f);
				
				return transform.position + cursorPositionDelta;
			}
			
			return _camera.ScreenToWorldPoint(Input.mousePosition);
		}

		private void ResetTransfer()
		{
			if (_canTransfer)
			{
				_spriteRenderer.color = CanNotMimic;
				_canTransfer = false;
			}

			if (_isTransferring)
			{
				_isTransferring = false;
			}

			if (_percentTransferred > 0)
			{
				_percentTransferred = 0;
			}
		}

		private void TargetGuard(Guard guard)
		{
			MimicMarker.transform.position = guard.Head.position;

			if (!guard.CanMimic)
			{
				_spriteRenderer.color = CanNotMimic;
				return;
			}

			if (!_canTransfer)
			{
				_spriteRenderer.color = CanMimic;
				_canTransfer = true;
			}

			if (Input.GetAxis("Attack") >= 0.95f)
			{
				if (!_isTransferring)
				{
					_transferStartTime = Time.time;
					_isTransferring = true;
				}

				_percentTransferred = (Time.time - _transferStartTime) / TransferTime;
				UpdateMarkerPercentage();

				return;
			}
			
			if (Input.GetAxis("Attack") < 0.95f && _isTransferring)
			{
				_transferStartTime = 0f;
				_isTransferring = false;

				_percentTransferred = 0;
				UpdateMarkerPercentage();
			}
		}

		private void UpdateMarkerPercentage()
		{
			float markerSizeRange = _markerSizeMax - _markerSizeMin;
			float markerScale = markerSizeRange * _percentTransferred + _markerSizeMin;
			MimicMarker.transform.localScale = Vector2.one * markerScale;
		}

		private void PerformMimic()
		{
			//Debug.Log ("Transfer Complete");
			MimicMarker.gameObject.SetActive(false);
			Disguised = true;
			SpellCaster.Disguised();
			PlayerController.gameObject.layer = 9;

			//DISGUISE HERE
			PlayerController.PlaySound("Mimic");
			PlayerController.InDisguise(true);
			PlayerController.CanAttack = true;

			if (!InputManager.UsingGamepad)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
		}

		public void EndMimic()
		{
			//UNDISGUISE
			if (Disguised)
			{
				PlayerController.gameObject.layer = 8;
				PlayerController.PlaySound("Mimic");
				PlayerController.InDisguise(false);
			}

			PlayerController.CanAttack = true;
			if (!InputManager.UsingGamepad)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}

			Destroy(gameObject);
		}
	}
}
