using Player;
using System.Collections;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Spells
{
	public class SpellCasting : MonoBehaviour
	{
		#region Enums

		public enum SpellNames
		{
			Translocation,
			ShadowSink,
			Mimic,
			Stasis,
			HellfireBlast,
			ExtremeForce,
			Traitor
		}

		#endregion

		#region Fields

		public int SpellLevel = 1;

		public Translocation TranslocationPrefab;
		public ShadowSink ShadowSinkPrefab;
		public Mimic MimicPrefab;
		public Stasis StasisPrefab;
		public HellfireBlast HellfireBlastPrefab;
		public ExtremeForce ExtremeForcePrefab;
		public Traitor TraitorPrefab;
		public Transform FirePoint;
		public Transform InGameHUD;
		public Image ManaBar;

		public SpellNames CurrentSpell;
		public bool Hidden;

		public int ManaCap;
		public int CurrentMana;
		public float ManaRestoreTick;
		public int ManaRestoreAmount;
		public float ManaRestoreBegin;
		public Vector3 HellfireBlastOffset;

		private float _nextManaRestoreTime;

		private PlayerController _playerController;
		private Rigidbody2D _rigidbody;

		private Translocation _currentTranslocationObject;
		private ShadowSink _currentShadowSinkObject;
		private Mimic _currentMimicObject;
		private Stasis _currentStasisObject;
		private Traitor _currentTraitorObject;

		private bool _canSpellCast = true;
		private bool _costingMana;
		private float _manaDeductTick;
		private int _manaTickCost;
		private float _nextManaDeductTime;

		private bool _triggerReleased = true;

		private Coroutine _manaRegenCoroutine;

		#endregion

		#region Initialization Functions

		private void Start()
		{
			_playerController = GetComponent<PlayerController>();
			_rigidbody = GetComponent<Rigidbody2D>();
			
			StartManaRegeneration();
		}

		#endregion

		#region Update Functions

		private void Update()
		{
			ProcessManaSystem();
			EquipSpell();
			CastSpell();
			UpdateManaBar();
		}

		// AI - Handle mana deduction or restoration based on current state.
		private void ProcessManaSystem()
		{
			if (_costingMana)
			{
				if (Time.time >= _nextManaDeductTime)
				{
					ManaDeductTick();
				}
				return;
			}

			// AI - Mana restoration handled by coroutine now, no need for time checks here.
		}

		// AI - Update the mana bar UI.
		private void UpdateManaBar()
		{
			if (ManaBar != null)
			{
				ManaBar.fillAmount = (float)CurrentMana / (float)ManaCap;
			}
		}

		#endregion

		#region Mana System Functions

		// AI - Start the mana regeneration coroutine.
		private void StartManaRegeneration()
		{
			if (_manaRegenCoroutine != null)
			{
				StopCoroutine(_manaRegenCoroutine);
			}

			_manaRegenCoroutine = StartCoroutine(RegenerateMana());
		}

		// AI - Coroutine that gradually restores mana over time.
		private IEnumerator RegenerateMana()
		{
			while (true)
			{
				// AI - Wait for the next restore time.
				yield return new WaitForSeconds(ManaRestoreTick);

				// AI - Skip restoration if costing mana or already at max.
				if (_costingMana || CurrentMana >= ManaCap)
				{
					continue;
				}

				// AI - Wait for the restore delay after last mana use.
				if (Time.time < _nextManaRestoreTime)
				{
					continue;
				}

				ManaRestore();
			}
		}

		// AI - Deduct mana on a regular tick for sustained spells.
		private void ManaDeductTick()
		{
			if (CurrentMana > 0)
			{
				CurrentMana = Mathf.Max(0, CurrentMana - _manaTickCost);
			}

			if (CurrentMana == 0)
			{
				EndSpell(CurrentSpell);
			}

			_nextManaDeductTime = Time.time + _manaDeductTick;
		}

		// AI - Restore mana gradually.
		private void ManaRestore()
		{
			if (CurrentMana >= ManaCap)
			{
				return;
			}

			CurrentMana = Mathf.Min(ManaCap, CurrentMana + ManaRestoreAmount);
		}

		// AI - Check if the player has enough mana for a spell.
		private bool CheckMana(int manaCost)
		{
			return manaCost <= CurrentMana;
		}

		// AI - Deduct mana and start the restore delay.
		private void DeductMana(int manaCost)
		{
			CurrentMana -= manaCost;
			_nextManaRestoreTime = Time.time + ManaRestoreBegin;
		}

		#endregion

		#region Spell Equipment Functions

		// AI - Handle keyboard/gamepad input for equipping spells.
		private void EquipSpell()
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				EquipSpellByType(SpellNames.Translocation, 0);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				EquipSpellByType(SpellNames.ShadowSink, 1);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				EquipSpellByType(SpellNames.HellfireBlast, 2);
			}
			else if (SpellLevel >= 2)
			{
				if (Input.GetKeyDown(KeyCode.Alpha4))
				{
					EquipSpellByType(SpellNames.Mimic, 3);
				}
				else if (Input.GetKeyDown(KeyCode.Alpha5))
				{
					EquipSpellByType(SpellNames.ExtremeForce, 4);
				}
				else if (SpellLevel >= 3)
				{
					if (Input.GetKeyDown(KeyCode.Alpha6))
					{
						EquipSpellByType(SpellNames.Stasis, 5);
					}
					else if (Input.GetKeyDown(KeyCode.Alpha7))
					{
						EquipSpellByType(SpellNames.Traitor, 6);
					}
				}
			}
		}

		// AI - Equip a spell by type, ending the current spell if different.
		private void EquipSpellByType(SpellNames spellName, int outlineIndex)
		{
			if (CurrentSpell != spellName)
			{
				EndSpell(CurrentSpell);
			}

			CurrentSpell = spellName;
			InGameManagement.Instance.ChangeOutline(outlineIndex);
		}

		// AI - Equip a spell by number (for UI/external systems).
		public void EquipSpell(int spellNumber)
		{
			SpellNames spellName = spellNumber switch
			{
				0 => SpellNames.Translocation,
				1 => SpellNames.ShadowSink,
				2 => SpellNames.HellfireBlast,
				3 => SpellNames.Mimic,
				4 => SpellNames.ExtremeForce,
				5 => SpellNames.Stasis,
				6 => SpellNames.Traitor,
				_ => CurrentSpell
			};

			if (CurrentSpell != spellName)
			{
				EndSpell(CurrentSpell);
			}

			CurrentSpell = spellName;
		}

		#endregion

		#region Spell Casting Functions

		// AI - Handle input for casting the currently equipped spell.
		private void CastSpell()
		{
			if (!_canSpellCast)
			{
				return;
			}
			
			if (Input.GetAxis("Use Item") == 1f && _triggerReleased)
			{
				_triggerReleased = false;
				AttemptCastSpell();
			}

			if (Input.GetAxis("Use Item") == 0f)
			{
				_triggerReleased = true;
				HandleSpellRelease();
			}
		}

		// AI - Try to cast the currently equipped spell if mana requirements are met.
		private void AttemptCastSpell()
		{
			switch (CurrentSpell)
			{
				case SpellNames.Translocation:
					CastTranslocation();
					break;
				case SpellNames.ShadowSink:
					CastShadowSink();
					break;
				case SpellNames.Mimic:
					CastMimic();
					break;
				case SpellNames.HellfireBlast:
					CastHellfireBlast();
					break;
				case SpellNames.ExtremeForce:
					CastExtremeForce();
					break;
				case SpellNames.Stasis:
					CastStasis();
					break;
				case SpellNames.Traitor:
					CastTraitor();
					break;
			}
		}

		// AI - Handle special logic when certain spells are released.
		private void HandleSpellRelease()
		{
			if (CurrentSpell == SpellNames.Mimic && _currentMimicObject != null && !_currentMimicObject.Disguised)
			{
				EndSpell(SpellNames.Mimic);
				Destroy(_currentMimicObject.gameObject);
			}
			else if (CurrentSpell == SpellNames.Traitor && _currentTraitorObject != null)
			{
				EndSpell(SpellNames.Traitor);
				Destroy(_currentTraitorObject.gameObject);
			}
		}

		#endregion

		#region Individual Spell Cast Functions

		// AI - Cast Translocation spell.
		private void CastTranslocation()
		{
			if (!CheckMana(TranslocationPrefab.ManaCost))
			{
				return;
			}

			_currentTranslocationObject = Instantiate(TranslocationPrefab,
				transform.position + new Vector3(5f * transform.localScale.x, 0f, 0f), transform.rotation,
				transform);
			_currentTranslocationObject.Init(_playerController, this);
			EndSpell(SpellNames.ShadowSink);
			EndSpell(SpellNames.Mimic);
		}

		// AI - Cast Shadow Sink spell.
		private void CastShadowSink()
		{
			if (!CheckMana(ShadowSinkPrefab.ManaCost) || _currentShadowSinkObject != null || _rigidbody.linearVelocity.x != 0f)
			{
				return;
			}

			_currentShadowSinkObject = Instantiate(ShadowSinkPrefab, transform.position, transform.rotation);
			_currentShadowSinkObject.Init(_playerController, this);
			DeductMana(_currentShadowSinkObject.ManaCost);
			Hidden = true;
			_costingMana = true;
			_manaDeductTick = _currentShadowSinkObject.ManaDeductTick;
			_manaTickCost = _currentShadowSinkObject.ManaTickCost;
			_nextManaDeductTime = Time.time + _manaDeductTick;

			EndSpell(SpellNames.Mimic);
		}

		// AI - Cast Mimic spell.
		private void CastMimic()
		{
			if (!CheckMana(MimicPrefab.ManaCost) || _playerController.IsSeen || _currentMimicObject != null)
			{
				return;
			}

			Vector3 mimicPosition = transform.position + new Vector3(5f * transform.localScale.x, 0f, 0f);
			_currentMimicObject = Instantiate(MimicPrefab, mimicPosition, transform.rotation, transform);
			_currentMimicObject.Init(_playerController, this);

			EndSpell(SpellNames.ShadowSink);
		}

		// AI - Cast Hellfire Blast spell.
		private void CastHellfireBlast()
		{
			if (!CheckMana(HellfireBlastPrefab.ManaCost))
			{
				return;
			}

			HellfireBlast hellfireBlastObject = Instantiate(HellfireBlastPrefab, FirePoint.position + HellfireBlastOffset,
				Quaternion.LookRotation(Vector3.forward * (transform.localScale.x / Mathf.Abs(transform.localScale.x))));
			hellfireBlastObject.Init(_playerController, this);
			Destroy(hellfireBlastObject.gameObject, 10f);
			DeductMana(HellfireBlastPrefab.ManaCost);
			EndSpell(SpellNames.ShadowSink);
			EndSpell(SpellNames.Mimic);
		}

		// AI - Cast Extreme Force spell.
		private void CastExtremeForce()
		{
			if (!CheckMana(ExtremeForcePrefab.ManaCost))
			{
				return;
			}

			Quaternion spawnRotation = Quaternion.LookRotation(Vector3.forward * (transform.localScale.x / Mathf.Abs(transform.localScale.x)));
			ExtremeForce extremeForceObject = Instantiate(ExtremeForcePrefab, FirePoint.position, spawnRotation);
			extremeForceObject.Init(_playerController, this);
			Destroy(extremeForceObject.gameObject, 3f);

			DeductMana(ExtremeForcePrefab.ManaCost);
			EndSpell(SpellNames.ShadowSink);
			EndSpell(SpellNames.Mimic);
		}

		// AI - Cast Stasis spell.
		private void CastStasis()
		{
			if (!CheckMana(StasisPrefab.ManaCost))
			{
				return;
			}

			_currentStasisObject = Instantiate(StasisPrefab,
				transform.position + new Vector3(5f * transform.localScale.x, 2.5f, 0f),
				transform.rotation);
			_currentStasisObject.Init(_playerController, this);
			EndSpell(SpellNames.ShadowSink);
			EndSpell(SpellNames.Mimic);
		}

		// AI - Cast Traitor spell.
		private void CastTraitor()
		{
			if (!CheckMana(TraitorPrefab.ManaCost) || _currentTraitorObject != null)
			{
				return;
			}

			_currentTraitorObject = Instantiate(TraitorPrefab,
				transform.position + new Vector3(5f * transform.localScale.x, 0f, 0f),
				transform.rotation, transform);
			_currentTraitorObject.Init(_playerController, this);

			EndSpell(SpellNames.ShadowSink);
			EndSpell(SpellNames.Mimic);
		}

		#endregion

		#region Spell Ending Functions

		// AI - End a spell and clean up its effects.
		public void EndSpell(SpellNames spell)
		{
			switch (spell)
			{
				case SpellNames.Translocation:
					EndTranslocation();
					break;
				case SpellNames.ShadowSink:
					EndShadowSink();
					break;
				case SpellNames.Mimic:
					EndMimic();
					break;
				case SpellNames.Traitor:
					EndTraitor();
					break;
			}
		}

		// AI - End Translocation spell.
		private void EndTranslocation()
		{
			if (_currentTranslocationObject != null)
			{
				Destroy(_currentTranslocationObject.gameObject);
			}
		}

		// AI - End Shadow Sink spell.
		private void EndShadowSink()
		{
			if (_currentShadowSinkObject != null)
			{
				_currentShadowSinkObject.EndShadowSink();
				Hidden = false;
				_costingMana = false;
				_manaDeductTick = 0f;
				_manaTickCost = 0;
			}
		}

		// AI - End Mimic spell.
		private void EndMimic()
		{
			if (_currentMimicObject != null)
			{
				_currentMimicObject.EndMimic();
				_costingMana = false;
				_manaDeductTick = 0f;
				_manaTickCost = 0;
			}
		}

		// AI - End Traitor spell.
		private void EndTraitor()
		{
			if (_currentTraitorObject != null)
			{
				_currentTraitorObject.EndTraitor();
			}
		}

		#endregion

		#region Spell Event Callbacks

		// AI - Set whether the player can cast spells.
		public void SetCanSpellcast(bool canSpellcast)
		{
			if (!canSpellcast)
			{
				EndSpell(CurrentSpell);
			}

			_canSpellCast = canSpellcast;
		}

		// AI - Called when translocation is used.
		public void TranslocationOccured()
		{
			if (_currentTranslocationObject != null)
			{
				DeductMana(_currentTranslocationObject.ManaCost);
			}
		}

		// AI - Called when player successfully disguises.
		public void Disguised()
		{
			if (_currentMimicObject == null)
			{
				return;
			}

			DeductMana(_currentMimicObject.ManaCost);
			_costingMana = true;
			_manaDeductTick = _currentMimicObject.ManaDeductTick;
			_manaTickCost = _currentMimicObject.ManaTickCost;
			_nextManaDeductTime = Time.time + _manaDeductTick;
		}

		// AI - Called when stasis field is created.
		public void StasisOccured()
		{
			if (_currentStasisObject != null)
			{
				DeductMana(_currentStasisObject.ManaCost);
			}
		}

		// AI - Called when a guard is corrupted.
		public void Corrupted()
		{
			if (_currentTraitorObject != null)
			{
				DeductMana(_currentTraitorObject.ManaCost);
			}
		}

		#endregion
	}
}
