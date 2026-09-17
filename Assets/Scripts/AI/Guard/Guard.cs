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
		public CircleCollider2D GuardVisionCircle;
		public Transform AttackPos;
		public float AttackRange;
		public float StartTimeBetweenAttack;
		public LayerMask WhatAreEnemies;
		public LayerMask WhatAreFriends;
		public LayerMask WhatIsGround;
		public LayerMask WhatIsWall;
		public Transform SuspicionSpriteMask;
		public Transform Head;

		private Guard[] _fellowGuards;
		private Guard _closestGuard;

		private PlayerController _playerController;
		private Animator _animator;
		
		private Rigidbody2D _rigidbody;
		public Rigidbody2D RigidBody => _rigidbody;
		private BoxCollider2D _collider;
		public BoxCollider2D Collider => _collider;
		
		private Transform _floor;
		private GuardQuickSave _guardQuickSave;

		private float _timeBetweenAttack;

		private bool _idling;
		private bool _playerInMeleeRange;
		private bool _playerDead;
		private bool _inStasis = false;
		private bool _inForceField = false;
		private bool _incapacitationManaged = false;
		private bool _groundReached = false;

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

		private float _playerAwarenessUpdateTime = 0.02f;
		private float _nextPlayerAwarenessUpdate;
		private float _alertedTime = 7f;
		private float _alertedEndTime;
		private bool _searchUnderway = false;
		private bool _confused = false;
		private const int NumberOfOscillations = 3;
		private float[] _oscillationTimes = new float[3];
		private int _oscillationsOccured = 0;

		private bool _guardFound = false;
		private Vector3 _downedGuard;

		private float CurrentVelocity => Mathf.Abs(_rigidbody.linearVelocity.x);
		private float _maxRecordedForceFieldVelocity;

		#region Start and Quit Functions

		protected override void Start()
		{
			base.Start();
			
			_playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
			_animator = GetComponent<Animator>();
			_rigidbody = GetComponent<Rigidbody2D>();
			_collider = GetComponent<BoxCollider2D>();
			_floor = transform.parent.parent;
			
			_fellowGuards = _floor.GetComponentsInChildren<Guard>();
			
			_guardQuickSave = new GuardQuickSave();

			_lastKnownLocation = transform.position;

			OnHit += OnGuardHit;
			_playerController.OnDeath += OnPlayerDeath;
		}

		private void OnApplicationQuit()
		{
			OnHit -= OnGuardHit;
			_playerController.OnDeath -= OnPlayerDeath;
		}
		
		#endregion

		#region Update Functions

		private void Update()
		{
			// If the Player's dead, nothing matters.
			if (_playerDead)
			{
				return;
			}
			
			// If Guard is incapacitated, have them fall down and animate. THEN, nothing matters.
			if (State == GuardState.Dead || State == GuardState.Unconscious)
			{
				HandleIncapacitation();
				HandleFallToGround();
				return;
			}

			// Halt the Guard in Stasis and stop any other updates.
			if (_inStasis)
			{
				_animator.SetFloat("Speed", 0f);
				return;
			}
			
			// Handle Corrupted Guard Behavior and stop any other updates.
			if (State == GuardState.Corrupted)
			{
				HandleCorruptedGuard();

				return;
			}

			// If in a force field, record the fastest speed traveled and stop any other updates.
			if (_inForceField)
			{
				_maxRecordedForceFieldVelocity = Mathf.Max(CurrentVelocity, _maxRecordedForceFieldVelocity);

				return;
			}

			// If nothing else is happening to the Guard, handle normal patrol / player tracking
			PlayerAwareness();
			
			if (State == GuardState.Unaware)
			{
				HandleEnemyPatrol();
			}

			if (State == GuardState.Chasing)
			{
				ChasePlayer();
			}

			if (State == GuardState.Alerted)
			{
				SearchingForPlayer(_guardFound ? _downedGuard : _lastKnownLocation);
			}

			_animator.SetFloat("Speed", CurrentVelocity);
		}

		// Run code once to functionally kill the guard.
		private void HandleIncapacitation()
		{
			if (_incapacitationManaged)
			{
				return;
			}
			
			SuspicionPercentage = 0f;
			_collider.isTrigger = true;
			for (int childObjectIndex = 0; childObjectIndex < transform.childCount; ++childObjectIndex)
			{
				transform.GetChild(childObjectIndex).gameObject.SetActive(false);
			}
			GuardStopsHolder.SetActive(false);

			Head.localPosition = new Vector3(0f, 0.27f, 0f);

			_incapacitationManaged = true;
		}

		// Let the guard fall until a downward facing raycast hits the ground. Then stop movement.
		private void HandleFallToGround()
		{
			if (_groundReached)
			{
				return;
			}
			
			float distanceToGround = transform.localScale.y * 1.25f;
			Debug.DrawRay(transform.position, -Vector2.up * distanceToGround, Color.green);
			if (Physics2D.Raycast(transform.position, -Vector2.up, distanceToGround, WhatIsGround))
			{
				_rigidbody.gravityScale = 0f;
				_rigidbody.linearVelocity = Vector2.zero;
				_rigidbody.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;

				_groundReached = true;
			}
		}

		// Handle Corrupted Guard.
		private void HandleCorruptedGuard()
		{
			// When the Guard is Corrupted (Corrupt()), the closest guard becomes the target and is searched for immediately.
			// If it's not found the Corrupted Guard is killed.
			if (_closestGuard == null)
			{
				TakeHit(1000);
				return;
			}

			// If the Guard that's found is not a valid target, search again.
			if (_closestGuard.State == GuardState.Corrupted || _closestGuard.State == GuardState.Unconscious || _closestGuard.State == GuardState.Dead)
			{
				FindClosestGuard();
				return;
			}
			
			// Move to Attack closest guard.
			AttackGuard(_closestGuard);
		}

		// Handle Guard Patrol
		private void HandleEnemyPatrol()
		{
			if (!_idling)
			{
				_rigidbody.linearVelocity = new Vector2(GetDirection(NextStop.position) * WalkSpeed, 0f);
				return;
			}
			
			if (Time.time > _idleFinishTime)
			{
				_idling = false;
				ChangeDirectionForNextStop();
			}
		}
		
		#endregion

		#region Idle Functions

		//Takes information from the Guard Stop once one has been reached to determine the Guard's behavior.
		public void StopReached(float idleTime, Vector2 idleDirection, Transform nextStop)
		{
			if (Dead || _inStasis || _inForceField || State == GuardState.Corrupted)
			{
				return;
			}

			if (State == GuardState.Chasing || State == GuardState.Alerted)
			{
				return;
			}
			
			_idleTime = idleTime;
			_idleDirection = idleDirection;
			NextStop = nextStop;
			
			if (idleTime <= 0f)
			{
				ChangeDirectionForNextStop();
				return;
			}
			
			Idle();
		}

		// Holds the Guard forcibly when it's reached its boundary
		public void ConstrainEnemy(Vector3 constraintPosition)
		{
			if (_inForceField)
			{
				return;
			}

			transform.position = constraintPosition;
		}

		// Stop the Guard in place for a set amount of time while patrolling.
		private void Idle()
		{
			_idleFinishTime = Time.time + _idleTime;
			ChangeDirection(_idleDirection);
			_idling = true;
		}
		
		#endregion

		#region Player Perception Functions

		// Called by the Guard Vision when the Guard sees the player.
		public void CheckSeesPlayer(float playerVisibilityFactor)
		{
			if (_playerIsHiding)
			{
				return;
			}
			
			_playerVisibilityFactor = playerVisibilityFactor;
			SeesPlayer = true;
		}

		// Called by the Guard Vision when the Guard no longer sees the player.
		public void LostPlayer()
		{
			SeesPlayer = false;
		}

		// Take information from the Guard Vision and use it to determine the Guard's behavior.
		private void PlayerAwareness()
		{
			if (Dead || _inStasis || _inForceField || State == GuardState.Corrupted)
			{
				return;
			}

			// Ensure the Player Awareness updates only so often, no matter the framerate.
			if (Time.time < _nextPlayerAwarenessUpdate)
			{
				return;
			}
			
			_nextPlayerAwarenessUpdate = Time.time + _playerAwarenessUpdateTime;
			
			// Update Suspicion Percentage based on the Player's visibility and reflect it visually.
			UpdateSuspicionPercentage();
			
			SuspicionSpriteMask.localPosition = new Vector3(0f, SuspicionPercentage * 0.625f, 0f);

			// Make the Guard Unaware if the Suspicion is sufficiently low for the current Guard State.
			if (State == GuardState.Suspicious && SuspicionPercentage < 0.5f 
			    || State == GuardState.Alerted && SuspicionPercentage <= 0f)
			{
				State = GuardState.Unaware;
				HandlePlayerLost();
			}

			// Make the Guard Suspicious.
			if (State == GuardState.Unaware && SuspicionPercentage > 0.5f)
			{
				State = GuardState.Suspicious;
			}

			// Make the Guard Alerted.
			if (_guardFound || State == GuardState.Chasing && !SeesPlayer)
			{
				State = GuardState.Alerted;
			}

			// Run code once to spot the Player.
			if (SuspicionPercentage >= 1f)
			{
				if (_guardFound || State == GuardState.Chasing)
				{
					return;
				}

				HandlePlayerSpotted();

				State = GuardState.Chasing;
			}
		}

		// Update Suspicion Percentage based on Player visibility and given sees/loses player rates.
		private void UpdateSuspicionPercentage()
		{
			if (SeesPlayer && SuspicionPercentage < 1)
			{
				SuspicionPercentage += _playerVisibilityFactor * _seesPlayerRate / 100f;
			}

			if (!SeesPlayer && SuspicionPercentage > 0 && State != GuardState.Alerted)
			{
				// Lose Player at a slower rate depending on the current Guard State.
				float losesPlayerRate = State switch
				{
					GuardState.Suspicious => 2.5f,
					GuardState.Chasing => 1f,
					GuardState.Alerted => 1f,
					_ => _losesPlayerRate
				};

				SuspicionPercentage -= losesPlayerRate / 100f;
			}

			SuspicionPercentage = Mathf.Clamp(SuspicionPercentage, 0, 1);
		}

		// Handle Player spotted.
		private void HandlePlayerSpotted()
		{
			if (_canSpot)
			{
				// Mark Player Spotted for end-of-level recap screen.
				MoralitySystem.Instance.TimesSpotted++;
				AudioManager.Instance.PlaySound("Guard Surprise");
				_animator.SetTrigger("Surprised");
			}

			_canSpot = false;

			// Alert all other Guards on the floor.
			for (int fellowGuardIndex = 0; fellowGuardIndex < _fellowGuards.Length; ++fellowGuardIndex)
			{
				if (_fellowGuards[fellowGuardIndex] == this)
				{
					continue;
				}
							
				_fellowGuards[fellowGuardIndex].PlayerSeenByOther();
			}
		}

		// Handle Player Lost.
		private void HandlePlayerLost()
		{
			_canSpot = true;
		}
		
		#endregion

		#region Alerted Functions

		// Called by the Guard Vision when the player is right in front of the Guard.
		public void SetPlayerInMeleeRange()
		{
			_playerInMeleeRange = true;

			if (Dead || _inStasis || _inForceField || State == GuardState.Corrupted)
			{
				return;
			}
			
			if (!_playerController.tag.Equals("Player"))
			{
				return;
			}
			
			SuspicionPercentage = 1f;
		}
		
		// Called by the Guard Vision when the player is no longer right in front of the Guard.
		public void SetPlayerOutOfMeleeRange()
		{
			_playerInMeleeRange = false;
		}

		// Called when another guard on the same floor has seen the player.
		public void PlayerSeenByOther()
		{
			if (Dead || _inStasis || _inForceField || State == GuardState.Unconscious || State == GuardState.Corrupted)
			{
				return;
			}
			
			SuspicionPercentage = 1f;
			_lastKnownLocation = _playerController.transform.position;
			_lastSeenTime = Time.time;
			FlipToFace(_playerController.transform.position);
			State = GuardState.Chasing;
			AudioManager.Instance.PlaySound("Guard Surprise");
		}

		// Called when the guard finds an incapacitated compatriot.
		public void FoundGuard(Guard fellowGuard)
		{
			if (Dead || _inStasis || _inForceField || State == GuardState.Chasing || State == GuardState.Corrupted)
			{
				return;
			}
			
			SuspicionPercentage = 1f;
			
			FlipToFace(fellowGuard.transform.position);
			
			_guardFound = true;
			_downedGuard = fellowGuard.transform.position;
			
			AudioManager.Instance.PlaySound("Guard Surprise");
			_animator.SetTrigger("Surprised");
		}

		// Chase the player around and attack them while the player is in sight.
		private void ChasePlayer()
		{
			if (Dead || _inStasis || _inForceField || State == GuardState.Corrupted)
			{
				return;
			}
			
			_idling = false;
			if (SeesPlayer)
			{
				_lastKnownLocation = _playerController.transform.position;
				_lastSeenTime = Time.time;
			}

			if (!_playerInMeleeRange)
			{
				_rigidbody.linearVelocity = new Vector2(GetDirection(_playerController.transform.position) * RunSpeed, 0f);
				return;
			}

			if (_timeBetweenAttack > 0f)
			{
				_timeBetweenAttack -= Time.deltaTime;
				return;
			}
			
			Collider2D[] enemiesToDamage = Physics2D.OverlapCircleAll(AttackPos.position, AttackRange, WhatAreEnemies);
			enemiesToDamage[0].GetComponent<LivingEntity>().TakeHit(Damage);
			_timeBetweenAttack = StartTimeBetweenAttack;
			AudioManager.Instance.PlaySound("Swipe");
			_animator.SetTrigger("Attacking");
		}

		// Search for the player. Works while the guard is aware that the player is around, but can't see them currently.
		private void SearchingForPlayer(Vector3 searchArea)
		{
			// Running toward Search Area
			if (_guardFound && Vector2.Distance(transform.position, searchArea) > 0.1f || Time.time < _lastSeenTime + _giveUpTime)
			{
				_rigidbody.linearVelocity = new Vector2(GetDirection(searchArea) * RunSpeed, 0f);
				_alertedEndTime = Time.time + _alertedTime;
				return;
			}
			
			// Conducting Search / Displaying Confusion
			if (_alertedEndTime >= Time.time)
			{
				if (!_searchUnderway)
				{
					_guardFound = false;
					_searchUnderway = true;
					for (int oscillationIndex = 0; oscillationIndex < NumberOfOscillations; oscillationIndex++)
					{
						_oscillationTimes[oscillationIndex] = Time.time + _alertedTime / NumberOfOscillations * (oscillationIndex + 1);
					}

					return;
				}
				
				// Confusion period where the Guard stays in place but looks back and forth.
				// If the guard doesn't see the player after so many seconds, it returns to patrol.
				if (!_confused)
				{
					_animator.SetTrigger("Confused");
					_confused = true;
				}

				if (_oscillationsOccured < _oscillationTimes.Length && Time.time > _oscillationTimes[_oscillationsOccured])
				{
					float newDirection = transform.localScale.x * -1;
					ChangeDirection(newDirection);
					_oscillationsOccured++;
				}

				return;
			}
			
			// Return to regular patrol but remain Suspicious.
			_searchUnderway = false;
			_oscillationsOccured = 0;
			State = GuardState.Suspicious;
			
			if (_collider.IsTouching(NextStop.GetComponent<BoxCollider2D>()))
			{
				NextStop.GetComponent<GuardStop>().ForceUpdate();
				return;
			}
			
			ChangeDirectionForNextStop();
		}

		// Seek and attack the closest Guard while corrupted.
		private void AttackGuard(Guard fellowGuard)
		{
			if (State != GuardState.Corrupted || Dead || _inStasis || _inForceField)
			{
				return;
			}

			bool isTouchingFellowGuard = GuardVisionCircle.IsTouching(fellowGuard.Collider);

			if (!isTouchingFellowGuard)
			{
				//transform.position = new Vector2 (Vector2.MoveTowards (transform.position, fellowGuard.position, runSpeed * Time.fixedDeltaTime).x, transform.position.y);
				_rigidbody.linearVelocity = new Vector2(GetDirection(fellowGuard.transform.position) * RunSpeed, 0f);
				FlipToFace(_closestGuard.transform.position);

				return;
			}

			if (_timeBetweenAttack > 0)
			{
				_timeBetweenAttack -= Time.deltaTime;
				return;
			}
			
			Collider2D[] friendsToDamage = Physics2D.OverlapCircleAll(AttackPos.position, AttackRange, WhatAreFriends);
			for (int fellowGuardIndex = 0; fellowGuardIndex < friendsToDamage.Length; ++fellowGuardIndex)
			{
				Collider2D friendToDamage = friendsToDamage[fellowGuardIndex];
				
				if (friendToDamage.transform == transform || !friendToDamage.name.Equals("Guard Actual"))
				{
					continue;
				}
				
				friendToDamage.GetComponent<Guard>().TakeHit(1000);
			}

			AudioManager.Instance.PlaySound("Swipe");
			_animator.SetTrigger("Attacking");
			_timeBetweenAttack = StartTimeBetweenAttack;
		}

		//Draws the red circle gizmo to show the Guard's attack range.
		/*private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(AttackPos.position, AttackRange);
		}*/
		
		#endregion

		#region Player Ability Functions

		// Called if the Guard is in a Stasis Bubble.
		public void InStasis(bool inStasis)
		{
			_inStasis = inStasis;
			_animator.speed = inStasis ? 0f : 1f;
		}

		// Called when the Guard is corrupted by the Traitor ability.
		public void Corrupt()
		{
			FindClosestGuard();
			
			_animator.SetBool("Corrupted", true);
			_animator.SetTrigger("Cursed");
			
			State = GuardState.Corrupted;
			SuspicionSpriteMask.parent.gameObject.SetActive(false);
		}

		// Find the closest Guard to attack.
		private void FindClosestGuard()
		{
			_closestGuard = null;
			float closestGuardDistance = Mathf.Infinity;
			
			if (_fellowGuards.Length == 0)
			{
				return;
			}
			
			for (int fellowGuardIndex = 0; fellowGuardIndex < _fellowGuards.Length; ++fellowGuardIndex)
			{
				Guard fellowGuard = _fellowGuards[fellowGuardIndex];
				
				float currentDistance = Vector2.Distance(transform.position, fellowGuard.transform.position);
				GuardState fellowGuardState = fellowGuard.State;

				if (fellowGuard == this)
				{
					continue;
				}

				if (currentDistance > closestGuardDistance)
				{
					continue;
				}

				if (fellowGuardState == GuardState.Corrupted || fellowGuardState == GuardState.Unconscious || fellowGuardState == GuardState.Dead)
				{
					continue;
				}
				
				_closestGuard = fellowGuard;
				closestGuardDistance = currentDistance;
			}
		}
		
		#endregion

		#region Event Functions

		// Set the Guard State to Alerted when it's been hit.
		private void OnGuardHit(float timeHit, float startingHealth, float currentHealth)
		{
			if (Dead || _inStasis || _inForceField || State == GuardState.Corrupted || currentHealth <= 0f)
			{
				return;
			}
			
			AudioManager.Instance.PlaySound("Guard Hurt");
			SuspicionPercentage = 1f;
			FlipToFace(_playerController.transform.position);
			State = GuardState.Alerted;
		}

		// Handle unique assets on Guard Hit.
		public override void TakeHit(float damage)
		{
			base.TakeHit(damage);
		}

		// Handle unique assets on Guard Healed.
		public override void Heal(float heals)
		{
			base.Heal(heals);
		}

		// Handle unique assets on Guard Death.
		protected override void Die()
		{
			base.Die();
			
			if (State == GuardState.Corrupted)
			{
				AudioManager.Instance.PlaySound("Corrupted Guard Death");
			}

			_animator.speed = 1f;
			_animator.SetTrigger("Killed");
			_animator.SetBool("Incapacitated", true);
			
			State = GuardState.Dead;
			
			MoralitySystem.Instance.EnemiesKilled++;
		}

		// Called when the guard is knocked unconscious.
		public void OnGuardUnconscious()
		{
			_animator.speed = 1f;
			_animator.SetTrigger("KnockedOut");
			_animator.SetBool("Incapacitated", true);
			
			State = GuardState.Unconscious;
		}

		// Called when the player dies.
		private void OnPlayerDeath()
		{
			_playerDead = true;
			State = GuardState.Unaware;
			SuspicionPercentage = 0f;
		}
		
		#endregion

		#region Collision Functions

		// Handle the Guard hitting a non-trigger collider.
		private void OnCollisionEnter2D(Collision2D collision)
		{
			// If in Force Field, check to see if what was hit was a Wall or Platform.
			if (_inForceField)
			{
				int layerIndex = collision.gameObject.layer;
				int mask = WhatIsGround | WhatIsWall;
				
				// Use bitwise math to see if the collider is assigned to the Ground or Wall layers.
				if ((mask & (1 << layerIndex)) == 0)
				{
					return;
				}
				
				// Kill the guard if going 30 units per second or faster when a stop is hit.
				if (_maxRecordedForceFieldVelocity >= 30f)
				{
					TakeHit(1000f);
					return;
				}
				
				// Knock out the guard if going 10 units per second or faster when a stop is hit.
				if (_maxRecordedForceFieldVelocity >= 10f)
				{
					OnGuardUnconscious();
					return;
				}

				return;
			}

			if (Dead || _inStasis || State == GuardState.Unconscious || State == GuardState.Corrupted)
			{
				return;
			}
			
			// Set to Alerted if the Guard runs into the Player.
			if (collision.collider.tag.Equals("Player") && !_playerController.InShadowSink)
			{
				SuspicionPercentage = 1f;
				State = GuardState.Alerted;
				_lastKnownLocation = _playerController.transform.position;
				_lastSeenTime = Time.time;
				FlipToFace(_playerController.transform.position);
			}
		}
		
		// Handle the Guard hitting a trigger collider.
		private void OnTriggerEnter2D(Collider2D otherCollider)
		{
			if (otherCollider.name.Contains("Extreme Force"))
			{
				_inForceField = true;
			}
		}
		
		#endregion
		
		#region Helper Functions

		//Force-Updates the direction of the Guard.
		private void ChangeDirection(float direction)
		{
			float directionX = direction >= 0f ? 1f : -1f;
			SetLocalScale(directionX);
		}
		private void ChangeDirection(Vector2 direction)
		{
			float directionX = direction.x >= 0f ? 1f : -1f;
			SetLocalScale(directionX);
		}

		//Flips the direction of the Guard while patrolling to look at the next stop.
		private void ChangeDirectionForNextStop() => FlipToFace(NextStop.position);

		//Flips the direction of the Guard to face a given target.
		private void FlipToFace(Vector2 targetPosition)
		{
			float newDirection = GetDirection(targetPosition);
			SetLocalScale(newDirection);
		}

		//Returns a 1 or a -1 depending on where the target is in respect to the object on the x-axis.
		private float GetDirection(Vector2 targetPosition) => targetPosition.x - transform.position.x >= 0f ? 1f : -1f;

		//Sets the local scale. Requires the direction to be normalized (either 1, or -1).
		private void SetLocalScale(float direction)
		{
			float localScale = Mathf.Abs(transform.localScale.x);
			transform.localScale = new Vector2(localScale * direction, transform.localScale.y);
		}
		
		#endregion

		#region Quick Save Functions

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
			transform.localScale = new Vector3(_guardQuickSave.Direction, transform.localScale.y, transform.localScale.z);
			NextStop = _guardQuickSave.NextStop;
			Health = _guardQuickSave.Health;
			State = _guardQuickSave.State;
			_playerDead = false;
			_animator.SetBool("Dead", false);
			SuspicionPercentage = _guardQuickSave.SuspicionPercentage;
			if (Dead && State != GuardState.Dead && State != GuardState.Unconscious)
			{
				_collider.isTrigger = false;
				for (int childObjectIndex = 0; childObjectIndex < transform.childCount; ++childObjectIndex)
				{
					transform.GetChild(childObjectIndex).gameObject.SetActive(true);
				}

				GuardStopsHolder.SetActive(true);
				_rigidbody.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
				
				Dead = false;
				_incapacitationManaged = false;
				_groundReached = false;
			}
		}
		
		#endregion
	}
}
