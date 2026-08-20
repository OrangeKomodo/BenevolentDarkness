using System.Collections;
using System.Collections.Generic;
using AI.Guard;
using AI.Sentry;
using Player;
using Spells;
using UnityEngine;

public class Stasis : Spell
{

	public float maxDistance;
	public float duration;
	public bool stasisOccured;

	float stasisStartedTime;

	CameraController cameraController;
	GameObject player;
	PlayerController playerController;
	LayerMask whatAreEnemies;

	List<Guard> frozenGuards = new List<Guard>();
	List<Sentry> frozenSentries = new List<Sentry>();

	bool usingController;

	void Start()
	{
		Time.timeScale = 0.5f;
		cameraController = FindObjectOfType<CameraController>();
		cameraController.NewTarget(transform, Vector2.zero, 0.5f);
		player = GameObject.FindGameObjectWithTag("Player");
		playerController = player.GetComponent<PlayerController>();
		whatAreEnemies = LayerMask.GetMask("Enemies");
		usingController = Input.GetJoystickNames().Length > 0;

		if (!usingController)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}

	void Update()
	{
		if (Input.GetAxis("Use Item") == 1f && !stasisOccured)
		{
			Vector2 mouseRay;
			if (usingController)
			{
				transform.Translate(new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0f));
				mouseRay = transform.position;
			}
			else
			{
				mouseRay = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			}

			RaycastHit2D mouseRayHit = Physics2D.Raycast(mouseRay, Vector2.zero, 100f);
			RaycastHit2D playerRayHit;
			LayerMask layerMask = LayerMask.GetMask("Platforms", "Walls");

			if (mouseRayHit)
			{
				Vector3 targetPosition = mouseRayHit.point;
				Vector3 playerPosition = player.transform.position;

				Debug.DrawRay(playerPosition,
					(targetPosition - playerPosition).normalized
					* Mathf.Clamp(Vector2.Distance(playerPosition, targetPosition), 0f, maxDistance), Color.white);
				playerRayHit = Physics2D.Raycast(playerPosition, targetPosition - playerPosition,
					Mathf.Clamp(Vector2.Distance(playerPosition, targetPosition), 0f, maxDistance), layerMask);

				if (playerRayHit.collider == null)
				{
					if (Vector2.Distance(targetPosition, playerPosition) <= maxDistance)
					{
						//Debug.Log ("Mouse Point");
						transform.position = mouseRayHit.point;
					}
					else
					{
						//Debug.Log ("Boundry Point");
						transform.position =
							playerPosition + (targetPosition - playerPosition).normalized * maxDistance;
					}
				}
				else
				{
					//Debug.Log ("Player Point");
					transform.position = playerRayHit.point;
				}

				//Debug.Log (playerRayHit.distance);
			}
		}

		if (Input.GetAxis("Use Item") == 0f && !stasisOccured)
		{
			playerController.PlaySound("Stasis");
			Time.timeScale = 1f;
			cameraController.ResetTarget();
			Collider2D[] enemiesToFreeze =
				Physics2D.OverlapCircleAll(transform.position, transform.localScale.x, whatAreEnemies);
			for (int i = 0; i < enemiesToFreeze.Length; i++)
			{
				if (enemiesToFreeze[i].name.Contains("Guard"))
				{
					Guard guard = enemiesToFreeze[i].GetComponent<Guard>();
					guard.InStasis(true);
					frozenGuards.Add(guard);
				}
				else if (enemiesToFreeze[i].name.Contains("Sentry"))
				{
					Sentry sentry = enemiesToFreeze[i].GetComponent<Sentry>();
					sentry.InStasis(true);
					frozenSentries.Add(sentry);
				}
			}

			stasisStartedTime = Time.time;
			stasisOccured = true;
			player.GetComponent<SpellCasting>().StasisOccured();

			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		if (stasisOccured && Time.time >= stasisStartedTime + duration)
		{
			EndStasis();
		}

		if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Exit"))
		{
			EndStasis();
		}
	}

	void EndStasis()
	{
		if (stasisOccured)
		{
			foreach (Guard guard in frozenGuards)
				guard.InStasis(false);
			foreach (Sentry sentry in frozenSentries)
				sentry.InStasis(false);
			cameraController.ResetTarget();
		}
		else
		{
			Time.timeScale = 1f;
			cameraController.ResetTarget();
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		Destroy(gameObject);
	}

	void OnTriggerEnter2D(Collider2D collider)
	{
		if (stasisOccured && collider.gameObject.layer == 9)
		{
			if (collider.name.Contains("Guard"))
			{
				Guard guard = collider.GetComponent<Guard>();
				guard.InStasis(true);
				frozenGuards.Add(guard);
			}
			else if (collider.name.Contains("Sentry"))
			{
				Sentry sentry = collider.GetComponent<Sentry>();
				sentry.InStasis(true);
				frozenSentries.Add(sentry);
			}
		}
	}
}
