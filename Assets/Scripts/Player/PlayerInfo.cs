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
	public class PlayerInfo : LivingEntity
	{
		[Serializable]
		struct PlayerQuickSave {

			public Vector2 position;
			public float direction;
			public float health;
			public int mana;
			public bool frozen;
			public SpellCasting.SpellNames spell;
			public List<string> inventory;
		}
	
		public float baseVisibilityFactor = 0.1f;
		[Range(0, 1)] public float visibilityFactor = 0.1f;
		public float rawVisibilityFactor = 0.1f;
		public float lightTotals = 0f;
		public bool inShadowSink = false;
		public bool disguisedAsGuard = false;
		public bool canAttack = true;
		public bool isSeen = false;
		public Image healthBar;
		public Transform effectedPlatforms;
		public Transform enemiesHolder;
		public Transform canvas;

		public List<string> inventory = new List<string>();
		public List<GameObject> lights = new List<GameObject>();
		public List<GameObject> items = new List<GameObject>();

		public LayerMask[] hidingPlaceLayerMasks;

		AudioManager audioManager;
		SpellCasting spellcaster;
		PlatformerCharacter2D platformerCharacter;
		Rigidbody2D rb;
		Animator anim;
		PlayerQuickSave playerQuickSave;

		Guard[] guards;
		Sentry[] sentries;

		Transform inGameHUD;
		Transform missionFailedMenu;

		float healthRegenTime = 3f;
		float healthRegenBegin;
		float healthRegenTick = 0.5f;
		float healthRegenPerTick = 2f;

		bool canUse = false;
		bool falling = false;

		float timeOfDeath = 0f;
		bool deathScreenLoaded = false;

		protected override void Start()
		{
			base.Start();
			audioManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<AudioManager>();
			spellcaster = GetComponent<SpellCasting>();
			platformerCharacter = GetComponent<PlatformerCharacter2D>();
			rb = GetComponent<Rigidbody2D>();
			anim = GetComponent<Animator>();

			playerQuickSave = new PlayerQuickSave();
			playerQuickSave.inventory = new List<string>();

			guards = enemiesHolder.GetComponentsInChildren<Guard>();
			sentries = enemiesHolder.GetComponentsInChildren<Sentry>();
			inGameHUD = canvas.GetChild(0);
			missionFailedMenu = canvas.GetChild(3);

			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

			OnDeath += OnPlayerDeath;
			OnHit += OnPlayerHit;
		}

		void Update()
		{
			if (!dead)
			{
				lightTotals = 0f;
				for (int i = 0; i < lights.Count; i++)
				{
					float radius = lights[i].GetComponent<CircleCollider2D>().radius;
					float distance = Vector2.Distance(transform.position, lights[i].transform.position);
					lightTotals += radius * (radius - distance) / 50;
				}

				rawVisibilityFactor = baseVisibilityFactor + lightTotals;

				if (!spellcaster.hidden)
				{
					visibilityFactor = rawVisibilityFactor;
				}
				else
				{
					visibilityFactor = 0f;
				}

				if (health < startingHealth && healthRegenBegin <= Time.time)
				{
					Heal(healthRegenPerTick);
					healthBar.fillAmount = health / startingHealth;
					healthRegenBegin = Time.time + healthRegenTick;
				}

				if (items.Count > 0)
				{
					if (!canUse)
					{
						inGameHUD.GetComponent<InGameManagement>().LoadUseIcon(true);
						canUse = true;
					}

					Item currentItem = items[0].GetComponent<Item>();
					if (currentItem.type.ToString().Equals("missionItem"))
					{
						if (Input.GetButtonDown("Use"))
						{
							PlaySound("Swipe");
							inventory.Add(currentItem.itemName);
							currentItem.gameObject.SetActive(false);
							if (currentItem.itemName.Equals("MacGuffin"))
							{
								GameObject.FindGameObjectWithTag("GameController").GetComponent<ObjectiveSystem>()
									.SetObjectiveStatus(101, Objective.Status.completed);
								inGameHUD.GetComponent<InGameManagement>().LoadMissionText("Make your way to the Exit");
							}
							if (currentItem.itemName.Equals("MacGuffin2"))
							{
								GameObject.FindGameObjectWithTag("GameController").GetComponent<ObjectiveSystem>()
									.SetObjectiveStatus(201, Objective.Status.completed);
								inGameHUD.GetComponent<InGameManagement>().LoadMissionText("Make your way to the Exit");
							}

							items.Remove(currentItem.gameObject);
						}
					}
					else if (currentItem.type.ToString().Equals("button"))
					{
						if (Input.GetButtonDown("Use"))
						{
							PlaySound("Use Button");
							currentItem.gameObject.GetComponent<ButtonItem>().UseButton();
						}
					}
					else if (currentItem.type.ToString().Equals("door"))
					{
						if (Input.GetButtonDown("Use"))
						{
							PlaySound("Door");
							currentItem.gameObject.GetComponent<Door>().UseDoor(inventory);
						}
					}
					else if (currentItem.type.ToString().Equals("hidingPlace") && !disguisedAsGuard)
					{
						if (Input.GetButtonDown("Use"))
						{
							currentItem.gameObject.GetComponent<HidingPlace>().Hide();
						}
					}

					if (Input.GetButtonDown("Use") && !disguisedAsGuard)
					{
						anim.SetTrigger("Use");
					}
				}
				else if (canUse)
				{
					inGameHUD.GetComponent<InGameManagement>().LoadUseIcon(false);
					canUse = false;
				}

				if (Input.GetAxis("Vertical") < -0.5f)
				{
					for (int i = 0; i < effectedPlatforms.childCount; i++)
					{
						if (effectedPlatforms.GetChild(i).gameObject.layer == 12)
						{
							effectedPlatforms.GetChild(i).GetComponent<PlatformEffector2D>().rotationalOffset = 180f;
						}
					}
				}

				if (Input.GetAxis("Vertical") >= -0.5f)
				{
					for (int i = 0; i < effectedPlatforms.childCount; i++)
					{
						if (effectedPlatforms.GetChild(i).gameObject.layer == 12)
						{
							effectedPlatforms.GetChild(i).GetComponent<PlatformEffector2D>().rotationalOffset = 0f;
						}
					}
				}

				if (falling && Mathf.Abs(rb.linearVelocity.y) < 0.05f)
				{
					TakeHit(1000);
				}

				UpdatePlayerSeenStatus();
			}
			else
			{
				if (Time.time >= timeOfDeath + 2f && !deathScreenLoaded)
				{
					if (Input.GetJoystickNames().Length == 0)
					{
						Cursor.lockState = CursorLockMode.None;
						Cursor.visible = true;
					}

					GameObject.FindGameObjectWithTag("GameController").GetComponent<MenuSwitcher>().LoadMenu(3);
					missionFailedMenu.GetComponent<MissionFailedManagement>().SetCause(0);
					deathScreenLoaded = true;
				}
			}
		}

		public void PlaySound(string _name)
		{
			audioManager.PlaySound(_name);
		}

		public void Flip()
		{
			platformerCharacter.Flip();
		}

		public void Attack(int typeOfAttack)
		{
			//0 = attack, 1 = knockout
			spellcaster.EndSpell(spellcaster.currentSpell); //WITH MIMIC THIS GOES FROM THE SPELLCASTER TO THE MIMIC PREFAB BACK TO THE PLAYER (IN-DISGUISE FUNCTION) THEN TO THE PLAYER CONTROLLER
			anim.SetTrigger("Attacking");
			anim.SetInteger("Attack Type", typeOfAttack);
		}

		public void InShadowSink(bool _inShadowSink)
		{
			inShadowSink = _inShadowSink;
			Physics2D.SetLayerCollisionMask(8, hidingPlaceLayerMasks[inShadowSink ? 1 : 0]);
			GetComponent<SpriteRenderer>().sortingOrder = inShadowSink ? 0 : 2;
		}

		public void InHidingPlace(bool isHiding)
		{
			Physics2D.SetLayerCollisionMask(8, hidingPlaceLayerMasks[isHiding ? 1 : 0]);
			GetComponent<SpriteRenderer>().sortingOrder = isHiding ? -7 : 2;
			anim.SetBool("Under Table", isHiding);
			Freeze(isHiding);
		}

		public void Freeze(bool freezing)
		{
			canAttack = !freezing;
			spellcaster.SetCanSpellcast(!freezing);
			platformerCharacter.frozen = freezing;
		}

		public void InDisguise(bool isDisguised)
		{
			disguisedAsGuard = isDisguised;
			platformerCharacter.disguisedAsGuard = disguisedAsGuard;
			anim.SetBool("Disguised", isDisguised);
			anim.SetTrigger("Mimic Used");
		}

		public void IsFalling(bool isFalling)
		{
			falling = isFalling;
		}

		public void LoadAttackIcons(bool load)
		{
			inGameHUD.GetComponent<InGameManagement>().LoadAttackIcons(load);
		}

		void UpdatePlayerSeenStatus()
		{
			isSeen = false;
			if (!isSeen)
			{
				for (int i = 0; i < guards.Length && !isSeen; i++)
				{
					if (!isSeen)
					{
						isSeen = guards[i].suspicionPercentage == 1f;
					}
				}
			}

			if (!isSeen)
			{
				for (int i = 0; i < sentries.Length; i++)
				{
					if (!isSeen)
					{
						isSeen = sentries[i].suspicionPercentage == 1f;
					}
				}
			}

			if (isSeen)
				spellcaster.EndSpell(SpellCasting.SpellNames.mimic);
		}

		void OnPlayerHit(float timeHit, float startingHealth, float health)
		{
			audioManager.PlaySound("Player Hurt");
			healthBar.fillAmount = health / startingHealth;
			healthRegenBegin = timeHit + healthRegenTime;
		}

		void OnPlayerDeath()
		{
			audioManager.PlaySound("Player Death");
			dead = true;
			healthBar.fillAmount = 0f;
			anim.SetTrigger("Dies");
			anim.SetBool("Dead", true);
			Freeze(true);
			canUse = false;
			canAttack = false;
			timeOfDeath = Time.time;
		}

		void OnTriggerEnter2D(Collider2D collider)
		{
			if (collider.tag.Equals("Light") && !lights.Contains(collider.gameObject))
			{
				lights.Add(collider.gameObject);
			}

			if (collider.tag.Equals("Item") && !items.Contains(collider.gameObject))
			{
				items.Add(collider.gameObject);
			}
		}

		void OnTriggerExit2D(Collider2D collider)
		{
			if (collider.tag.Equals("Light") && lights.Contains(collider.gameObject))
			{
				lights.Remove(collider.gameObject);
			}

			if (collider.tag.Equals("Item") && !collider.IsTouching(transform.GetComponent<BoxCollider2D>()))
			{
				items.Remove(collider.gameObject);
			}
		}

		public void QuickSave()
		{
			playerQuickSave.position = transform.position;
			playerQuickSave.direction = transform.localScale.x;
			playerQuickSave.health = health;
			playerQuickSave.mana = spellcaster.currentMana;
			playerQuickSave.frozen = platformerCharacter.frozen;
			playerQuickSave.spell = spellcaster.currentSpell;
			playerQuickSave.inventory.Clear();
			playerQuickSave.inventory.AddRange(inventory);
		}

		public void QuickLoad()
		{
			transform.position = playerQuickSave.position;
			if (transform.localScale != new Vector3(playerQuickSave.direction, transform.localScale.y, transform.localScale.z))
			{
				Flip();
			}
			health = playerQuickSave.health;
			dead = false;
			anim.SetBool("Dead", false);
			deathScreenLoaded = false;
			healthBar.fillAmount = health;
			spellcaster.currentMana = playerQuickSave.mana;
			Freeze(false);
			spellcaster.currentSpell = playerQuickSave.spell;
			inventory.Clear();
			inventory.AddRange(playerQuickSave.inventory);
		}
	}
}
