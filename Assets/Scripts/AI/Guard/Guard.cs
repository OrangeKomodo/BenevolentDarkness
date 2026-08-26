using System.Collections.Generic;
using DamageSystem;
using GameManager;
using Player;
using UnityEngine;

namespace AI.Guard
{
	public class Guard : LivingEntity
	{
		public enum GuardState
		{
			Unaware,
			Suspicious,
			Chasing,
			Alerted,
			Dead,
			Unconscious,
			Corrupted
		}
		
		struct GuardQuickSave
		{
			public Vector2 Position;
			public float Direction;
			public Transform NextStop;
			public float Health;
			public GuardState State;
			public float SuspicionPercentage;
		}

		public GuardState State = GuardState.Unaware;
		public GameObject GuardStopsHolder;
		public Transform NextStop;
		public float WalkSpeed;
		public float RunSpeed;
		[Range(0, 1)]
		public float SuspicionPercentage = 0f;
		public bool SeesPlayer = false;
		public bool CanMimic = true;
		public int Damage;
		public Transform AttackPos;
		public float AttackRange;
		public float StartTimeBetweenAttack;
		public LayerMask WhatAreEnemies;
		public LayerMask WhatAreFriends;
		public LayerMask WhatIsGround;
		public Transform SuspicionSpriteMask;

		private List<Transform> _fellowGuards = new List<Transform>();
		private Transform _closestGuard;

		private GameObject _player;
		private Rigidbody2D _rigidbody;
		private Animator _animator;
		private BoxCollider2D _collider;
		private Transform _floor;
		private GuardQuickSave _guardQuickSave;

		private float _timeBetweenAttack;

		private bool _idling;
		private bool _playerInMeleeRange;
		private bool _playerDead;
		private bool _inStasis = false;
		private bool _inForceField = false;
		private bool _deathManaged = false;

		private float _idleTime;
		private Vector2 _idleDirection;
		private float _idleFinishTime;

		private bool _playerIsHiding = false;
		private float _playerVisibilityFactor = 0f;
		private float _seesPlayerRate = 5f;
		private float _losesPlayerRate = 5f;

		private Vector3 _lastKnownLocation;
		private float _giveUpTime = 5f;
		private float _lastSeenTime;
		private bool _canSpot = true;

		private float _alertedTime = 7f;
		private float _alertedEndTime;
		private bool _searchUnderway = false;
		private bool _confused = false;
		private const int NumberOfOscillations = 3;
		private float[] _oscillationTimes = new float[3];
		private int _oscillationsOccured = 0;

		private bool _guardFound = false;
		private Vector3 _downedGuard;

		private float _maxVelocity;

		protected override void Start()
		{
			base.Start();
			
			_player = GameObject.FindGameObjectWithTag("Player");
			_rigidbody = GetComponent<Rigidbody2D>();
			_animator = GetComponent<Animator>();
			_collider = GetComponent<BoxCollider2D>();
			_floor = transform.parent.parent;
			for (int i = 0; i < _floor.childCount; i++)
			{
				if (_floor.GetChild(i).GetChild(0) != transform && _floor.GetChild(i).GetChild(0).name.Contains("Guard"))
				{
					_fellowGuards.Add(_floor.GetChild(i).GetChild(0));
				}
			}
			
			_guardQuickSave = new GuardQuickSave();

			_lastKnownLocation = transform.position;

			OnHit += OnGuardHit;
			_player.GetComponent<PlayerController>().OnDeath += OnPlayerDeath;
		}

		private void OnApplicationQuit()
		{
			OnHit -= OnGuardHit;
			_player.GetComponent<PlayerController>().OnDeath -= OnPlayerDeath;
		}

		private void FixedUpdate()
		{
			if (!_playerDead && State != GuardState.Corrupted)
			{
				if (State == GuardState.Dead || State == GuardState.Unconscious)
				{
					if (!_deathManaged)
					{
						_rigidbody.linearVelocity = Vector2.zero;
						SuspicionPercentage = 0f;
						//GetComponent<SpriteRenderer> ().color = state == State.dead ? Color.red : Color.blue;
						_collider.isTrigger = true;
						for (int i = 0; i < transform.childCount; i++)
						{
							transform.GetChild(i).gameObject.SetActive(false);
						}
						GuardStopsHolder.SetActive(false);

						_deathManaged = true;
					}

					if (_rigidbody.gravityScale > 0f)
					{
						Debug.DrawRay(transform.position, -Vector2.up * (transform.parent.localScale.y * 1.25f),
							Color.green);
						if (Physics2D.Raycast(transform.position, -Vector2.up, transform.parent.localScale.y * 1.25f,
							    WhatIsGround))
						{
							_rigidbody.gravityScale = 0f;
							_rigidbody.linearVelocity = Vector2.zero;
							_rigidbody.constraints = RigidbodyConstraints2D.FreezePositionX |
							                 RigidbodyConstraints2D.FreezePositionY;
						}
					}
				}
				else if (!_inStasis)
				{
					if (!_inForceField)
					{
						if (State == GuardState.Unaware)
						{
							if (!_idling)
							{
								//transform.position = new Vector2 (Vector2.MoveTowards (transform.position, nextStop.position, walkSpeed * Time.fixedDeltaTime).x, transform.position.y);
								_rigidbody.linearVelocity = new Vector2(
									GetDirection(transform.position, NextStop.position) * WalkSpeed,
									0f);
							}
							else if (Time.time > _idleFinishTime)
							{
								_idling = false;
								ChangeDirection();
							}
						}

						PlayerAwareness();

						if (State == GuardState.Chasing)
						{
							PlayerSeen();
						}

						if (State == GuardState.Alerted)
						{
							SearchingForPlayer(_guardFound ? _downedGuard : _lastKnownLocation);
						}
					}
					else if (Mathf.Abs(_rigidbody.linearVelocity.magnitude) > _maxVelocity)
						_maxVelocity = Mathf.Abs(_rigidbody.linearVelocity.magnitude);
				}
			}
			else if (State == GuardState.Corrupted)
			{
				//Debug.Log ((closestGuard == transform) + " " + closestGuard.position + " " + transform.position);
				if (_closestGuard != transform)
				{
					GuardState closestGuardGuardState = _closestGuard.GetComponent<Guard>().State;
					if (closestGuardGuardState != GuardState.Corrupted && closestGuardGuardState != GuardState.Unconscious &&
					    closestGuardGuardState != GuardState.Dead)
					{
						AttackGuard(_closestGuard);
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

			_animator.SetFloat("Speed", Mathf.Abs(_rigidbody.linearVelocity.x));
		}

		////////////////////////////////////////////////////////////////////////////////////IDLE FUNCTIONS////////////////////////////////////////////////////////////////////////////////////////////

		//Takes information from the Guard Stop once one has been reached to determine the Guard's behavior.
		public void StopReached(float idleTime, Vector2 idleDirection, Transform nextStop)
		{
			if (!Dead && !_inStasis && State != GuardState.Corrupted)
			{
				if (State != GuardState.Chasing && State != GuardState.Alerted)
				{
					_idleTime = idleTime;
					_idleDirection = idleDirection;
					NextStop = nextStop;
					if (idleTime > 0f)
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
		private void Idle()
		{
			_idleFinishTime = Time.time + _idleTime;
			transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * _idleDirection.x,
				transform.localScale.y, 0);
			//anim.SetInteger ("Walk State", 0);
			_idling = true;
		}

		//Flips the direction of the Guard while patrolling to look at the next stop.
		private void ChangeDirection()
		{
			float newDirection = GetDirection(transform.position, NextStop.position);
			transform.localScale =
				new Vector3(Mathf.Abs(transform.localScale.x) * newDirection, transform.localScale.y, 0);
		}

		//Returns a 1 or a -1 depending on where the target is in repect to the object on the x axis.
		private float GetDirection(Vector3 currentPosition, Vector3 targetPosition)
		{
			return targetPosition.x - currentPosition.x == 0f
				? 1f
				: Mathf.Round(Mathf.Abs(targetPosition.x - currentPosition.x) / (targetPosition.x - currentPosition.x));
		}

		//////////////////////////////////////////////////////////////////////////////PLAYER PERCEPTION FUNCTIONS/////////////////////////////////////////////////////////////////////////////////////

		//Is called by the Guard Vision when the Guard sees the player.
		public void CheckSeesPlayer(float playerVisibilityFactor)
		{
			if (playerVisibilityFactor > 0 && !_playerIsHiding)
			{
				this._playerVisibilityFactor = playerVisibilityFactor;
				SeesPlayer = true;
			}
		}

		//Is called by the Guard Vision when the Guard no longer sees the player.
		public void LostPlayer()
		{
			SeesPlayer = false;
		}

		//Takes information from the Guard Vision and uses it to determine the Guard's behavior.
		private void PlayerAwareness()
		{
			if (!Dead && !_inStasis && !_inForceField && State != GuardState.Corrupted)
			{
				if (SeesPlayer && SuspicionPercentage < 1)
				{
					SuspicionPercentage += _playerVisibilityFactor * _seesPlayerRate / 100f;
				}

				if (!SeesPlayer && SuspicionPercentage > 0 && State != GuardState.Alerted)
				{
					if (State == GuardState.Suspicious)
					{
						_losesPlayerRate = 2.5f;
					}
					else if (State == GuardState.Chasing)
					{
						_losesPlayerRate = 1f;
					}
					SuspicionPercentage -= _losesPlayerRate / 100f;
				}

				SuspicionPercentage = Mathf.Clamp(SuspicionPercentage, 0, 1);

				SuspicionSpriteMask.localPosition = new Vector3(0f, SuspicionPercentage * 0.625f, 0f);

				if (State != GuardState.Unaware && SuspicionPercentage == 0f)
				{
					State = GuardState.Unaware;
					//anim.SetInteger ("Walk State", 1);
					_canSpot = true;
				}

				if (State == GuardState.Unaware && SuspicionPercentage > 0.5f)
				{
					State = GuardState.Suspicious;
				}

				if (State == GuardState.Suspicious && SuspicionPercentage < 0.5f ||
				    State == GuardState.Alerted && SuspicionPercentage == 0)
				{
					State = GuardState.Unaware;
					//anim.SetInteger ("Walk State", 1);
					_canSpot = true;
				}

				if (SuspicionPercentage == 1f)
				{
					if (State != GuardState.Chasing && !_guardFound)
					{
						if (_canSpot)
						{
							MoralitySystem.Instance.TimesSpotted++;
							AudioManager.Instance.PlaySound("Guard Surprise");
							_animator.SetTrigger("Surprised");
						}

						_canSpot = false;

						for (int i = 0; i < _fellowGuards.Count; i++)
						{
							if (_fellowGuards[i] != transform)
							{
								_fellowGuards[i].GetComponent<Guard>().PlayerSeenByOther();
							}
						}
					}

					State = GuardState.Chasing;
				}

				if (State == GuardState.Chasing && !SeesPlayer || _guardFound)
				{
					State = GuardState.Alerted;
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////////////ALERTED FUNCTIONS///////////////////////////////////////////////////////////////////////////////////////////

		//Is called by the Guard Vision when the player is right in front of the Guard.
		public void PlayerInMeleeRange(bool inRange)
		{
			_playerInMeleeRange = inRange;
			if (!Dead && inRange && _player.tag.Equals("Player") && !_inStasis && !_inForceField &&
			    State != GuardState.Corrupted)
			{
				SuspicionPercentage = 1f;
			}
		}

		//Is called when another guard on the same floor has seen the player.
		public void PlayerSeenByOther()
		{
			if (State != GuardState.Dead && State != GuardState.Unconscious && !Dead && !_inStasis && !_inForceField &&
			    State != GuardState.Corrupted)
			{
				SuspicionPercentage = 1f;
				State = GuardState.Alerted;
				_lastKnownLocation = _player.transform.position;
				_lastSeenTime = Time.time;
				transform.localScale =
					new Vector3(
						Mathf.Abs(transform.localScale.x) * GetDirection(transform.position, _player.transform.position),
						transform.localScale.y, 0);
				State = GuardState.Chasing;
				AudioManager.Instance.PlaySound("Guard Surprise");
			}
		}

		//Is called when the guard finds an incapacitated compatriot.
		public void FoundGuard(Guard fellowGuard)
		{
			if (State != GuardState.Chasing && !Dead && !_inStasis && !_inForceField && State != GuardState.Corrupted)
			{
				SuspicionPercentage = 1f;
				transform.localScale =
					new Vector3(
						Mathf.Abs(transform.localScale.x) *
						GetDirection(transform.position, fellowGuard.transform.position), transform.localScale.y, 0);
				_guardFound = true;
				_downedGuard = fellowGuard.transform.position;
				AudioManager.Instance.PlaySound("Guard Surprise");
				_animator.SetTrigger("Surprised");
			}
		}

		//Chases the player around and attacks them while the player is in sight and alerts other guards on the same floor.
		private void PlayerSeen()
		{
			if (State != GuardState.Dead && !Dead && !_inStasis && !_inForceField && State != GuardState.Corrupted)
			{
				_idling = false;
				if (_playerInMeleeRange)
				{
					if (_timeBetweenAttack <= 0)
					{
						Collider2D[] enemiesToDamage =
							Physics2D.OverlapCircleAll(AttackPos.position, AttackRange, WhatAreEnemies);
						enemiesToDamage[0].GetComponent<LivingEntity>().TakeHit(Damage);
						_timeBetweenAttack = StartTimeBetweenAttack;
						AudioManager.Instance.PlaySound("Swipe");
						_animator.SetTrigger("Attacking");
					}
					else
						_timeBetweenAttack -= Time.deltaTime;
				}
				else
				{
					//transform.position = new Vector2 (Vector2.MoveTowards (transform.position, player.transform.position, runSpeed * Time.fixedDeltaTime).x, transform.position.y);
					_rigidbody.linearVelocity = new Vector2(GetDirection(transform.position, _player.transform.position) * RunSpeed,
						0f);
				}

				if (SeesPlayer)
				{
					_lastKnownLocation = _player.transform.position;
					_lastSeenTime = Time.time;
				}
			}
		}

		//Searches for the player. Works while the guard is aware that the player is around, but can't see them currently.
		private void SearchingForPlayer(Vector3 searchArea)
		{
			if (_guardFound && Vector2.Distance(transform.position, searchArea) > 0.1f ||
			    Time.time < _lastSeenTime + _giveUpTime)
			{
				//transform.position = new Vector2 (Vector2.MoveTowards (transform.position, searchArea, runSpeed * Time.fixedDeltaTime).x, transform.position.y);
				_rigidbody.linearVelocity = new Vector2(GetDirection(transform.position, searchArea) * RunSpeed, 0f);
				_alertedEndTime = Time.time + _alertedTime;
			}
			else if (_alertedEndTime >= Time.time)
			{
				if (!_searchUnderway)
				{
					_guardFound = false;
					_searchUnderway = true;
					for (int i = 0; i < NumberOfOscillations; i++)
					{
						_oscillationTimes[i] = Time.time + _alertedTime / NumberOfOscillations * (i + 1);
					}
				}
				else
				{
					//Confusion period where the Guard stays in place but looks back and forth. If the guard doesn't see the player after so many seconds, have it return to patrol.
					if (!_confused)
					{
						_animator.SetTrigger("Confused");
						_confused = true;
					}

					if (_oscillationsOccured < _oscillationTimes.Length
					    && Time.time > _oscillationTimes[_oscillationsOccured])
					{
						transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, 0);
						_oscillationsOccured++;
					}
				}
			}
			else
			{
				_searchUnderway = false;
				_oscillationsOccured = 0;
				State = GuardState.Suspicious;
				if (_collider.IsTouching(NextStop.GetComponent<BoxCollider2D>()))
				{
					NextStop.GetComponent<GuardStop>().ForceUpdate();
				}
				else
				{
					ChangeDirection();
				}
			}
		}

		//Seeks and attacks the closest Guard while corrupted
		private void AttackGuard(Transform fellowGuard)
		{
			if (State == GuardState.Corrupted && !Dead && !_inStasis && !_inForceField)
			{
				if (transform.GetChild(0).GetComponent<CircleCollider2D>()
				    .IsTouching(fellowGuard.GetComponent<BoxCollider2D>()))
				{
					if (_timeBetweenAttack <= 0)
					{
						Collider2D[] friendsToDamage =
							Physics2D.OverlapCircleAll(AttackPos.position, AttackRange, WhatAreFriends);
						for (int i = 0; i < friendsToDamage.Length; i++)
						{
							if (friendsToDamage[i].transform != transform
							    && friendsToDamage[i].name.Equals("Guard Actual"))
							{
								friendsToDamage[i].GetComponent<LivingEntity>().TakeHit(1000);
							}
						}

						AudioManager.Instance.PlaySound("Swipe");
						_animator.SetTrigger("Attacking");
						_timeBetweenAttack = StartTimeBetweenAttack;
					}
					else
					{
						_timeBetweenAttack -= Time.deltaTime;
					}
				}
				else
				{
					//transform.position = new Vector2 (Vector2.MoveTowards (transform.position, fellowGuard.position, runSpeed * Time.fixedDeltaTime).x, transform.position.y);
					_rigidbody.linearVelocity = new Vector2(GetDirection(transform.position, fellowGuard.position) * RunSpeed, 0f);
					transform.localScale =
						new Vector3(
							Mathf.Abs(transform.localScale.x) * GetDirection(transform.position, _closestGuard.position),
							transform.localScale.y, 0);
				}
			}
		}

		//Draws the red circle gizmo to show the Guard's attack range.
		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(AttackPos.position, AttackRange);
		}

		//////////////////////////////////////////////////////////////////////////////PLAYER ABILITY FUNCTIONS////////////////////////////////////////////////////////////////////////////////////////

		//Is called when the player has entered a natural hiding place to force the guard to lose them.
		public void PlayerHiding(bool _playerIsHiding)
		{
			this._playerIsHiding = _playerIsHiding;
			LostPlayer();
		}

		//Is called if the Guard is in a Stasis Bubble.
		public void InStasis(bool _inStasis)
		{
			this._inStasis = _inStasis;
			_animator.speed = _inStasis ? 0f : 1f;
		}

		//Is called when the Guard is corrupted by the Traitor ability.
		public void Corrupt()
		{
			State = GuardState.Corrupted;
			//GetComponent<SpriteRenderer> ().color = Color.yellow;
			SuspicionSpriteMask.parent.gameObject.SetActive(false);
			FindClosestGuard();
			_animator.SetBool("Corrupted", true);
			_animator.SetTrigger("Cursed");
		}

		//Finds the closest Guard to attack.
		private void FindClosestGuard()
		{
			_closestGuard = transform;
			float closestGuardDistance = 0f;
			if (_fellowGuards.Count > 0)
			{
				for (int i = 0; i < _fellowGuards.Count; i++)
				{
					float currentDistance = Vector2.Distance(transform.position, _fellowGuards[i].position);
					GuardState fellowGuardGuardState = _fellowGuards[i].GetComponent<Guard>().State;
					if ((closestGuardDistance == 0 || currentDistance < closestGuardDistance) &&
					    fellowGuardGuardState != GuardState.Corrupted && fellowGuardGuardState != GuardState.Unconscious &&
					    fellowGuardGuardState != GuardState.Dead)
					{
						_closestGuard = _fellowGuards[i];
						closestGuardDistance = currentDistance;
					}
				}
			}
			//Debug.Log (closestGuard.parent.name);
		}

		///////////////////////////////////////////////////////////////////////////////////EVENT FUNCTIONS////////////////////////////////////////////////////////////////////////////////////////////

		//Sets the guard's state to alert when it's been hit.
		private void OnGuardHit(float timeHit, float startingHealth, float currentHealth)
		{
			if (Dead || _inStasis || _inForceField || State == GuardState.Corrupted || currentHealth <= 0f)
			{
				return;
			}
			
			AudioManager.Instance.PlaySound("Guard Hurt");
			SuspicionPercentage = 1f;
			transform.localScale =
				new Vector3(
					Mathf.Abs(transform.localScale.x) * GetDirection(transform.position, _player.transform.position),
					transform.localScale.y, 0);
			State = GuardState.Alerted;
		}

		public override void TakeHit(float damage)
		{
			base.TakeHit(damage);
		}

		public override void Heal(float heals)
		{
			base.Heal(heals);
		}

		//Sets the Guard and the Guard Stops inactive upon its death.
		protected override void Die()
		{
			base.Die();
			
			if (State == GuardState.Corrupted)
			{
				AudioManager.Instance.PlaySound("Corrupted Guard Death");
			}

			State = GuardState.Dead;
			_animator.speed = 1f;
			_animator.SetTrigger("Dies");
			_animator.SetBool("Dead", true);
			MoralitySystem.Instance.EnemiesKilled++;
		}

		//Calls when the guard is knocked unconscious.
		public void OnGuardUnconscious()
		{
			_animator.speed = 1f;
			_animator.SetTrigger("Unconscious");
			_animator.SetBool("Dead", true);
			State = GuardState.Unconscious;
		}

		//Calls when the player dies.
		private void OnPlayerDeath()
		{
			_playerDead = true;
			State = GuardState.Unaware;
			SuspicionPercentage = 0f;
		}

		/////////////////////////////////////////////////////////////////////////////////COLLISION FUNCTIONS//////////////////////////////////////////////////////////////////////////////////////////

		private void OnCollisionEnter2D(Collision2D collision)
		{
			if (collision.collider.tag.Equals("Player") && !_player.GetComponent<PlayerController>().InShadowSink
			                                            && !Dead && !_inStasis && !_inForceField
			                                            && State != GuardState.Corrupted)
			{
				SuspicionPercentage = 1f;
				State = GuardState.Alerted;
				_lastKnownLocation = _player.transform.position;
				_lastSeenTime = Time.time;
				transform.localScale =
					new Vector3(
						Mathf.Abs(transform.localScale.x) * GetDirection(transform.position, _player.transform.position),
						transform.localScale.y, 0);
			}
			else if (Mathf.Abs(_rigidbody.linearVelocity.x) <= 0.01f)
			{
				if ((collision.collider.gameObject.layer == 11 || collision.collider.gameObject.layer == 12 ||
				     collision.collider.gameObject.layer == 13) && _maxVelocity >= 30f)
				{
					TakeHit(1000f);
				}
				else if ((collision.collider.name.Contains("Crate") || collision.collider.name.Contains("Desk")) &&
				         _maxVelocity >= 10f)
				{
					OnGuardUnconscious();
				}
			}
		}

		private void OnTriggerEnter2D(Collider2D otherCollider)
		{
			if (otherCollider.name.Contains("Extreme Force"))
			{
				_inForceField = true;
			}
		}

		////////////////////////////////////////////////////////////////////////////////QUICK SAVE FUNCTIONS//////////////////////////////////////////////////////////////////////////////////////////

		public void QuickSave()
		{
			_guardQuickSave.Position = transform.position;
			_guardQuickSave.Direction = transform.localScale.x;
			_guardQuickSave.NextStop = NextStop;
			_guardQuickSave.Health = Health;
			_guardQuickSave.State = State;
			_guardQuickSave.SuspicionPercentage = SuspicionPercentage;
		}

		public void QuickLoad()
		{
			transform.position = _guardQuickSave.Position;
			transform.localScale =
				new Vector3(_guardQuickSave.Direction, transform.localScale.y, transform.localScale.z);
			NextStop = _guardQuickSave.NextStop;
			Health = _guardQuickSave.Health;
			State = _guardQuickSave.State;
			_playerDead = false;
			_animator.SetBool("Dead", false);
			SuspicionPercentage = _guardQuickSave.SuspicionPercentage;
			if (Dead && State != GuardState.Dead && State != GuardState.Unconscious)
			{
				_collider.isTrigger = false;
				for (int i = 0; i < transform.childCount; i++)
				{
					transform.GetChild(i).gameObject.SetActive(true);
				}

				GuardStopsHolder.SetActive(true);
				_rigidbody.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
				Dead = false;
			}
		}
	}
}
