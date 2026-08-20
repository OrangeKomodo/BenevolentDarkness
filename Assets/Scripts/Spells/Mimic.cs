using System.Collections;
using System.Collections.Generic;
using AI.Guard;
using Player;
using Spells;
using UnityEngine;

public class Mimic : Spell
{

	public int manaTickCost;
	public float manaDeductTick;

	public float maxTransferDistance;

	public float transferTime;

	//public float disguiseTime;
	public bool disguised;

	public Color canMimic = Color.white;
	public Color canNotMimic = Color.gray;
	//public Color transfering = Color.blue;

	GameObject mimicMarker;
	GameObject player;
	PlayerInfo playerInfo;
	SpellCasting spellCaster;
	SpriteRenderer spriteRenderer;

	bool canTransfer = false;
	bool isTransfering = false;
	float transferStartTime;
	float transferEndTime;
	float percentTransfered;
	float startPercentage;

	float markerSizeMin = 0.5f;
	float markerSizeMax = 1.0f;

	bool usingController;

	void Start()
	{
		manaDeductTick = (FindObjectOfType<SpellCasting>().spellLevel - 1f) * 0.5f;
		mimicMarker = gameObject.transform.GetChild(0).gameObject;
		player = GameObject.FindGameObjectWithTag("Player");
		playerInfo = player.GetComponent<PlayerInfo>();
		spellCaster = FindObjectOfType<SpellCasting>();
		spriteRenderer = mimicMarker.GetComponent<SpriteRenderer>();
		usingController = Input.GetJoystickNames().Length > 0;

		playerInfo.canAttack = false;

		if (!usingController)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}

	void FixedUpdate()
	{
		if (!disguised)
		{
			mimicMarker.SetActive(true);

			Vector2 mouseRay;
			if (usingController)
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
				Vector3 playerPosition = player.transform.position;

				Debug.DrawRay(playerPosition,
					(targetPosition - playerPosition).normalized
					* Mathf.Clamp(Vector2.Distance(playerPosition, targetPosition), 0f, maxTransferDistance),
					Color.blue);
				playerRayHit = Physics2D.Raycast(playerPosition, targetPosition - playerPosition,
					Mathf.Clamp(Vector2.Distance(playerPosition, targetPosition), 0f, maxTransferDistance), layerMask);

				if (playerRayHit.collider == null && !(mouseRayHit.collider.gameObject.layer == 9
				                                       && playerRayHit.collider.gameObject.layer == 9))
				{
					if (canTransfer)
						canTransfer = false;
					if (isTransfering)
					{
						transferEndTime = Time.time;
						startPercentage = percentTransfered;
						isTransfering = false;
						//Debug.Log (transferEndTime + " " + percentTransfered);
					}

					mimicMarker.transform.position = mouseRayHit.point;
					if (percentTransfered > 0)
						percentTransfered =
							1f - ((Time.time - transferEndTime) / transferTime + (1f - startPercentage));

				}
				else
				{
					if ("Guard Actual Backside".Contains(playerRayHit.collider.name))
					{
						Transform guard = playerRayHit.collider.name.Equals("Backside")
							? playerRayHit.collider.transform.parent
							: playerRayHit.transform;

						//Debug.Log (playerRayHit.collider.name);
						mimicMarker.transform.position = guard.GetChild(4).position;

						if (!canTransfer && guard.GetComponent<Guard>().canMimic)
							canTransfer = true;

						if (canTransfer && Input.GetAxis("Attack") == 1f)
						{
							if (!isTransfering)
							{
								transferStartTime = Time.time;
								startPercentage = percentTransfered;
								isTransfering = true;
							}

							percentTransfered = (Time.time - transferStartTime) / transferTime + startPercentage;
						}
					}
				}

				//Debug.Log (playerRayHit.distance);
				percentTransfered = Mathf.Clamp(percentTransfered, 0, 1);

				if (percentTransfered == 1f)
				{
					//Debug.Log ("Transfer Complete");
					mimicMarker.SetActive(false);
					disguised = true;
					spellCaster.Disguised();
					player.layer = 9;

					//DISGUISE HERE
					playerInfo.PlaySound("Mimic");
					playerInfo.InDisguise(true);
					playerInfo.canAttack = true;

					if (!usingController)
					{
						Cursor.lockState = CursorLockMode.Locked;
						Cursor.visible = false;
					}
				}

				spriteRenderer.color = canTransfer ? canMimic : canNotMimic;
				if (isTransfering)
					mimicMarker.transform.localScale = new Vector2(1, 1)
					                                   * ((markerSizeMax - markerSizeMin) * percentTransfered
					                                      + markerSizeMin);
			}
		}

		if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Exit")
		                                || (Input.GetAxis("Use Item") == 0f && !disguised))
			spellCaster.EndSpell(SpellCasting.SpellNames.mimic);
	}

	public void EndMimic()
	{
		//UNDISGUISE
		if (disguised)
		{
			player.layer = 8;
			playerInfo.PlaySound("Mimic");
			playerInfo.InDisguise(false);
		}

		playerInfo.canAttack = true;
		if (!usingController)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		Destroy(gameObject);
	}
}
