using AI.Guard;
using UnityEngine;

namespace Spells
{
	public class Mimic : Spell
	{
		public int ManaTickCost;
		public float ManaDeductTick;

		public float MaxTransferDistance;

		public float TransferTime;

		//public float disguiseTime;
		public bool Disguised;

		public Color CanMimic = Color.white;
		public Color CanNotMimic = Color.gray;
		//public Color Transfering = Color.blue;

		private GameObject _mimicMarker;
		private SpriteRenderer _spriteRenderer;

		private bool _canTransfer = false;
		private bool _isTransferring = false;
		private float _transferStartTime;
		private float _transferEndTime;
		private float _percentTransferred;
		private float _startPercentage;

		private readonly float _markerSizeMin = 0.5f;
		private readonly float _markerSizeMax = 1.0f;

		private bool _usingController;

		private void Start()
		{
			ManaDeductTick = (SpellCaster.SpellLevel - 1f) * 0.5f;
			_mimicMarker = gameObject.transform.GetChild(0).gameObject;
			_spriteRenderer = _mimicMarker.GetComponent<SpriteRenderer>();
			_usingController = Input.GetJoystickNames().Length > 0;

			PlayerController.CanAttack = false;

			if (!_usingController)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}

		private void FixedUpdate()
		{
			if (!Disguised)
			{
				_mimicMarker.SetActive(true);

				Vector2 mouseRay;
				if (_usingController)
				{
					transform.Translate(new Vector3(Input.GetAxis("Mouse X") * transform.parent.localScale.x,
						Input.GetAxis("Mouse Y"), 0f));
					mouseRay = transform.position;
				}
				else
					mouseRay = Camera.main.ScreenToWorldPoint(Input.mousePosition);

				RaycastHit2D mouseRayHit = Physics2D.Raycast(mouseRay, Vector2.zero, 100f);
				RaycastHit2D playerRayHit;
				LayerMask layerMask = LayerMask.GetMask("Enemies", "Platforms", "Walls");

				if (mouseRayHit)
				{
					Vector3 targetPosition = mouseRayHit.point;
					Vector3 playerPosition = PlayerController.transform.position;

					Debug.DrawRay(playerPosition,
						(targetPosition - playerPosition).normalized
						* Mathf.Clamp(Vector2.Distance(playerPosition, targetPosition), 0f, MaxTransferDistance),
						Color.blue);
					playerRayHit = Physics2D.Raycast(playerPosition, targetPosition - playerPosition,
						Mathf.Clamp(Vector2.Distance(playerPosition, targetPosition), 0f, MaxTransferDistance), layerMask);

					if (playerRayHit.collider == null && !(mouseRayHit.collider.gameObject.layer == 9
					                                       && playerRayHit.collider.gameObject.layer == 9))
					{
						if (_canTransfer)
							_canTransfer = false;
						if (_isTransferring)
						{
							_transferEndTime = Time.time;
							_startPercentage = _percentTransferred;
							_isTransferring = false;
							//Debug.Log (transferEndTime + " " + percentTransfered);
						}

						_mimicMarker.transform.position = mouseRayHit.point;
						if (_percentTransferred > 0)
							_percentTransferred =
								1f - ((Time.time - _transferEndTime) / TransferTime + (1f - _startPercentage));

					}
					else
					{
						if ("Guard Actual Backside".Contains(playerRayHit.collider.name))
						{
							Transform guard = playerRayHit.collider.name.Equals("Backside")
								? playerRayHit.collider.transform.parent
								: playerRayHit.transform;

							//Debug.Log (playerRayHit.collider.name);
							_mimicMarker.transform.position = guard.GetChild(4).position;

							if (!_canTransfer && guard.GetComponent<Guard>().CanMimic)
								_canTransfer = true;

							if (_canTransfer && Input.GetAxis("Attack") == 1f)
							{
								if (!_isTransferring)
								{
									_transferStartTime = Time.time;
									_startPercentage = _percentTransferred;
									_isTransferring = true;
								}

								_percentTransferred = (Time.time - _transferStartTime) / TransferTime + _startPercentage;
							}
						}
					}

					//Debug.Log (playerRayHit.distance);
					_percentTransferred = Mathf.Clamp(_percentTransferred, 0, 1);

					if (_percentTransferred == 1f)
					{
						//Debug.Log ("Transfer Complete");
						_mimicMarker.SetActive(false);
						Disguised = true;
						SpellCaster.Disguised();
						PlayerController.gameObject.layer = 9;

						//DISGUISE HERE
						PlayerController.PlaySound("Mimic");
						PlayerController.InDisguise(true);
						PlayerController.CanAttack = true;

						if (!_usingController)
						{
							Cursor.lockState = CursorLockMode.Locked;
							Cursor.visible = false;
						}
					}

					_spriteRenderer.color = _canTransfer ? CanMimic : CanNotMimic;
					if (_isTransferring)
						_mimicMarker.transform.localScale = new Vector2(1, 1)
						                                   * ((_markerSizeMax - _markerSizeMin) * _percentTransferred
						                                      + _markerSizeMin);
				}
			}

			if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Exit")
			                                || (Input.GetAxis("Use Item") == 0f && !Disguised))
			{
				SpellCaster.EndSpell(SpellCasting.SpellNames.Mimic);
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
			if (!_usingController)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}

			Destroy(gameObject);
		}
	}
}
