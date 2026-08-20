using System.Collections.Generic;
using DamageSystem;
using GameManager;
using Player;
using UnityEngine;

namespace AI.Guard
{
	
	public class Guard : LivingEntity
	{
		public enum State
		{
			unaware,
			suspicious,
			chasing,
			alerted,
			dead,
			unconscious,
			corrupted
		}
		
		struct GuardQuickSave
		{
			public Vector2 position;
			public float direction;
			public Transform nextStop;
			public float health;
			public State state;
			public float suspicionPercentage;
		}

		public State state = State.unaware;
		public GameObject guardStopsHolder;
		public Transform nextStop;
		public float walkSpeed;
		public float runSpeed;
		[Range(0, 1)] public float suspicionPercentage = 0f;
		public bool seesPlayer = false;
		public bool canMimic = true;
		public int damage;
		public Transform attackPos;
		public float attackRange;
		public float startTimeBetweenAttack;
		public LayerMask whatAreEnemies;
		public LayerMask whatAreFriends;
		public LayerMask whatIsGround;
		public Transform suspicionSpriteMask;

		List<Transform> fellowGuards = new List<Transform>();
		Transform closestGuard;

		GameObject player;
		Rigidbody2D rb;
		Animator anim;
		AudioManager audioManager;
		Transform floor;
		GuardQuickSave guardQuickSave;

		float timeBetweenAttack;

		bool idling;
		bool playerInMeleeRange;
		bool playerDead = false;
		bool inStasis = false;
		bool inForceField = false;

		float idleTime;
		Vector2 idleDirection;
		float idleFinishTime;

		bool playerIsHiding = false;
		float playerVisibilityFactor = 0f;
		float seesPlayerRate = 5f;
		float losesPlayerRate = 5f;

		Vector3 lastKnownLocation;
		float giveUpTime = 5f;
		float lastSeenTime;
		bool canSpot = true;

		float alertedTime = 7f;
		float alertedEndTime;
		bool searchUnderway = false;
		bool confused = false;
		const int numberOfOscillations = 3;
		float[] oscillationTimes = new float[3];
		int oscillationsOccured = 0;

		bool guardFound = false;
		Vector3 downedGuard;

		float maxVelocity;

		protected override void Start()
		{
			base.Start();
			player = GameObject.FindGameObjectWithTag("Player");
			rb = gameObject.GetComponent<Rigidbody2D>();
			anim = GetComponent<Animator>();
			audioManager = FindObjectOfType<AudioManager>();
			floor = transform.parent.parent;
			for (int i = 0; i < floor.childCount; i++)
			{
				if (floor.GetChild(i).GetChild(0) != transform && floor.GetChild(i).GetChild(0).name.Contains("Guard"))
				{
					fellowGuards.Add(floor.GetChild(i).GetChild(0));
				}
			}
			
			guardQuickSave = new GuardQuickSave();

			lastKnownLocation = transform.position;

			OnDeath += OnGuardDeath;
			OnHit += OnGuardHit;
			player.GetComponent<PlayerInfo>().OnDeath += OnPlayerDeath;
		}

		void FixedUpdate()
		{
			if (!playerDead && state != State.corrupted)
			{
				if (state == State.dead || state == State.unconscious)
				{
					if (!dead)
					{
						rb.linearVelocity = Vector2.zero;
						suspicionPercentage = 0f;
						//GetComponent<SpriteRenderer> ().color = state == State.dead ? Color.red : Color.blue;
						GetComponent<BoxCollider2D>().isTrigger = true;
						for (int i = 0; i < transform.childCount; i++)
						{
							transform.GetChild(i).gameObject.SetActive(false);
						}
						guardStopsHolder.SetActive(false);
					}

					if (rb.gravityScale > 0f)
					{
						Debug.DrawRay(transform.position, -Vector2.up * (transform.parent.localScale.y * 1.25f),
							Color.green);
						if (Physics2D.Raycast(transform.position, -Vector2.up, transform.parent.localScale.y * 1.25f,
							    whatIsGround))
						{
							rb.gravityScale = 0f;
							rb.linearVelocity = Vector2.zero;
							rb.constraints = RigidbodyConstraints2D.FreezePositionX |
							                 RigidbodyConstraints2D.FreezePositionY;
						}
					}

					dead = true;
				}
				else if (!inStasis)
				{
					if (!inForceField)
					{
						if (state == State.unaware)
						{
							if (!idling)
							{
								//transform.position = new Vector2 (Vector2.MoveTowards (transform.position, nextStop.position, walkSpeed * Time.fixedDeltaTime).x, transform.position.y);
								rb.linearVelocity = new Vector2(
									GetDirection(transform.position, nextStop.position) * walkSpeed,
									0f);
							}
							else if (Time.time > idleFinishTime)
							{
								idling = false;
								ChangeDirection();
							}
						}

						PlayerAwareness();

						if (state == State.chasing)
						{
							PlayerSeen();
						}

						if (state == State.alerted)
						{
							SearchingForPlayer(guardFound ? downedGuard : lastKnownLocation);
						}
					}
					else if (Mathf.Abs(rb.linearVelocity.magnitude) > maxVelocity)
						maxVelocity = Mathf.Abs(rb.linearVelocity.magnitude);
				}
			}
			else if (state == State.corrupted)
			{
				//Debug.Log ((closestGuard == transform) + " " + closestGuard.position + " " + transform.position);
				if (closestGuard != transform)
				{
					State closestGuardState = closestGuard.GetComponent<Guard>().state;
					if (closestGuardState != State.corrupted && closestGuardState != State.unconscious &&
					    closestGuardState != State.dead)
					{
						AttackGuard(closestGuard);
					}
					else
					{
						FindClosestGuard();
					}
				}
				else
				{
					//Play death animation
					TakeHit(1000);
				}
			}

			anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
		}

		////////////////////////////////////////////////////////////////////////////////////IDLE FUNCTIONS////////////////////////////////////////////////////////////////////////////////////////////

		//Takes information from the Guard Stop once one has been reached to determine the Guard's behavior.
		public void StopReached(float _idleTime, Vector2 _idleDirection, Transform _nextStop)
		{
			if (!playerDead && !inStasis && state != State.corrupted)
			{
				if (state != State.chasing && state != State.alerted)
				{
					idleTime = _idleTime;
					idleDirection = _idleDirection;
					nextStop = _nextStop;
					if (_idleTime > 0f)
					{
						Idle();
					}
					else
					{
						ChangeDirection();
					}
				}
			}
		}

		//Stops the Guard in place for a set amount of time while patrolling.
		void Idle()
		{
			idleFinishTime = Time.time + idleTime;
			transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * idleDirection.x,
				transform.localScale.y, 0);
			//anim.SetInteger ("Walk State", 0);
			idling = true;
		}

		//Flips the direction of the Guard while patrolling to look at the next stop.
		void ChangeDirection()
		{
			float newDirection = GetDirection(transform.position, nextStop.position);
			transform.localScale =
				new Vector3(Mathf.Abs(transform.localScale.x) * newDirection, transform.localScale.y, 0);
		}

		//Returns a 1 or a -1 depending on where the target is in repect to the object on the x axis.
		float GetDirection(Vector3 currentPosition, Vector3 targetPosition)
		{
			return targetPosition.x - currentPosition.x == 0f
				? 1f
				: Mathf.Round(Mathf.Abs(targetPosition.x - currentPosition.x) / (targetPosition.x - currentPosition.x));
		}

		//////////////////////////////////////////////////////////////////////////////PLAYER PERCEPTION FUNCTIONS/////////////////////////////////////////////////////////////////////////////////////

		//Is called by the Guard Vision when the Guard sees the player.
		public void SeesPlayer(float _playerVisibilityFactor)
		{
			if (_playerVisibilityFactor > 0 && !playerIsHiding)
			{
				playerVisibilityFactor = _playerVisibilityFactor;
				seesPlayer = true;
			}
		}

		//Is called by the Guard Vision when the Guard no longer sees the player.
		public void LostPlayer()
		{
			seesPlayer = false;
		}

		//Takes information from the Guard Vision and uses it to determine the Guard's behavior.
		void PlayerAwareness()
		{
			if (!playerDead && !inStasis && !inForceField && state != State.corrupted)
			{
				if (seesPlayer && suspicionPercentage < 1)
				{
					suspicionPercentage += playerVisibilityFactor * seesPlayerRate / 100f;
				}

				if (!seesPlayer && suspicionPercentage > 0 && state != State.alerted)
				{
					if (state == State.suspicious)
					{
						losesPlayerRate = 2.5f;
					}
					else if (state == State.chasing)
					{
						losesPlayerRate = 1f;
					}
					suspicionPercentage -= losesPlayerRate / 100f;
				}

				suspicionPercentage = Mathf.Clamp(suspicionPercentage, 0, 1);

				suspicionSpriteMask.localPosition = new Vector3(0f, suspicionPercentage * 0.625f, 0f);

				if (state != State.unaware && suspicionPercentage == 0f)
				{
					state = State.unaware;
					//anim.SetInteger ("Walk State", 1);
					canSpot = true;
				}

				if (state == State.unaware && suspicionPercentage > 0.5f)
				{
					state = State.suspicious;
				}

				if (state == State.suspicious && suspicionPercentage < 0.5f ||
				    state == State.alerted && suspicionPercentage == 0)
				{
					state = State.unaware;
					//anim.SetInteger ("Walk State", 1);
					canSpot = true;
				}

				if (suspicionPercentage == 1f)
				{
					if (state != State.chasing && !guardFound)
					{
						if (canSpot)
						{
							GameObject.FindGameObjectWithTag("GameController").GetComponent<MoralitySystem>()
								.timesSpotted++;
							audioManager.PlaySound("Guard Surprise");
							anim.SetTrigger("Surprised");
						}

						canSpot = false;

						for (int i = 0; i < fellowGuards.Count; i++)
						{
							if (fellowGuards[i] != transform)
							{
								fellowGuards[i].GetComponent<Guard>().PlayerSeenByOther();
							}
						}
					}

					state = State.chasing;
				}

				if (state == State.chasing && !seesPlayer || guardFound)
				{
					state = State.alerted;
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////////////ALERTED FUNCTIONS///////////////////////////////////////////////////////////////////////////////////////////

		//Is called by the Guard Vision when the player is right in front of the Guard.
		public void PlayerInMeleeRange(bool inRange)
		{
			playerInMeleeRange = inRange;
			if (!playerDead && inRange && player.tag.Equals("Player") && !inStasis && !inForceField &&
			    state != State.corrupted)
			{
				suspicionPercentage = 1f;
			}
		}

		//Is called when another guard on the same floor has seen the player.
		public void PlayerSeenByOther()
		{
			if (state != State.dead && state != State.unconscious && !playerDead && !inStasis && !inForceField &&
			    state != State.corrupted)
			{
				suspicionPercentage = 1f;
				state = State.alerted;
				lastKnownLocation = player.transform.position;
				lastSeenTime = Time.time;
				transform.localScale =
					new Vector3(
						Mathf.Abs(transform.localScale.x) * GetDirection(transform.position, player.transform.position),
						transform.localScale.y, 0);
				state = State.chasing;
				audioManager.PlaySound("Guard Surprise");
			}
		}

		//Is called when the guard finds an incapacitated compatriot.
		public void FoundGuard(Guard fellowGuard)
		{
			if (state != State.chasing && !playerDead && !inStasis && !inForceField && state != State.corrupted)
			{
				suspicionPercentage = 1f;
				transform.localScale =
					new Vector3(
						Mathf.Abs(transform.localScale.x) *
						GetDirection(transform.position, fellowGuard.transform.position), transform.localScale.y, 0);
				guardFound = true;
				downedGuard = fellowGuard.transform.position;
				audioManager.PlaySound("Guard Surprise");
				anim.SetTrigger("Surprised");
			}
		}

		//Chases the player around and attacks them while the player is in sight and alerts other guards on the same floor.
		void PlayerSeen()
		{
			if (state != State.dead && !playerDead && !inStasis && !inForceField && state != State.corrupted)
			{
				idling = false;
				if (playerInMeleeRange)
				{
					if (timeBetweenAttack <= 0)
					{
						Collider2D[] enemiesToDamage =
							Physics2D.OverlapCircleAll(attackPos.position, attackRange, whatAreEnemies);
						enemiesToDamage[0].GetComponent<LivingEntity>().TakeHit(damage);
						timeBetweenAttack = startTimeBetweenAttack;
						audioManager.PlaySound("Swipe");
						anim.SetTrigger("Attacking");
					}
					else
						timeBetweenAttack -= Time.deltaTime;
				}
				else
				{
					//transform.position = new Vector2 (Vector2.MoveTowards (transform.position, player.transform.position, runSpeed * Time.fixedDeltaTime).x, transform.position.y);
					rb.linearVelocity = new Vector2(GetDirection(transform.position, player.transform.position) * runSpeed,
						0f);
				}

				if (seesPlayer)
				{
					lastKnownLocation = player.transform.position;
					lastSeenTime = Time.time;
				}
			}
		}

		//Searches for the player. Works while the guard is aware that the player is around, but can't see them currently.
		void SearchingForPlayer(Vector3 searchArea)
		{
			if (guardFound && Vector2.Distance(transform.position, searchArea) > 0.1f ||
			    Time.time < lastSeenTime + giveUpTime)
			{
				//transform.position = new Vector2 (Vector2.MoveTowards (transform.position, searchArea, runSpeed * Time.fixedDeltaTime).x, transform.position.y);
				rb.linearVelocity = new Vector2(GetDirection(transform.position, searchArea) * runSpeed, 0f);
				alertedEndTime = Time.time + alertedTime;
			}
			else if (alertedEndTime >= Time.time)
			{
				if (!searchUnderway)
				{
					guardFound = false;
					searchUnderway = true;
					for (int i = 0; i < numberOfOscillations; i++)
					{
						oscillationTimes[i] = Time.time + alertedTime / numberOfOscillations * (i + 1);
					}
				}
				else
				{
					//Confusion period where the Guard stays in place but looks back and forth. If the guard doesn't see the player after so many seconds, have it return to patrol.
					if (!confused)
					{
						anim.SetTrigger("Confused");
						confused = true;
					}

					if (oscillationsOccured < oscillationTimes.Length
					    && Time.time > oscillationTimes[oscillationsOccured])
					{
						transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, 0);
						oscillationsOccured++;
					}
				}
			}
			else
			{
				searchUnderway = false;
				oscillationsOccured = 0;
				state = State.suspicious;
				if (transform.GetComponent<BoxCollider2D>().IsTouching(nextStop.GetComponent<BoxCollider2D>()))
				{
					nextStop.GetComponent<GuardStop>().ForceUpdate();
				}
				else
				{
					ChangeDirection();
				}
			}
		}

		//Seeks and attacks the closest Guard while corrupted
		void AttackGuard(Transform fellowGuard)
		{
			if (state == State.corrupted && !playerDead && !inStasis && !inForceField)
			{
				if (transform.GetChild(0).GetComponent<CircleCollider2D>()
				    .IsTouching(fellowGuard.GetComponent<BoxCollider2D>()))
				{
					if (timeBetweenAttack <= 0)
					{
						Collider2D[] friendsToDamage =
							Physics2D.OverlapCircleAll(attackPos.position, attackRange, whatAreFriends);
						for (int i = 0; i < friendsToDamage.Length; i++)
						{
							if (friendsToDamage[i].transform != transform
							    && friendsToDamage[i].name.Equals("Guard Actual"))
							{
								friendsToDamage[i].GetComponent<LivingEntity>().TakeHit(1000);
							}
						}

						audioManager.PlaySound("Swipe");
						anim.SetTrigger("Attacking");
						timeBetweenAttack = startTimeBetweenAttack;
					}
					else
					{
						timeBetweenAttack -= Time.deltaTime;
					}
				}
				else
				{
					//transform.position = new Vector2 (Vector2.MoveTowards (transform.position, fellowGuard.position, runSpeed * Time.fixedDeltaTime).x, transform.position.y);
					rb.linearVelocity = new Vector2(GetDirection(transform.position, fellowGuard.position) * runSpeed, 0f);
					transform.localScale =
						new Vector3(
							Mathf.Abs(transform.localScale.x) * GetDirection(transform.position, closestGuard.position),
							transform.localScale.y, 0);
				}
			}
		}

		//Draws the red circle gizmo to show the Guard's attack range.
		void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(attackPos.position, attackRange);
		}

		//////////////////////////////////////////////////////////////////////////////PLAYER ABILITY FUNCTIONS////////////////////////////////////////////////////////////////////////////////////////

		//Is called when the player has entered a natural hiding place to force the guard to lose them.
		public void PlayerHiding(bool _playerIsHiding)
		{
			playerIsHiding = _playerIsHiding;
			LostPlayer();
		}

		//Is called if the Guard is in a Stasis Bubble.
		public void InStasis(bool _inStasis)
		{
			inStasis = _inStasis;
			anim.speed = _inStasis ? 0f : 1f;
		}

		//Is called when the Guard is corrupted by the Traitor ability.
		public void Corrupt()
		{
			state = State.corrupted;
			//GetComponent<SpriteRenderer> ().color = Color.yellow;
			suspicionSpriteMask.parent.gameObject.SetActive(false);
			FindClosestGuard();
			anim.SetBool("Corrupted", true);
			anim.SetTrigger("Cursed");
		}

		//Finds the closest Guard to attack.
		void FindClosestGuard()
		{
			closestGuard = transform;
			float closestGuardDistance = 0f;
			if (fellowGuards.Count > 0)
			{
				for (int i = 0; i < fellowGuards.Count; i++)
				{
					float currentDistance = Vector2.Distance(transform.position, fellowGuards[i].position);
					State fellowGuardState = fellowGuards[i].GetComponent<Guard>().state;
					if ((closestGuardDistance == 0 || currentDistance < closestGuardDistance) &&
					    fellowGuardState != State.corrupted && fellowGuardState != State.unconscious &&
					    fellowGuardState != State.dead)
					{
						closestGuard = fellowGuards[i];
						closestGuardDistance = currentDistance;
					}
				}
			}
			//Debug.Log (closestGuard.parent.name);
		}

		///////////////////////////////////////////////////////////////////////////////////EVENT FUNCTIONS////////////////////////////////////////////////////////////////////////////////////////////

		//Sets the guard's state to alert when it's been hit.
		public void OnGuardHit(float timeHit, float startingHealth, float currentHealth)
		{
			if (!playerDead && !inStasis && !inForceField && state != State.corrupted)
			{
				if (currentHealth > 0f)
				{
					audioManager.PlaySound("Guard Hurt");
					suspicionPercentage = 1f;
					transform.localScale =
						new Vector3(
							Mathf.Abs(transform.localScale.x)
							* GetDirection(transform.position, player.transform.position),
							transform.localScale.y, 0);
					state = State.alerted;
				}
			}
		}

		//Sets the Guard and the Guard Stops inactive upon its death.
		public void OnGuardDeath()
		{
			if (state == State.corrupted)
			{
				audioManager.PlaySound("Corrupted Guard Death");
			}

			state = State.dead;
			anim.speed = 1f;
			anim.SetTrigger("Dies");
			anim.SetBool("Dead", true);
			GameObject.FindGameObjectWithTag("GameController").GetComponent<MoralitySystem>().enemiesKilled++;
		}

		//Calls when the guard is knocked unconscious.
		public void OnGuardUnconscious()
		{
			anim.speed = 1f;
			anim.SetTrigger("Unconscious");
			anim.SetBool("Dead", true);
			state = State.unconscious;
		}

		//Calls when the player dies.
		void OnPlayerDeath()
		{
			playerDead = true;
			state = State.unaware;
			suspicionPercentage = 0f;
		}

		/////////////////////////////////////////////////////////////////////////////////COLLISION FUNCTIONS//////////////////////////////////////////////////////////////////////////////////////////

		void OnCollisionEnter2D(Collision2D collision)
		{
			if (collision.collider.tag.Equals("Player") && !player.GetComponent<PlayerInfo>().inShadowSink
			                                            && !playerDead &&
			                                            !inStasis && !inForceField && state != State.corrupted)
			{
				suspicionPercentage = 1f;
				state = State.alerted;
				lastKnownLocation = player.transform.position;
				lastSeenTime = Time.time;
				transform.localScale =
					new Vector3(
						Mathf.Abs(transform.localScale.x) * GetDirection(transform.position, player.transform.position),
						transform.localScale.y, 0);
			}
			else if (Mathf.Abs(rb.linearVelocity.x) <= 0.01f)
			{
				if ((collision.collider.gameObject.layer == 11 || collision.collider.gameObject.layer == 12 ||
				     collision.collider.gameObject.layer == 13) && maxVelocity >= 30f)
				{
					GetComponent<LivingEntity>().TakeHit(1000f);
				}
				else if ((collision.collider.name.Contains("Crate") || collision.collider.name.Contains("Desk")) &&
				         maxVelocity >= 10f)
				{
					OnGuardUnconscious();
				}
			}
		}

		void OnTriggerEnter2D(Collider2D collider)
		{
			if (collider.name.Contains("Extreme Force"))
			{
				inForceField = true;
			}
		}

		////////////////////////////////////////////////////////////////////////////////QUICK SAVE FUNCTIONS//////////////////////////////////////////////////////////////////////////////////////////

		public void QuickSave()
		{
			guardQuickSave.position = transform.position;
			guardQuickSave.direction = transform.localScale.x;
			guardQuickSave.nextStop = nextStop;
			guardQuickSave.health = health;
			guardQuickSave.state = state;
			guardQuickSave.suspicionPercentage = suspicionPercentage;
		}

		public void QuickLoad()
		{
			transform.position = guardQuickSave.position;
			transform.localScale =
				new Vector3(guardQuickSave.direction, transform.localScale.y, transform.localScale.z);
			nextStop = guardQuickSave.nextStop;
			health = guardQuickSave.health;
			state = guardQuickSave.state;
			playerDead = false;
			anim.SetBool("Dead", false);
			suspicionPercentage = guardQuickSave.suspicionPercentage;
			if (dead && state != State.dead && state != State.unconscious)
			{
				//GetComponent<SpriteRenderer> ().color = Color.white;
				GetComponent<BoxCollider2D>().isTrigger = false;
				for (int i = 0; i < transform.childCount; i++)
				{
					transform.GetChild(i).gameObject.SetActive(true);
				}

				guardStopsHolder.SetActive(true);
				rb.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
				dead = false;
			}
		}
	}
}
