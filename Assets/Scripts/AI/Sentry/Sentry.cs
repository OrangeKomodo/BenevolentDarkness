using DamageSystem;
using GameManager;
using Player;
using UnityEngine;

namespace AI.Sentry
{
	
	[RequireComponent(typeof(Collider2D))]
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
		[Range(0, 1)]
		public float SuspicionPercentage = 0f;
		public bool SeesPlayer = false;
		public LayerMask WhatAreEnemies;
		public LayerMask WhatIsGround;
		public Transform SuspicionSpriteMask;
		public Sprite UnawareSprite;
		public Sprite ChasingSprite;
		public Sprite DisabledSprite;
		
		[SerializeField] private float SeesPlayerRate = 20f;
		[SerializeField] private float LosesPlayerRate = 1f;

		private GameObject _player;
		private Rigidbody2D _rigidbody;
		private Collider2D _collider;
		private SpriteRenderer _spriteRenderer;
		private Transform _floor;
		private SentryQuickSave _sentryQuickSave;

		private bool _idling;
		private bool _playerDead;
		private bool _inStasis;
		private bool _deathManaged = false;

		private float _idleTime;
		private float _idleFinishTime;

		private bool _playerIsHiding = false;
		private float _playerVisibilityFactor = 0f;

		private float _maxVelocity;

		public Sentry(Collider2D collider)
		{
			_collider = collider;
		}

		protected override void Start()
		{
			base.Start();
			_player = GameObject.FindGameObjectWithTag("Player");
			_rigidbody = gameObject.GetComponent<Rigidbody2D>();
			_collider = gameObject.GetComponent<BoxCollider2D>();
			_spriteRenderer = GetComponent<SpriteRenderer>();
			_floor = transform.parent.parent;

			_sentryQuickSave = new SentryQuickSave();

			_player.GetComponent<PlayerController>().OnDeath += OnPlayerDeath;
		}

		private void OnApplicationQuit()
		{
			_player.GetComponent<PlayerController>().OnDeath -= OnPlayerDeath;
		}

		private void FixedUpdate()
		{
			if (!_playerDead && !_inStasis)
			{
				if (State == SentryState.Disabled)
				{
					if (!_deathManaged)
					{
						_rigidbody.linearVelocity = Vector2.zero;
						SuspicionPercentage = 0f;
						_spriteRenderer.sprite = DisabledSprite;
						_collider.isTrigger = true;
						_rigidbody.gravityScale = 1f;
						for (int i = 0; i < transform.childCount; i++)
						{
							transform.GetChild(i).gameObject.SetActive(false);
						}

						SentryStopsHolder.SetActive(false);

						_deathManaged = true;
					}

					if (_rigidbody.gravityScale > 0f)
					{
						Debug.DrawRay(transform.position, -Vector2.up * (transform.localScale.y / 2f + .3f),
							Color.green);
						if (Physics2D.Raycast(transform.position, -Vector2.up, transform.localScale.y / 2f + .3f,
							    WhatIsGround))
						{
							_rigidbody.gravityScale = 0f;
							_rigidbody.linearVelocity = Vector2.zero;
						}
					}
				}
				else
				{
					if (State == SentryState.Unaware)
					{
						if (!_idling)
						{
							_spriteRenderer.sprite = UnawareSprite;
							transform.position =
								new Vector2(
									Vector2.MoveTowards(transform.position, NextStop.position,
										FlySpeed * Time.fixedDeltaTime).x, transform.position.y);
						}
						else if (Time.time > _idleFinishTime)
						{
							_idling = false;
						}
					}

					PlayerAwareness();

					if (State == SentryState.Chasing)
					{
						_spriteRenderer.sprite = ChasingSprite;
						PlayerSeen();
					}
				}

				if (Mathf.Abs(_rigidbody.linearVelocity.magnitude) > _maxVelocity)
				{
					_maxVelocity = Mathf.Abs(_rigidbody.linearVelocity.magnitude);
				}
			}
		}

		//Is called by the Sentry Vision when the Sentry sees the player.
		public void CheckSeesPlayer(float playerVisibilityFactor)
		{
			if (playerVisibilityFactor > 0 && !_playerIsHiding)
			{
				_playerVisibilityFactor = playerVisibilityFactor;
				SeesPlayer = true;
			}
		}

		//Is called by the Sentry Vision when the Sentry no longer sees the player.
		public void LostPlayer()
		{
			SeesPlayer = false;
		}

		//Is called when the player has entered a natural hiding place to force the Sentry to lose them.
		public void PlayerHiding(bool playerIsHiding)
		{
			_playerIsHiding = playerIsHiding;
			LostPlayer();
		}

		//Is called if the Sentry is in a Stasis Bubble.
		public void InStasis(bool inStasis)
		{
			_inStasis = inStasis;
		}

		//Takes information from the Sentry Stop once one has been reached to determine the Sentry's behavior.
		public void StopReached(float idleTime, Transform nextStop)
		{
			if (!_playerDead && !_inStasis)
			{
				if (State != SentryState.Chasing)
				{
					_idleTime = idleTime;
					NextStop = nextStop;
					if (idleTime > 0f)
					{
						Idle();
					}
				}
			}
		}

		public override void TakeHit(float damage)
		{
			base.TakeHit(damage);
		}

		public override void Heal(float heals)
		{
			// Nothing
		}

		//Sets the Sentry and the Sentry Stops inactive upon its death.
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

		//Stops the Sentry in place for a set amount of time while patrolling.
		private void Idle()
		{
			_idleFinishTime = Time.time + _idleTime;
			_idling = true;
		}

		//Takes information from the Sentry Vision and uses it to determine the Sentry's behavior.
		private void PlayerAwareness()
		{
			if (!_playerDead && !_inStasis)
			{
				if (SeesPlayer && SuspicionPercentage < 1)
				{
					SuspicionPercentage += _playerVisibilityFactor * SeesPlayerRate / 100f;
				}
				if (!SeesPlayer && SuspicionPercentage > 0)
				{
					SuspicionPercentage -= LosesPlayerRate / 100f;
				}

				SuspicionPercentage = Mathf.Clamp(SuspicionPercentage, 0, 1);

				SuspicionSpriteMask.localPosition = new Vector3(0f, SuspicionPercentage * 0.625f, 0f);

				if (State == SentryState.Chasing && SuspicionPercentage < 1f)
				{
					AudioManager.Instance.StopSound("Alarm");
					State = SentryState.Unaware;
					if (transform.GetComponent<BoxCollider2D>().IsTouching(NextStop.GetComponent<BoxCollider2D>()))
					{
						NextStop.GetComponent<SentryStop>().ForceUpdate();
					}
				}

				if (SuspicionPercentage == 1f)
				{
					if (State != SentryState.Chasing)
					{
						AudioManager.Instance.PlaySound("Alarm");
						for (int i = 0; i < _floor.childCount; i++)
						{
							if (_floor.GetChild(i).GetChild(0).name.Contains("Guard"))
							{
								_floor.GetChild(i).GetChild(0).GetComponent<Guard.Guard>().PlayerSeenByOther();
							}
						}
					}

					State = SentryState.Chasing;
				}
			}
		}

		//Chases the player around.
		private void PlayerSeen()
		{
			_idling = false;
			transform.position =
				new Vector2(
					Vector2.MoveTowards(transform.position, _player.transform.position,
							FlySpeed * Time.fixedDeltaTime * 10)
						.x, transform.position.y);
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			if (collision.collider.gameObject.layer == 13)
			{
				if (_maxVelocity >= 30f)
				{
					TakeHit(1000f);
				}
			}
		}

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
	}
}
