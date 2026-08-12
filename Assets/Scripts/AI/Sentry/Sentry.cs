using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SentryQuickSave {

	public Vector2 position;
	public Transform nextStop;
	public float health;
	public Sentry.State state;
	public float suspicionPercentage;
}

public class Sentry : LivingEntity {

	public enum State {
		unaware,
		chasing,
		disabled
	};

	public State state = State.unaware;
	public GameObject sentryStopsHolder;
	public Transform nextStop;
	public float flySpeed;
	[Range(0,1)] public float suspicionPercentage = 0f;
	public bool seesPlayer = false;
	public LayerMask whatAreEnemies;
	public LayerMask whatIsGround;
	public Transform suspicionSpriteMask;
	public SentryQuickSave sentryQuickSave;
	public Sprite unawareSprite;
	public Sprite chasingSprite;
	public Sprite disabledSprite;

	GameObject player;
	Rigidbody2D rb;
	SpriteRenderer spriteRenderer;
	Transform floor;

    AudioManager audioManager;

	bool idling;
	bool playerDead;
	bool inStasis;

	float idleTime;
	float idleFinishTime;
	//Vector2 newDirection;

	bool playerIsHiding = false;
	float playerVisibilityFactor = 0f;
	float seesPlayerRate = 20f;
	float losesPlayerRate = 1f;

	float maxVelocity;

	protected override void Start () {
		base.Start ();
		player = GameObject.FindGameObjectWithTag ("Player");
		rb = gameObject.GetComponent<Rigidbody2D> ();
		spriteRenderer = GetComponent<SpriteRenderer> ();
		floor = transform.parent.parent;

        audioManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<AudioManager>();

		OnDeath += OnSentryDeath;
		player.GetComponent<PlayerInfo> ().OnDeath += OnPlayerDeath;
	}

	void FixedUpdate () {
		if (!playerDead && !inStasis) {
			if (state == State.disabled) {
				if (!dead) {
					rb.velocity = Vector2.zero;
					suspicionPercentage = 0f;
					spriteRenderer.sprite = disabledSprite;
					GetComponent<BoxCollider2D> ().isTrigger = true;
					rb.gravityScale = 1f;
					for (int i = 0; i < transform.childCount; i++)
						transform.GetChild (i).gameObject.SetActive (false);
					sentryStopsHolder.SetActive (false);
				}
				if (rb.gravityScale > 0f) {
					Debug.DrawRay (transform.position, -Vector2.up * (transform.localScale.y / 2f + .3f), Color.green);
					if (Physics2D.Raycast (transform.position, -Vector2.up, transform.localScale.y / 2f + .3f, whatIsGround)) {
						rb.gravityScale = 0f;
						rb.velocity = Vector2.zero;
					}
				}
				dead = true;
			} else {
				if (state == State.unaware) {
					if (!idling) {
						spriteRenderer.sprite = unawareSprite;
						transform.position = new Vector2 (Vector2.MoveTowards (transform.position, nextStop.position, flySpeed * Time.fixedDeltaTime).x, transform.position.y);
					} else if (Time.time > idleFinishTime) {
						idling = false;
					}
				}

				PlayerAwareness ();

				if (state == State.chasing) {
					spriteRenderer.sprite = chasingSprite;
					PlayerSeen ();
				}
			}

			if (Mathf.Abs (rb.velocity.magnitude) > maxVelocity)
				maxVelocity = Mathf.Abs (rb.velocity.magnitude);
		}
	}

	//Is called by the Sentry Vision when the Sentry sees the player.
	public void SeesPlayer (float _playerVisibilityFactor) {
		if (_playerVisibilityFactor > 0 && !playerIsHiding) {
			playerVisibilityFactor = _playerVisibilityFactor;
			seesPlayer = true;
		}
	}

	//Is called by the Sentry Vision when the Sentry no longer sees the player.
	public void LostPlayer () {
		seesPlayer = false;
	}

	//Is called when the player has entered a natural hiding place to force the Sentry to lose them.
	public void PlayerHiding (bool _playerIsHiding) {
		playerIsHiding = _playerIsHiding;
		LostPlayer ();
	}

	//Is called if the Sentry is in a Stasis Bubble.
	public void InStasis (bool _inStasis) {
		inStasis = _inStasis;
	}

	//Takes information from the Sentry Stop once one has been reached to determine the Sentry's behavior.
	public void StopReached (float _idleTime, Transform _nextStop){
		if (!playerDead && !inStasis) {
			if (state != State.chasing) {
				idleTime = _idleTime;
				nextStop = _nextStop;
				if (_idleTime > 0f)
					Idle ();
			}
		}
	}

	//Sets the Sentry and the Sentry Stops inactive upon its death.
	public void OnSentryDeath () {
		state = State.disabled;
	}

	//Calls when the player dies.
	void OnPlayerDeath () {
		playerDead = true;
		state = State.unaware;
		suspicionPercentage = 0f;
	}

	//Stops the Sentry in place for a set amount of time while patrolling.
	void Idle () {
		idleFinishTime = Time.time + idleTime;
		idling = true;
	}

	//Takes information from the Sentry Vision and uses it to determine the Sentry's behavior.
	void PlayerAwareness () {
		if (!playerDead && !inStasis) {
			if (seesPlayer && suspicionPercentage < 1)
				suspicionPercentage += playerVisibilityFactor * seesPlayerRate / 100f;
			if (!seesPlayer && suspicionPercentage > 0) {
				suspicionPercentage -= losesPlayerRate / 100f;
			}
			suspicionPercentage = Mathf.Clamp (suspicionPercentage, 0, 1);

			suspicionSpriteMask.localPosition = new Vector3 (0f, suspicionPercentage * 0.625f, 0f);

			if (state == State.chasing && suspicionPercentage < 1f) {
                audioManager.StopSound("Alarm");
				state = State.unaware;
				if (transform.GetComponent<BoxCollider2D> ().IsTouching (nextStop.GetComponent<BoxCollider2D> ()))
					nextStop.GetComponent<SentryStop> ().ForceUpdate ();
			}

			if (suspicionPercentage == 1f) {
				if (state != State.chasing) {
                    audioManager.PlaySound("Alarm");
					for (int i = 0; i < floor.childCount; i++)
						if (floor.GetChild (i).GetChild (0).name.Contains ("Guard"))
							floor.GetChild (i).GetChild (0).GetComponent<Guard> ().PlayerSeenByOther ();
				}
				state = State.chasing;
			}
		}
	}

	//Chases the player around.
	void PlayerSeen () {
		idling = false;
		transform.position = new Vector2 (Vector2.MoveTowards (transform.position, player.transform.position, flySpeed * Time.fixedDeltaTime * 10).x, transform.position.y);
	}

	void OnCollisionEnter2D (Collision2D collision) {
		if (collision.collider.gameObject.layer == 13) {
			if (maxVelocity >= 30f)
				GetComponent<LivingEntity> ().TakeHit (1000f);
		}
	}

	public void QuickSave () {
		sentryQuickSave.position=transform.position;
		sentryQuickSave.nextStop = nextStop;
		sentryQuickSave.health = health;
		sentryQuickSave.state = state;
		sentryQuickSave.suspicionPercentage = suspicionPercentage;
	}

	public void QuickLoad () {
		transform.position = sentryQuickSave.position;
		nextStop = sentryQuickSave.nextStop;
		health = sentryQuickSave.health;
		state = sentryQuickSave.state;
		playerDead = false;
		suspicionPercentage = sentryQuickSave.suspicionPercentage;
		if (dead && state != State.disabled) {
			GetComponent<SpriteRenderer> ().color = Color.white;
			GetComponent<BoxCollider2D> ().isTrigger = false;
			for (int i = 0; i < transform.childCount; i++)
				transform.GetChild (i).gameObject.SetActive (true);
			sentryStopsHolder.SetActive (true);
			dead = false;
		}
	}
}
