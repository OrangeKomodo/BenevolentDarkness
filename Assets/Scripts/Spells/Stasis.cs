using System.Collections.Generic;
using AI.Guard;
using AI.Sentry;
using Player;
using UnityEngine;

namespace Spells
{
	public class Stasis : Spell
	{

		public float MaxDistance;
		public float Duration;
		public bool StasisOccured;

		private float _stasisStartedTime;

		private CameraController _cameraController;
		private LayerMask _whatAreEnemies;

		private List<Guard> _frozenGuards = new List<Guard>();
		private List<Sentry> _frozenSentries = new List<Sentry>();

		private bool _usingController;

		private void Start()
		{
			Time.timeScale = 0.5f;
			_cameraController = PlayerController.CameraController;
			_cameraController.NewTarget(transform, Vector2.zero, 0.5f);
			_whatAreEnemies = LayerMask.GetMask("Enemies");
			_usingController = Input.GetJoystickNames().Length > 0;

			if (!_usingController)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}

		private void Update()
		{
			if (Input.GetAxis("Use Item") == 1f && !StasisOccured)
			{
				Vector2 mouseRay;
				if (_usingController)
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
					Vector3 playerPosition = PlayerController.transform.position;

					Debug.DrawRay(playerPosition,
						(targetPosition - playerPosition).normalized
						* Mathf.Clamp(Vector2.Distance(playerPosition, targetPosition), 0f, MaxDistance), Color.white);
					playerRayHit = Physics2D.Raycast(playerPosition, targetPosition - playerPosition,
						Mathf.Clamp(Vector2.Distance(playerPosition, targetPosition), 0f, MaxDistance), layerMask);

					if (playerRayHit.collider == null)
					{
						if (Vector2.Distance(targetPosition, playerPosition) <= MaxDistance)
						{
							//Debug.Log ("Mouse Point");
							transform.position = mouseRayHit.point;
						}
						else
						{
							//Debug.Log ("Boundry Point");
							transform.position =
								playerPosition + (targetPosition - playerPosition).normalized * MaxDistance;
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

			if (Input.GetAxis("Use Item") == 0f && !StasisOccured)
			{
				PlayerController.PlaySound("Stasis");
				Time.timeScale = 1f;
				_cameraController.ResetTarget();
				Collider2D[] enemiesToFreeze =
					Physics2D.OverlapCircleAll(transform.position, transform.localScale.x, _whatAreEnemies);
				for (int i = 0; i < enemiesToFreeze.Length; i++)
				{
					if (enemiesToFreeze[i].name.Contains("Guard"))
					{
						Guard guard = enemiesToFreeze[i].GetComponent<Guard>();
						guard.InStasis(true);
						_frozenGuards.Add(guard);
					}
					else if (enemiesToFreeze[i].name.Contains("Sentry"))
					{
						Sentry sentry = enemiesToFreeze[i].GetComponent<Sentry>();
						sentry.InStasis(true);
						_frozenSentries.Add(sentry);
					}
				}

				_stasisStartedTime = Time.time;
				StasisOccured = true;
				SpellCaster.StasisOccured();

				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}

			if (StasisOccured && Time.time >= _stasisStartedTime + Duration)
			{
				EndStasis();
			}

			if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Exit"))
			{
				EndStasis();
			}
		}

		private void EndStasis()
		{
			if (StasisOccured)
			{
				foreach (Guard guard in _frozenGuards)
					guard.InStasis(false);
				foreach (Sentry sentry in _frozenSentries)
					sentry.InStasis(false);
				_cameraController.ResetTarget();
			}
			else
			{
				Time.timeScale = 1f;
				_cameraController.ResetTarget();
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}

			Destroy(gameObject);
		}

		private void OnTriggerEnter2D(Collider2D otherCollider)
		{
			if (StasisOccured && otherCollider.gameObject.layer == 9)
			{
				if (otherCollider.name.Contains("Guard"))
				{
					Guard guard = otherCollider.GetComponent<Guard>();
					guard.InStasis(true);
					_frozenGuards.Add(guard);
				}
				else if (otherCollider.name.Contains("Sentry"))
				{
					Sentry sentry = otherCollider.GetComponent<Sentry>();
					sentry.InStasis(true);
					_frozenSentries.Add(sentry);
				}
			}
		}
	}
}
