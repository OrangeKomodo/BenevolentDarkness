using DamageSystem;
using GameManager;
using Player;
using UnityEngine;

namespace AI.Sentry
{
	public class Sentry : LivingEntity
	{
		public enum SentryState
		{
			Unaware,
			Chasing,
			Disabled
		}
		
		struct SentryQuickSave
		{
			public Vector2 Position;
			public Transform NextStop;
			public float Health;
			public SentryState State;
			public float SuspicionPercentage;
		}

		public SentryState State = SentryState.Unaware;
		public GameObject SentryStopsHolder;
		public Transform NextStop;
		public float FlySpeed;
		public float ChaseSpeed;
		[Range(0, 1)]
		public float SuspicionPercentage = 0f;
		public bool SeesPlayer = false;
		public LayerMask WhatAreEnemies;
		public LayerMask WhatIsGround;
		public LayerMask WhatIsWall;
		public Transform SuspicionSpriteMask;
		public Sprite UnawareSprite;
		public Sprite ChasingSprite;
		public Sprite DisabledSprite;
		
		public float SeesPlayerRate = 5f;
		public float LosesPlayerRate = 1f;
		
		private float _playerAwarenessUpdateTime = 0.02f;
		private float _nextPlayerAwarenessUpdate;

		private Guard.Guard[] _fellowGuards;

		private PlayerController _playerController;
		private Rigidbody2D _rigidbody;
		private Collider2D _collider;
		private SpriteRenderer _spriteRenderer;
		private Transform _floor;
		private SentryQuickSave _sentryQuickSave;

		private bool _idling;
		private bool _playerDead;
		private bool _inStasis = false;
		private bool _inForceField = false;
		private bool _incapacitationManaged = false;
		private bool _groundReached = false;

		private float _idleTime;
		private float _idleFinishTime;

		private bool _playerIsHiding = false;
		private float _playerVisibilityFactor = 0f;

		private float CurrentVelocity => Mathf.Abs(_rigidbody.linearVelocity.x);
		private float _maxRecordedForceFieldVelocity;

		#region Start and Quit Functions

		protected override void Start()
		{
			base.Start();
			
			_playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
			_rigidbody = gameObject.GetComponent<Rigidbody2D>();
			_collider = gameObject.GetComponent<BoxCollider2D>();
			_spriteRenderer = GetComponent<SpriteRenderer>();
			_floor = transform.parent.parent;
		
			_fellowGuards = _floor.GetComponentsInChildren<Guard.Guard>();

			_sentryQuickSave = new SentryQuickSave();

			_playerController.GetComponent<PlayerController>().OnDeath += OnPlayerDeath;
		}

		private void OnApplicationQuit()
		{
			_playerController.GetComponent<PlayerController>().OnDeath -= OnPlayerDeath;
		}
		
		#endregion

		#region Update Functions

		private void Update()
		{
			if (_playerDead)
			{
				return;
			}
			
			// If in a force field, record the fastest speed traveled and stop any other updates.
			if (_inForceField)
			{
				_maxRecordedForceFieldVelocity = Mathf.Max(CurrentVelocity, _maxRecordedForceFieldVelocity);

				return;
			}
			
			if (_inStasis)
			{
				return;
			}
			
			if (State == SentryState.Disabled)
			{
				HandleIncapacitation();
				HandleFallToGround();

				return;
			}

			PlayerAwareness();
			
			if (State == SentryState.Unaware)
			{
				HandleEnemyPatrol();
			}
			
			if (State == SentryState.Chasing)
			{
				_spriteRenderer.sprite = ChasingSprite;
				ChasePlayer();
			}
		}

		// Run code once to functionally kill the sentry.
		private void HandleIncapacitation()
		{
			if (_incapacitationManaged)
			{
				return;
			}
			
			_rigidbody.linearVelocity = Vector2.zero;
			SuspicionPercentage = 0f;
			_spriteRenderer.sprite = DisabledSprite;
			_collider.isTrigger = true;
			_rigidbody.gravityScale = 1f;
			for (int childObjectIndex = 0; childObjectIndex < transform.childCount; childObjectIndex++)
			{
				transform.GetChild(childObjectIndex).gameObject.SetActive(false);
			}
			SentryStopsHolder.SetActive(false);
			
			_incapacitationManaged = true;
		}

		// Let the guard fall until a downward facing raycast hits the ground. Then stop movement.
		private void HandleFallToGround()
		{
			if (_groundReached)
			{
				return;
			}
			
			float distanceToGround = transform.localScale.y / 2f + .3f;
			Debug.DrawRay(transform.position, -Vector2.up * distanceToGround, Color.green);
			if (Physics2D.Raycast(transform.position, -Vector2.up, distanceToGround, WhatIsGround))
			{
				_rigidbody.gravityScale = 0f;
				_rigidbody.linearVelocity = Vector2.zero;
				_rigidbody.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;

				_groundReached = true;
			}
		}

		// Handle Sentry Hover
		private void HandleEnemyPatrol()
		{
			if (!_idling)
			{
				_spriteRenderer.sprite = UnawareSprite;
				float flightDelta = FlySpeed * Time.deltaTime;
				float newXPosition = Vector2.MoveTowards(transform.position, NextStop.position, flightDelta).x;
				transform.position = new Vector2(newXPosition, transform.position.y);

				return;
			}
				
			if (Time.time > _idleFinishTime)
			{
				_idling = false;
			}
		}
		
		#endregion

		#region Idle Functions

		// Take information from the Sentry Stop once one has been reached to determine the Sentry's behavior.
		public void StopReached(float idleTime, Transform nextStop)
		{
			if (Dead || _inStasis || _inForceField)
			{
				return;
			}

			if (State == SentryState.Chasing)
			{
				return;
			}
			
			_idleTime = idleTime;
			NextStop = nextStop;
			
			if (idleTime <= 0f)
			{
				return;
			}
			
			Idle();
		}

		// Holds the Sentry forcibly when it's reached its boundary
		public void ConstrainEnemy(Vector3 constraintPosition)
		{
			if (_inForceField)
			{
				return;
			}

			transform.position = constraintPosition;
		}
		
		//Stops the Sentry in place for a set amount of time while patrolling.
		private void Idle()
		{
			_idleFinishTime = Time.time + _idleTime;
			_idling = true;
		}

		#endregion

		#region Player Perception Functions

		// Called by the Sentry Vision when the Sentry sees the player.
		public void CheckSeesPlayer(float playerVisibilityFactor)
		{
			if (_playerIsHiding)
			{
				return;
			}
			
			_playerVisibilityFactor = playerVisibilityFactor;
			SeesPlayer = true;
		}

		// Called by the Sentry Vision when the Sentry no longer sees the player.
		public void LostPlayer()
		{
			SeesPlayer = false;
		}

		//Takes information from the Sentry Vision and uses it to determine the Sentry's behavior.
		private void PlayerAwareness()
		{
			if (_playerDead || _inStasis)
			{
				return;
			}

			// Ensure the Player Awareness updates only so often, no matter the framerate.
			if (Time.time < _nextPlayerAwarenessUpdate)
			{
				return;
			}
			
			_nextPlayerAwarenessUpdate = Time.time + _playerAwarenessUpdateTime;

			UpdateSuspicionPercentage();

			SuspicionSpriteMask.localPosition = new Vector3(0f, SuspicionPercentage * 0.625f, 0f);

			if (State == SentryState.Chasing && SuspicionPercentage < 0.5f)
			{
				AudioManager.Instance.StopSound("Alarm");
				State = SentryState.Unaware;
				HandlePlayerLost();
			}

			if (State == SentryState.Unaware && SuspicionPercentage >= 1f)
			{
				if (State != SentryState.Chasing)
				{
					AudioManager.Instance.PlaySound("Alarm");
					HandlePlayerSpotted();
				}

				State = SentryState.Chasing;
			}
		}

		// Update Suspicion Percentage based on Player visibility and given sees/loses player rates.
		private void UpdateSuspicionPercentage()
		{
			if (SeesPlayer && SuspicionPercentage < 1)
			{
				SuspicionPercentage += _playerVisibilityFactor * SeesPlayerRate / 100f;
			}

			if (!SeesPlayer && SuspicionPercentage > 0)
			{
				// Lose Player at a slower rate depending on the current Sentry State.
				float losesPlayerRate = State switch
				{
					SentryState.Chasing => 1f,
					_ => LosesPlayerRate
				};

				SuspicionPercentage -= losesPlayerRate / 100f;
			}

			SuspicionPercentage = Mathf.Clamp(SuspicionPercentage, 0, 1);
		}

		// Handle Player spotted.
		private void HandlePlayerSpotted()
		{
			// Alert all other Guards on the floor.
			for (int fellowGuardIndex = 0; fellowGuardIndex < _fellowGuards.Length; ++fellowGuardIndex)
			{
				_fellowGuards[fellowGuardIndex].PlayerSeenByOther();
			}
		}

		// Handle Player Lost.
		private void HandlePlayerLost()
		{
			if (_collider.IsTouching(NextStop.GetComponent<BoxCollider2D>()))
			{
				NextStop.GetComponent<SentryStop>().ForceUpdate();
			}
		}
		
		#endregion
		
		#region Alerted Functions

		//Chases the player around.
		private void ChasePlayer()
		{
			_idling = false;
			Vector2 playerPosition = _playerController.transform.position;
			float flightDelta = ChaseSpeed * Time.deltaTime;
			float newXPosition = Vector2.MoveTowards(transform.position, playerPosition, flightDelta).x;
			transform.position = new Vector2(newXPosition, transform.position.y);
		}
		
		#endregion
		
		#region Player Ability Functions

		// Called when the player has entered a natural hiding place to force the Sentry to lose them.
		public void PlayerHiding(bool playerIsHiding)
		{
			_playerIsHiding = playerIsHiding;
			LostPlayer();
		}

		// Called if the Sentry is in a Stasis Bubble.
		public void InStasis(bool inStasis)
		{
			_inStasis = inStasis;
		}
		
		#endregion

		#region Event Functions

		// Handle unique assets on Sentry Hit.
		public override void TakeHit(float damage)
		{
			base.TakeHit(damage);
		}

		// Handle unique assets on Sentry Healed.
		public override void Heal(float heals)
		{
			// Nothing
		}

		// Handle unique assets on Sentry Death.
		protected override void Die()
		{
			base.Die();
			
			State = SentryState.Disabled;
		}

		//Calls when the player dies.
		private void OnPlayerDeath()
		{
			_playerDead = true;
			State = SentryState.Unaware;
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
				
				// Disable the sentry if going 10 units per second or faster when a stop is hit.
				if (_maxRecordedForceFieldVelocity >= 10f)
				{
					TakeHit(1000f);
					return;
				}

				return;
			}
			
			// Maybe put something here in case something else hits it
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
		
		#region Quick Save Functions

		public void QuickSave()
		{
			_sentryQuickSave.Position = transform.position;
			_sentryQuickSave.NextStop = NextStop;
			_sentryQuickSave.Health = Health;
			_sentryQuickSave.State = State;
			_sentryQuickSave.SuspicionPercentage = SuspicionPercentage;
		}

		public void QuickLoad()
		{
			transform.position = _sentryQuickSave.Position;
			NextStop = _sentryQuickSave.NextStop;
			Health = _sentryQuickSave.Health;
			State = _sentryQuickSave.State;
			_playerDead = false;
			SuspicionPercentage = _sentryQuickSave.SuspicionPercentage;
			if (Dead && State != SentryState.Disabled)
			{
				_spriteRenderer.color = Color.white;
				_collider.isTrigger = false;
				for (int i = 0; i < transform.childCount; i++)
					transform.GetChild(i).gameObject.SetActive(true);
				SentryStopsHolder.SetActive(true);
				Dead = false;
			}
		}
		
		#endregion
	}
}
