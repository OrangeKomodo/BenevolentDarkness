using System;
using System.Collections.Generic;
using AI.Guard;
using AI.Sentry;
using DamageSystem;
using GameManager;
using Items;
using Spells;
using UI;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets._2D;

namespace Player
{
	public class PlayerController : LivingEntity
	{
		[Serializable]
		private struct PlayerQuickSave {

			public Vector2 Position;
			public float Direction;
			public float Health;
			public int Mana;
			public bool Frozen;
			public SpellCasting.SpellNames Spell;
			public List<string> Inventory;
		}
		
		public SpellCasting Spellcaster;
		public CameraController CameraController;
		public PlatformerCharacter2D PlatformerCharacter;
		public SpriteRenderer PlayerSprite;
		public Rigidbody2D Rigidbody;
		public Animator Animator;
	
		public float BaseVisibilityFactor = 0.1f;
		[Range(0, 1)]
		public float VisibilityFactor = 0.1f;
		public float RawVisibilityFactor = 0.1f;
		public float LightTotals = 0f;
		public bool InShadowSink = false;
		public bool DisguisedAsGuard = false;
		public bool CanAttack = true;
		public bool IsSeen = false;
		public Image HealthBar;
		public PlatformEffector2D[] AffectedPlatforms;
		public Transform EnemiesHolder;
		public Transform Canvas;

		[HideInInspector] public List<string> Inventory = new List<string>();
		
		private List<LightArea> _lights = new List<LightArea>();
		private List<Item> _items = new List<Item>();

		public LayerMask[] HidingPlaceLayerMasks;
		
		private PlayerQuickSave _playerQuickSave;
		
		private Guard[] _guards;
		private Sentry[] _sentries;

		private const float HealthRegenTime = 3f;
		private const float HealthRegenTick = 0.5f;
		private const float HealthRegenPerTick = 2f;
		private float _healthRegenBegin;

		private bool _canUse = false;
		private bool _falling = false;

		private float _timeOfDeath = 0f;
		private bool _deathScreenLoaded = false;

		protected override void Start()
		{
			base.Start();

			_playerQuickSave = new PlayerQuickSave
			{
				Inventory = new List<string>()
			};

			_guards = EnemiesHolder.GetComponentsInChildren<Guard>();
			_sentries = EnemiesHolder.GetComponentsInChildren<Sentry>();

			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

			OnHit += OnPlayerHit;
		}

		private void OnApplicationQuit()
		{
			OnHit -= OnPlayerHit;
		}

		private void Update()
		{
			if (Dead)
			{
				if (Time.time >= _timeOfDeath + 2f && !_deathScreenLoaded)
				{
					if (Input.GetJoystickNames().Length == 0)
					{
						Cursor.lockState = CursorLockMode.None;
						Cursor.visible = true;
					}

					MenuSwitcher.Instance.LoadMenu(3);
					MissionFailedManagement.Instance.SetCause(0);
					_deathScreenLoaded = true;
				}

				return;
			}
			
			LightTotals = 0f;
			for (int i = 0; i < _lights.Count; i++)
			{
				float radius = _lights[i].Collider2D.radius;
				float distance = Vector2.Distance(transform.position, _lights[i].transform.position);
				LightTotals += radius * (radius - distance) / 50f;
			}

			RawVisibilityFactor = BaseVisibilityFactor + LightTotals;

			VisibilityFactor = Spellcaster.Hidden ? 0f : RawVisibilityFactor;
			
			if (Health < StartingHealth && _healthRegenBegin <= Time.time)
			{
				Heal(HealthRegenPerTick);
				HealthBar.fillAmount = Health / StartingHealth;
				_healthRegenBegin = Time.time + HealthRegenTick;
			}

			if (_items.Count > 0)
			{
				if (!_canUse)
				{
					InGameManagement.Instance.LoadUseIcon(true);
					_canUse = true;
				}

				Item currentItem = _items[0];

				if (Input.GetButtonDown("Use") && !DisguisedAsGuard)
				{
					Animator.SetTrigger("Use");
				}

				if (Input.GetButtonDown("Use"))
				{
					if (currentItem is MissionItem missionItem)
					{
						missionItem.Interact();
						Inventory.Add(missionItem.ItemName);
						missionItem.gameObject.SetActive(false);
					}
					else if (currentItem is HidingPlace hidingPlace)
					{
						if (!DisguisedAsGuard)
						{
							hidingPlace.Interact();
						}
					}
					else
					{
						currentItem.Interact();
					}
				}
			}
			else if (_canUse)
			{
				InGameManagement.Instance.LoadUseIcon(false);
				_canUse = false;
			}

			if (Input.GetAxis("Vertical") < -0.5f)
			{
				for (int i = 0; i < AffectedPlatforms.Length; i++)
				{
					if (AffectedPlatforms[i].gameObject.layer == 12)
					{
						AffectedPlatforms[i].rotationalOffset = 180f;
					}
				}
			}

			if (Input.GetAxis("Vertical") >= -0.5f)
			{
				for (int i = 0; i < AffectedPlatforms.Length; i++)
				{
					if (AffectedPlatforms[i].gameObject.layer == 12)
					{
						AffectedPlatforms[i].rotationalOffset = 0f;
					}
				}
			}

			if (_falling && Mathf.Abs(Rigidbody.linearVelocity.y) < 0.05f)
			{
				TakeHit(1000);
			}

			UpdatePlayerSeenStatus();
		}

		public void PlaySound(string clipName)
		{
			AudioManager.Instance.PlaySound(clipName);
		}

		public void Flip()
		{
			PlatformerCharacter.Flip();
		}

		public void Attack(int typeOfAttack)
		{
			//0 = attack, 1 = knockout
			Spellcaster.EndSpell(Spellcaster.CurrentSpell); //WITH MIMIC THIS GOES FROM THE SPELLCASTER TO THE MIMIC PREFAB BACK TO THE PLAYER (IN-DISGUISE FUNCTION) THEN TO THE PLAYER CONTROLLER
			Animator.SetTrigger("Attacking");
			Animator.SetInteger("Attack Type", typeOfAttack);
		}

		public void ShadowSink(bool inShadowSink)
		{
			InShadowSink = inShadowSink;
			Physics2D.SetLayerCollisionMask(8, HidingPlaceLayerMasks[InShadowSink ? 1 : 0]);
			PlayerSprite.sortingOrder = InShadowSink ? 0 : 2;
		}

		public void IsInHidingPlace(bool isHiding)
		{
			Physics2D.SetLayerCollisionMask(8, HidingPlaceLayerMasks[isHiding ? 1 : 0]);
			PlayerSprite.sortingOrder = isHiding ? -7 : 2;
			Animator.SetBool("Under Table", isHiding);
			Freeze(isHiding);
		}

		public void Freeze(bool freeze)
		{
			CanAttack = !freeze;
			Spellcaster.SetCanSpellcast(!freeze);
			PlatformerCharacter.frozen = freeze;

			if (freeze)
			{
				Rigidbody.linearVelocity = Vector2.zero;
			}
		}

		public void InDisguise(bool isDisguised)
		{
			DisguisedAsGuard = isDisguised;
			PlatformerCharacter.disguisedAsGuard = DisguisedAsGuard;
			Animator.SetBool("Disguised", isDisguised);
			Animator.SetTrigger("Mimic Used");
		}

		public void IsFalling(bool isFalling)
		{
			_falling = isFalling;
		}

		public void LoadAttackIcons(bool load)
		{
			InGameManagement.Instance.LoadAttackIcons(load);
		}

		private void UpdatePlayerSeenStatus()
		{
			IsSeen = false;
			if (!IsSeen)
			{
				for (int i = 0; i < _guards.Length && !IsSeen; i++)
				{
					if (!IsSeen)
					{
						IsSeen = _guards[i].SuspicionPercentage == 1f;
					}
				}
			}

			if (!IsSeen)
			{
				for (int i = 0; i < _sentries.Length; i++)
				{
					if (!IsSeen)
					{
						IsSeen = _sentries[i].SuspicionPercentage == 1f;
					}
				}
			}

			if (IsSeen)
			{
				Spellcaster.EndSpell(SpellCasting.SpellNames.Mimic);
			}
		}

		private void OnPlayerHit(float timeHit, float startingHealth, float health)
		{
			PlaySound("Player Hurt");
			HealthBar.fillAmount = health / startingHealth;
			_healthRegenBegin = timeHit + HealthRegenTime;
		}

		public override void TakeHit(float damage)
		{
			base.TakeHit(damage);
		}

		public override void Heal(float heals)
		{
			base.Heal(heals);
		}

		protected override void Die()
		{
			base.Die();
			
			PlaySound("Player Death");
			Dead = true;
			HealthBar.fillAmount = 0f;
			Animator.SetTrigger("Dies");
			Animator.SetBool("Dead", true);
			Freeze(true);
			_canUse = false;
			CanAttack = false;
			_timeOfDeath = Time.time;
		}

		private void OnTriggerEnter2D(Collider2D otherCollider)
		{
			LightArea lightArea = otherCollider.GetComponent<LightArea>();
			Item item = otherCollider.GetComponent<Item>();
			
			if (lightArea != null && !_lights.Contains(lightArea))
			{
				_lights.Add(lightArea);
			}

			if (item != null && !_items.Contains(item))
			{
				_items.Add(item);
			}
		}

		private void OnTriggerExit2D(Collider2D otherCollider)
		{
			LightArea lightArea = otherCollider.GetComponent<LightArea>();
			Item item = otherCollider.GetComponent<Item>();
			
			if (lightArea != null && _lights.Contains(lightArea))
			{
				_lights.Remove(lightArea);
			}

			if (item != null && _items.Contains(item))
			{
				_items.Remove(item);
			}
		}

		public void QuickSave()
		{
			_playerQuickSave.Position = transform.position;
			_playerQuickSave.Direction = transform.localScale.x;
			_playerQuickSave.Health = Health;
			_playerQuickSave.Mana = Spellcaster.CurrentMana;
			_playerQuickSave.Frozen = PlatformerCharacter.frozen;
			_playerQuickSave.Spell = Spellcaster.CurrentSpell;
			_playerQuickSave.Inventory.Clear();
			_playerQuickSave.Inventory.AddRange(Inventory);
		}

		public void QuickLoad()
		{
			transform.position = _playerQuickSave.Position;
			if (transform.localScale != new Vector3(_playerQuickSave.Direction, transform.localScale.y, transform.localScale.z))
			{
				Flip();
			}
			Health = _playerQuickSave.Health;
			Dead = false;
			Animator.SetBool("Dead", false);
			_deathScreenLoaded = false;
			HealthBar.fillAmount = Health;
			Spellcaster.CurrentMana = _playerQuickSave.Mana;
			Freeze(_playerQuickSave.Frozen);
			Spellcaster.CurrentSpell = _playerQuickSave.Spell;
			Inventory.Clear();
			Inventory.AddRange(_playerQuickSave.Inventory);
		}
	}
}
