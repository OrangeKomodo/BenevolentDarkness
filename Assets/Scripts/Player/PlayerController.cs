using System;
using System.Collections.Generic;
using AI.Guard;
using AI.Sentry;
using DamageSystem;
using GameManager;
using Interfaces;
using Items;
using Player.Components;
using Spells;
using UI;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets._2D;

namespace Player
{
	public class PlayerController : LivingEntity, IDetectable
	{
		#region Structs

		[Serializable]
		private struct PlayerQuickSave
		{
			public Vector2 Position;
			public float Direction;
			public float Health;
			public int Mana;
			public bool Frozen;
			public SpellCasting.SpellNames Spell;
			public List<string> Inventory;
		}

		#endregion

		#region Components

		private PlayerVisibilityManager _visibilityManager;
		private PlayerHealthManager _healthManager;
		private PlayerInteractionHandler _interactionHandler;

		#endregion

		#region Fields

		public SpellCasting Spellcaster;
		public CameraController CameraController;
		public PlatformerCharacter2D PlatformerCharacter;
		public SpriteRenderer PlayerSprite;
		public Rigidbody2D Rigidbody;
		public Animator Animator;
		public Image HealthBar;
		public PlatformEffector2D[] AffectedPlatforms;
		public Transform EnemiesHolder;
		public Transform Canvas;

		[HideInInspector] public List<string> Inventory = new List<string>();

		public LayerMask[] HidingPlaceLayerMasks;
		
		private PlayerQuickSave _playerQuickSave;
		
		private Guard[] _guards;
		private Sentry[] _sentries;

		public bool InShadowSink = false;
		public bool DisguisedAsGuard = false;
		public bool CanAttack = true;
		public bool IsSeen = false;

		private bool _falling = false;
		private float _timeOfDeath = 0f;
		private bool _deathScreenLoaded = false;

		// AI - Cached animator parameter names
		private static readonly int AnimUse = Animator.StringToHash("Use");
		private static readonly int AnimAttacking = Animator.StringToHash("Attacking");
		private static readonly int AnimAttackType = Animator.StringToHash("Attack Type");
		private static readonly int AnimUnderTable = Animator.StringToHash("Under Table");
		private static readonly int AnimDisguised = Animator.StringToHash("Disguised");
		private static readonly int AnimMimicUsed = Animator.StringToHash("Mimic Used");
		private static readonly int AnimDies = Animator.StringToHash("Dies");
		private static readonly int AnimDead = Animator.StringToHash("Dead");

		#endregion

		#region Properties

		// AI - IDetectable implementation
		public float VisibilityFactor => _visibilityManager != null ? _visibilityManager.VisibilityFactor : 0.1f;
		public bool IsHidden => _visibilityManager != null && _visibilityManager.IsHidden;

		// AI - Expose component properties for backward compatibility
		public float RawVisibilityFactor => _visibilityManager != null ? _visibilityManager.RawVisibilityFactor : 0.1f;
		public float LightTotals => _visibilityManager != null ? _visibilityManager.LightTotals : 0f;

		#endregion

		#region Initialization and Cleanup Functions

		protected override void Start()
		{
			base.Start();

			InitializeComponents();
			InitializeQuickSave();
			CacheEnemyReferences();
			SetupCursor();
			SubscribeToEvents();
		}

		// AI - Get or add required components.
		private void InitializeComponents()
		{
			_visibilityManager = GetComponent<PlayerVisibilityManager>();
			if (_visibilityManager == null)
			{
				_visibilityManager = gameObject.AddComponent<PlayerVisibilityManager>();
			}

			_healthManager = GetComponent<PlayerHealthManager>();
			if (_healthManager == null)
			{
				_healthManager = gameObject.AddComponent<PlayerHealthManager>();
			}

			_interactionHandler = GetComponent<PlayerInteractionHandler>();
			if (_interactionHandler == null)
			{
				_interactionHandler = gameObject.AddComponent<PlayerInteractionHandler>();
			}

			// AI - Start health regeneration
			_healthManager.StartRegeneration();
		}

		// AI - Initialize quick save data structure.
		private void InitializeQuickSave()
		{
			_playerQuickSave = new PlayerQuickSave
			{
				Inventory = new List<string>()
			};
		}

		// AI - Cache references to all enemies in the scene.
		private void CacheEnemyReferences()
		{
			_guards = EnemiesHolder.GetComponentsInChildren<Guard>();
			_sentries = EnemiesHolder.GetComponentsInChildren<Sentry>();
		}

		// AI - Set up cursor visibility.
		private void SetupCursor()
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		// AI - Subscribe to damage events.
		private void SubscribeToEvents()
		{
			OnHit += OnPlayerHit;
		}

		// AI - Unsubscribe from events.
		private void OnApplicationQuit()
		{
			OnHit -= OnPlayerHit;
		}

		#endregion

		#region Update Functions

		private void Update()
		{
			// AI - If player is dead, handle death screen loading and skip other updates.
			if (Dead)
			{
				HandleDeathScreen();
				return;
			}
			
			UpdateVisibility();
			_interactionHandler.ProcessInteraction(this);
			HandlePlatformDropThrough();
			CheckFallingDeath();
			UpdatePlayerSeenStatus();
		}

		// AI - Display the failure menu after a delay when the player dies.
		private void HandleDeathScreen()
		{
			if (Time.time < _timeOfDeath + 2f || _deathScreenLoaded)
			{
				return;
			}

			if (!InputManager.UsingGamepad)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}

			MoralitySystem.Instance.FailureCauseIndex = 0;
			MenuSwitcher.Instance.LoadMenu(3);
			_deathScreenLoaded = true;
		}

		// AI - Update visibility calculation using the visibility manager.
		private void UpdateVisibility()
		{
			_visibilityManager.UpdateVisibility(Spellcaster.Hidden);
		}

		// AI - Allow the player to drop through one-way platforms by pressing down.
		private void HandlePlatformDropThrough()
		{
			bool pressingDown = Input.GetAxis("Vertical") < -0.5f;
			float targetOffset = pressingDown ? 180f : 0f;

			for (int i = 0; i < AffectedPlatforms.Length; i++)
			{
				if (AffectedPlatforms[i].gameObject.layer != GameConstants.DROP_THROUGH_PLATFORM_LAYER)
				{
					continue;
				}

				AffectedPlatforms[i].rotationalOffset = targetOffset;
			}
		}

		// AI - Check if the player has stopped falling after entering a death zone, then kill them.
		private void CheckFallingDeath()
		{
			if (!_falling || Mathf.Abs(Rigidbody.linearVelocity.y) >= 0.05f)
			{
				return;
			}

			TakeHit(GameConstants.INSTANT_KILL_DAMAGE);
		}

		// AI - Determine if any guard or sentry has fully spotted the player.
		private void UpdatePlayerSeenStatus()
		{
			IsSeen = false;

			for (int i = 0; i < _guards.Length; i++)
			{
				if (_guards[i] == null || _guards[i].SuspicionPercentage != GameConstants.FULLY_SPOTTED_THRESHOLD)
				{
					continue;
				}

				IsSeen = true;
				break;
			}

			if (!IsSeen)
			{
				for (int i = 0; i < _sentries.Length; i++)
				{
					if (_sentries[i] == null || _sentries[i].SuspicionPercentage != GameConstants.FULLY_SPOTTED_THRESHOLD)
					{
						continue;
					}

					IsSeen = true;
					break;
				}
			}

			if (IsSeen)
			{
				Spellcaster.EndSpell(SpellCasting.SpellNames.Mimic);
			}
		}

		#endregion

		#region Audio Functions

		// AI - Play a sound effect through the audio manager.
		public void PlaySound(string clipName)
		{
			AudioManager.Instance.PlaySound(clipName);
		}

		#endregion

		#region Movement Functions

		// AI - Flip the player's sprite to face the opposite direction.
		public void Flip()
		{
			PlatformerCharacter.Flip();
		}

		#endregion

		#region Combat Functions

		// AI - Trigger an attack or knockout animation. typeOfAttack: 0 = attack, 1 = knockout.
		// AI - Note: With Mimic, this flows through SpellCaster -> Mimic Prefab -> Player (InDisguise) -> PlayerController.
		public void Attack(int typeOfAttack)
		{
			Spellcaster.EndSpell(Spellcaster.CurrentSpell);
			Animator.SetTrigger(AnimAttacking);
			Animator.SetInteger(AnimAttackType, typeOfAttack);
		}

		// AI - Display or hide attack icons in the UI.
		public void LoadAttackIcons(bool load)
		{
			InGameManagement.Instance.LoadAttackIcons(load);
		}

		#endregion

		#region Spell Functions

		// AI - Toggle the Shadow Sink state, adjusting collision layers and sprite sorting.
		public void ShadowSink(bool inShadowSink)
		{
			InShadowSink = inShadowSink;
			Physics2D.SetLayerCollisionMask(GameConstants.PLAYER_LAYER, HidingPlaceLayerMasks[InShadowSink ? 1 : 0]);
			PlayerSprite.sortingOrder = InShadowSink ? 0 : 2;
			_visibilityManager.SetHidden(inShadowSink);
		}

		// AI - Toggle the disguise state when using the Mimic spell.
		public void InDisguise(bool isDisguised)
		{
			DisguisedAsGuard = isDisguised;
			PlatformerCharacter.DisguisedAsGuard = DisguisedAsGuard;
			Animator.SetBool(AnimDisguised, isDisguised);
			Animator.SetTrigger(AnimMimicUsed);
		}

		#endregion

		#region Hiding Functions

		// AI - Enter or exit a hiding place, adjusting collision layers and sprite sorting.
		public void IsInHidingPlace(bool isHiding)
		{
			Physics2D.SetLayerCollisionMask(GameConstants.PLAYER_LAYER, HidingPlaceLayerMasks[isHiding ? 1 : 0]);
			PlayerSprite.sortingOrder = isHiding ? -7 : 2;
			Animator.SetBool(AnimUnderTable, isHiding);
			Freeze(isHiding);
		}

		#endregion

		#region State Functions

		// AI - Freeze or unfreeze the player's movement and actions.
		public void Freeze(bool freeze)
		{
			CanAttack = !freeze;
			Spellcaster.SetCanSpellcast(!freeze);
			PlatformerCharacter.Frozen = freeze;

			if (freeze)
			{
				Rigidbody.linearVelocity = Vector2.zero;
			}
		}

		// AI - Mark the player as falling, which will trigger death when they hit the ground.
		public void IsFalling(bool isFalling)
		{
			_falling = isFalling;
		}

		#endregion

		#region Damage and Healing Functions

		// AI - Called when the player is hit, playing audio and updating the health bar.
		private void OnPlayerHit(float timeHit, float startingHealth, float health)
		{
			PlaySound("Player Hurt");
			_healthManager.TakeDamage(StartingHealth - health);
		}

		// AI - Apply damage to the player using the health manager.
		public override void TakeHit(float damage)
		{
			base.TakeHit(damage);
			_healthManager.TakeDamage(damage);
		}

		// AI - Heal the player using the health manager.
		public override void Heal(float heals)
		{
			base.Heal(heals);
			_healthManager.Heal(heals);
		}

		// AI - Handle player death, playing audio, triggering animations, and freezing controls.
		protected override void Die()
		{
			base.Die();
			
			PlaySound("Player Death");
			Dead = true;
			Animator.SetTrigger(AnimDies);
			Animator.SetBool(AnimDead, true);
			Freeze(true);
			CanAttack = false;
			_timeOfDeath = Time.time;
			
			_healthManager.StopRegeneration();
		}

		#endregion

		#region Collision Functions

		// AI - Detect when the player enters a light area or item trigger zone.
		private void OnTriggerEnter2D(Collider2D otherCollider)
		{
			LightArea lightArea = otherCollider.GetComponent<LightArea>();
			Item item = otherCollider.GetComponent<Item>();
			
			if (lightArea != null)
			{
				_visibilityManager.AddLight(lightArea);
			}

			if (item != null)
			{
				_interactionHandler.AddItem(item);
			}
		}

		// AI - Detect when the player exits a light area or item trigger zone.
		private void OnTriggerExit2D(Collider2D otherCollider)
		{
			LightArea lightArea = otherCollider.GetComponent<LightArea>();
			Item item = otherCollider.GetComponent<Item>();
			
			if (lightArea != null)
			{
				_visibilityManager.RemoveLight(lightArea);
			}

			if (item != null)
			{
				_interactionHandler.RemoveItem(item);
			}
		}

		#endregion

		#region Quick Save Functions

		// AI - Save the player's current state for quick save functionality.
		public void QuickSave()
		{
			_playerQuickSave.Position = transform.position;
			_playerQuickSave.Direction = transform.localScale.x;
			_playerQuickSave.Health = Health;
			_playerQuickSave.Mana = Spellcaster.CurrentMana;
			_playerQuickSave.Frozen = PlatformerCharacter.Frozen;
			_playerQuickSave.Spell = Spellcaster.CurrentSpell;
			_playerQuickSave.Inventory.Clear();
			_playerQuickSave.Inventory.AddRange(Inventory);
		}

		// AI - Restore the player to a previously saved state.
		public void QuickLoad()
		{
			transform.position = _playerQuickSave.Position;
			if (transform.localScale != new Vector3(_playerQuickSave.Direction, transform.localScale.y, transform.localScale.z))
			{
				Flip();
			}
			Health = _playerQuickSave.Health;
			Dead = false;
			Animator.SetBool(AnimDead, false);
			_deathScreenLoaded = false;
			_healthManager.Revive(_playerQuickSave.Health);
			Spellcaster.CurrentMana = _playerQuickSave.Mana;
			Freeze(_playerQuickSave.Frozen);
			Spellcaster.CurrentSpell = _playerQuickSave.Spell;
			Inventory.Clear();
			Inventory.AddRange(_playerQuickSave.Inventory);
		}

		#endregion
	}
}
