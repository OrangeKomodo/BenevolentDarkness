using Player;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Spells
{
	public class SpellCasting : MonoBehaviour
	{
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

		private Vector3 _shadowSinkPlayerPosition;

		private bool _triggerReleased = true;

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

		private void Start()
		{
			_playerController = GetComponent<PlayerController>();
			_rigidbody = GetComponent<Rigidbody2D>();
		}

		private void Update()
		{
			if (_costingMana)
			{
				if (Time.time >= _nextManaDeductTime)
				{
					ManaDeductTick();
				}
			}
			else
			{
				if (Time.time >= _nextManaRestoreTime)
				{
					ManaRestore();
				}
			}

			EquipSpell();
			CastSpell();

			ManaBar.fillAmount = (float)CurrentMana / (float)ManaCap;
		}

		private void ManaDeductTick()
		{
			if (CurrentMana > 0)
			{
				if (CurrentMana - _manaTickCost < 0)
				{
					CurrentMana = 0;
				}
				else
				{
					CurrentMana -= _manaTickCost;
				}
			}

			if (CurrentMana == 0)
			{
				EndSpell(CurrentSpell);
			}

			_nextManaDeductTime = Time.time + _manaDeductTick;
		}

		private void ManaRestore()
		{
			if (CurrentMana < ManaCap)
			{
				if (CurrentMana + ManaRestoreAmount > ManaCap)
				{
					CurrentMana = ManaCap;
				}
				else
				{
					CurrentMana += ManaRestoreAmount;
				}
			}

			_nextManaRestoreTime = Time.time + ManaRestoreTick;
		}

		private void EquipSpell()
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				if (CurrentSpell != SpellNames.Translocation)
				{
					EndSpell(CurrentSpell);
				}

				CurrentSpell = SpellNames.Translocation;
				InGameManagement.Instance.ChangeOutline(0);
			}

			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				if (CurrentSpell != SpellNames.ShadowSink)
				{
					EndSpell(CurrentSpell);
				}

				CurrentSpell = SpellNames.ShadowSink;
				InGameManagement.Instance.ChangeOutline(1);
			}

			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				if (CurrentSpell != SpellNames.HellfireBlast)
				{
					EndSpell(CurrentSpell);
				}

				CurrentSpell = SpellNames.HellfireBlast;
				InGameManagement.Instance.ChangeOutline(2);
			}

			if (SpellLevel >= 2)
			{
				if (Input.GetKeyDown(KeyCode.Alpha4))
				{
					if (CurrentSpell != SpellNames.Mimic)
					{
						EndSpell(CurrentSpell);
					}

					CurrentSpell = SpellNames.Mimic;
					InGameManagement.Instance.ChangeOutline(3);
				}

				if (Input.GetKeyDown(KeyCode.Alpha5))
				{
					if (CurrentSpell != SpellNames.ExtremeForce)
					{
						EndSpell(CurrentSpell);
					}

					CurrentSpell = SpellNames.ExtremeForce;
					InGameManagement.Instance.ChangeOutline(4);
				}

				if (SpellLevel >= 3)
				{
					if (Input.GetKeyDown(KeyCode.Alpha6))
					{
						if (CurrentSpell != SpellNames.Stasis)
						{
							EndSpell(CurrentSpell);
						}

						CurrentSpell = SpellNames.Stasis;
						InGameManagement.Instance.ChangeOutline(5);
					}

					if (Input.GetKeyDown(KeyCode.Alpha7))
					{
						if (CurrentSpell != SpellNames.Traitor)
						{
							EndSpell(CurrentSpell);
						}

						CurrentSpell = SpellNames.Traitor;
						InGameManagement.Instance.ChangeOutline(6);
					}
				}
			}
		}

		public void EquipSpell(int spellNumber)
		{
			switch (spellNumber)
			{
				case 0:
				{
					if (CurrentSpell != SpellNames.Translocation)
					{
						EndSpell(CurrentSpell);
					}
					CurrentSpell = SpellNames.Translocation;
					break;
				}
				case 1:
				{
					if (CurrentSpell != SpellNames.ShadowSink)
					{
						EndSpell(CurrentSpell);
					}
					CurrentSpell = SpellNames.ShadowSink;
					break;
				}
				case 2:
				{
					if (CurrentSpell != SpellNames.HellfireBlast)
					{
						EndSpell(CurrentSpell);
					}
					CurrentSpell = SpellNames.HellfireBlast;
					break;
				}
				case 3:
				{
					if (CurrentSpell != SpellNames.Mimic)
					{
						EndSpell(CurrentSpell);
					}
					CurrentSpell = SpellNames.Mimic;
					break;
				}
				case 4:
				{
					if (CurrentSpell != SpellNames.ExtremeForce)
					{
						EndSpell(CurrentSpell);
					}
					CurrentSpell = SpellNames.ExtremeForce;
					break;
				}
				case 5:
				{
					if (CurrentSpell != SpellNames.Stasis)
					{
						EndSpell(CurrentSpell);
					}
					CurrentSpell = SpellNames.Stasis;
					break;
				}
				case 6:
				{
					if (CurrentSpell != SpellNames.Traitor)
					{
						EndSpell(CurrentSpell);
					}
					CurrentSpell = SpellNames.Traitor;
					break;
				}
			}
		}

		private void CastSpell()
		{
			if (!_canSpellCast)
			{
				return;
			}
			
			if (Input.GetAxis("Use Item") == 1f && _triggerReleased)
			{
				_triggerReleased = false;
				if (CurrentSpell == SpellNames.Translocation && CheckMana(TranslocationPrefab.ManaCost))
				{
					_currentTranslocationObject = Instantiate(TranslocationPrefab,
						transform.position + new Vector3(5f * transform.localScale.x, 0f, 0f), transform.rotation,
						transform);
					_currentTranslocationObject.Init(_playerController, this);
					EndSpell(SpellNames.ShadowSink);
					EndSpell(SpellNames.Mimic);
				}
				else if (CurrentSpell == SpellNames.ShadowSink && CheckMana(ShadowSinkPrefab.ManaCost))
				{
					if (_currentShadowSinkObject == null && _rigidbody.linearVelocity.x == 0f)
					{
						_currentShadowSinkObject =
							Instantiate(ShadowSinkPrefab, transform.position, transform.rotation);
						_currentShadowSinkObject.Init(_playerController, this);
						DeductMana(_currentShadowSinkObject.ManaCost);
						Hidden = true;
						_costingMana = true;
						_manaDeductTick = _currentShadowSinkObject.ManaDeductTick;
						_manaTickCost = _currentShadowSinkObject.ManaTickCost;
						_nextManaDeductTime = Time.time + _manaDeductTick;
					}

					EndSpell(SpellNames.Mimic);
				}
				else if (CurrentSpell == SpellNames.Mimic && CheckMana(MimicPrefab.ManaCost) && !_playerController.IsSeen)
				{
					if (_currentMimicObject == null)
					{
						Vector3 mimicPosition =
							transform.position + new Vector3(5f * transform.localScale.x, 0f, 0f);
						_currentMimicObject = Instantiate(MimicPrefab, mimicPosition, transform.rotation, transform);
						_currentMimicObject.Init(_playerController, this);
					}

					EndSpell(SpellNames.ShadowSink);
				}
				else if (CurrentSpell == SpellNames.HellfireBlast && CheckMana(HellfireBlastPrefab.ManaCost))
				{
					HellfireBlast hellfireBlastObject = Instantiate(HellfireBlastPrefab, FirePoint.position + HellfireBlastOffset,
						Quaternion.LookRotation(Vector3.forward * (transform.localScale.x / Mathf.Abs(transform.localScale.x))));
					hellfireBlastObject.Init(_playerController, this);
					Destroy(hellfireBlastObject.gameObject, 10f);
					DeductMana(HellfireBlastPrefab.ManaCost);
					EndSpell(SpellNames.ShadowSink);
					EndSpell(SpellNames.Mimic);
				}
				else if (CurrentSpell == SpellNames.ExtremeForce && CheckMana(ExtremeForcePrefab.ManaCost))
				{
					Quaternion spawnRotation = Quaternion.LookRotation(Vector3.forward
					                                                   * (transform.localScale.x
					                                                      / Mathf.Abs(transform.localScale.x)));
					ExtremeForce extremeForceObject = Instantiate(ExtremeForcePrefab, FirePoint.position, spawnRotation);
					extremeForceObject.Init(_playerController, this);
					Destroy(extremeForceObject.gameObject, 3f);

					DeductMana(ExtremeForcePrefab.ManaCost);
					EndSpell(SpellNames.ShadowSink);
					EndSpell(SpellNames.Mimic);
				}
				else if (CurrentSpell == SpellNames.Stasis && CheckMana(StasisPrefab.ManaCost))
				{
					_currentStasisObject = Instantiate(StasisPrefab,
						transform.position + new Vector3(5f * transform.localScale.x, 2.5f, 0f),
						transform.rotation);
					_currentStasisObject.Init(_playerController, this);
					EndSpell(SpellNames.ShadowSink);
					EndSpell(SpellNames.Mimic);
				}
				else if (CurrentSpell == SpellNames.Traitor && CheckMana(TraitorPrefab.ManaCost))
				{
					if (_currentTraitorObject == null)
					{
						_currentTraitorObject = Instantiate(TraitorPrefab,
							transform.position + new Vector3(5f * transform.localScale.x, 0f, 0f),
							transform.rotation, transform);
						_currentTraitorObject.Init(_playerController, this);
					}

					EndSpell(SpellNames.ShadowSink);
					EndSpell(SpellNames.Mimic);
				}
			}

			if (Input.GetAxis("Use Item") == 0f)
			{
				_triggerReleased = true;
				if (CurrentSpell == SpellNames.Mimic && _currentMimicObject != null && !(_currentMimicObject as Mimic).Disguised)
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
		}

		private bool CheckMana(int manaCost)
		{
			return manaCost <= CurrentMana;
		}

		private void DeductMana(int manaCost)
		{
			CurrentMana -= manaCost;
			_nextManaRestoreTime = Time.time + ManaRestoreBegin;
		}

		public void EndSpell(SpellNames spell)
		{
			if (spell == SpellNames.Translocation && _currentTranslocationObject != null)
			{
				Destroy(_currentTranslocationObject.gameObject);
			}

			if (spell == SpellNames.ShadowSink && _currentShadowSinkObject != null)
			{
				_currentShadowSinkObject.EndShadowSink();
				Hidden = false;
				_costingMana = false;
				_manaDeductTick = 0f;
				_manaTickCost = 0;
			}

			if (spell == SpellNames.Mimic && _currentMimicObject != null)
			{
				_currentMimicObject.EndMimic();
				_costingMana = false;
				_manaDeductTick = 0f;
				_manaTickCost = 0;
			}

			if (spell == SpellNames.Traitor && _currentTraitorObject != null)
			{
				_currentTraitorObject.EndTraitor();
			}
		}

		public void SetCanSpellcast(bool _canSpellcast)
		{
			if (!_canSpellcast)
			{
				EndSpell(CurrentSpell);
			}

			_canSpellCast = _canSpellcast;
		}

		public void TranslocationOccured()
		{
			DeductMana(_currentTranslocationObject.ManaCost);
		}

		public void Disguised()
		{
			DeductMana(_currentMimicObject.ManaCost);
			_costingMana = true;
			_manaDeductTick = _currentMimicObject.ManaDeductTick;
			_manaTickCost = _currentMimicObject.ManaTickCost;
			_nextManaDeductTime = Time.time + _manaDeductTick;
		}

		public void StasisOccured()
		{
			DeductMana(_currentStasisObject.ManaCost);
		}

		public void Corrupted()
		{
			DeductMana(_currentTraitorObject.ManaCost);
		}
	}
}
