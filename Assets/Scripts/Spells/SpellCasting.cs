using Player;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Spells
{
	public class SpellCasting : MonoBehaviour
	{
		public int spellLevel = 1;

		public Translocation translocationPrefab;
		public ShadowSink shadowSinkPrefab;
		public Mimic mimicPrefab;
		public Stasis stasisPrefab;
		public HellfireBlast hellfireBlastPrefab;
		public ExtremeForce extremeForcePrefab;
		public Traitor traitorPrefab;
		public Transform firePoint;

		Translocation currentTranslocationObject;
		ShadowSink currentShadowSinkObject;
		Mimic currentMimicObject;
		Stasis currentStasisObject;
		Traitor currentTraitorObject;

		public Transform inGameHUD;
		public Image manaBar;

		public SpellNames currentSpell;
		public bool hidden;

		public int manaCap;
		public int currentMana;
		public float manaRestoreTick;
		public int manaRestoreAmount;
		float manaRestoreBegin = 3f;
		float nextManaRestoreTime;

		bool canSpellcast = true;

		bool costingMana;
		float manaDeductTick;
		int manaTickCost;
		float nextManaDeductTime;

		Vector3 shadowSinkPlayerPosition;

		bool triggerReleased = true;

		public enum SpellNames
		{
			translocation,
			shadowSink,
			mimic,
			stasis,
			hellfireBlast,
			extremeForce,
			traitor
		};

		void Update()
		{
			if (costingMana)
			{
				if (Time.time >= nextManaDeductTime)
				{
					ManaDeductTick();
				}
			}
			else
			{
				if (Time.time >= nextManaRestoreTime)
				{
					ManaRestore();
				}
			}

			EquipSpell();
			CastSpell();

			manaBar.fillAmount = (float)currentMana / (float)manaCap;
		}

		void ManaDeductTick()
		{
			if (currentMana > 0)
			{
				if (currentMana - manaTickCost < 0)
				{
					currentMana = 0;
				}
				else
				{
					currentMana -= manaTickCost;
				}
			}

			if (currentMana == 0)
			{
				EndSpell(currentSpell);
			}

			nextManaDeductTime = Time.time + manaDeductTick;
		}

		void ManaRestore()
		{
			if (currentMana < manaCap)
			{
				if (currentMana + manaRestoreAmount > manaCap)
				{
					currentMana = manaCap;
				}
				else
				{
					currentMana += manaRestoreAmount;
				}
			}

			nextManaRestoreTime = Time.time + manaRestoreTick;
		}

		void EquipSpell()
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				if (currentSpell != SpellNames.translocation)
				{
					EndSpell(currentSpell);
				}

				currentSpell = SpellNames.translocation;
				inGameHUD.GetComponent<InGameManagement>().ChangeOutline(0);
			}

			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				if (currentSpell != SpellNames.shadowSink)
				{
					EndSpell(currentSpell);
				}

				currentSpell = SpellNames.shadowSink;
				inGameHUD.GetComponent<InGameManagement>().ChangeOutline(1);
			}

			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				if (currentSpell != SpellNames.hellfireBlast)
				{
					EndSpell(currentSpell);
				}

				currentSpell = SpellNames.hellfireBlast;
				inGameHUD.GetComponent<InGameManagement>().ChangeOutline(2);
			}

			if (spellLevel >= 2)
			{
				if (Input.GetKeyDown(KeyCode.Alpha4))
				{
					if (currentSpell != SpellNames.mimic)
					{
						EndSpell(currentSpell);
					}

					currentSpell = SpellNames.mimic;
					inGameHUD.GetComponent<InGameManagement>().ChangeOutline(3);
				}

				if (Input.GetKeyDown(KeyCode.Alpha5))
				{
					if (currentSpell != SpellNames.extremeForce)
					{
						EndSpell(currentSpell);
					}

					currentSpell = SpellNames.extremeForce;
					inGameHUD.GetComponent<InGameManagement>().ChangeOutline(4);
				}

				if (spellLevel >= 3)
				{
					if (Input.GetKeyDown(KeyCode.Alpha6))
					{
						if (currentSpell != SpellNames.stasis)
						{
							EndSpell(currentSpell);
						}

						currentSpell = SpellNames.stasis;
						inGameHUD.GetComponent<InGameManagement>().ChangeOutline(5);
					}

					if (Input.GetKeyDown(KeyCode.Alpha7))
					{
						if (currentSpell != SpellNames.traitor)
						{
							EndSpell(currentSpell);
						}

						currentSpell = SpellNames.traitor;
						inGameHUD.GetComponent<InGameManagement>().ChangeOutline(6);
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
					if (currentSpell != SpellNames.translocation)
					{
						EndSpell(currentSpell);
					}
					currentSpell = SpellNames.translocation;
					break;
				}
				case 1:
				{
					if (currentSpell != SpellNames.shadowSink)
					{
						EndSpell(currentSpell);
					}
					currentSpell = SpellNames.shadowSink;
					break;
				}
				case 2:
				{
					if (currentSpell != SpellNames.hellfireBlast)
					{
						EndSpell(currentSpell);
					}
					currentSpell = SpellNames.hellfireBlast;
					break;
				}
				case 3:
				{
					if (currentSpell != SpellNames.mimic)
					{
						EndSpell(currentSpell);
					}
					currentSpell = SpellNames.mimic;
					break;
				}
				case 4:
				{
					if (currentSpell != SpellNames.extremeForce)
					{
						EndSpell(currentSpell);
					}
					currentSpell = SpellNames.extremeForce;
					break;
				}
				case 5:
				{
					if (currentSpell != SpellNames.stasis)
					{
						EndSpell(currentSpell);
					}
					currentSpell = SpellNames.stasis;
					break;
				}
				case 6:
				{
					if (currentSpell != SpellNames.traitor)
					{
						EndSpell(currentSpell);
					}
					currentSpell = SpellNames.traitor;
					break;
				}
			}
		}

		void CastSpell()
		{
			if (canSpellcast)
			{
				if (Input.GetAxis("Use Item") == 1f && triggerReleased)
				{
					triggerReleased = false;
					if (currentSpell == SpellNames.translocation && CheckMana(translocationPrefab.manaCost))
					{
						currentTranslocationObject = Instantiate(translocationPrefab,
							transform.position + new Vector3(5f * transform.localScale.x, 0f, 0f), transform.rotation,
							transform);
						EndSpell(SpellNames.shadowSink);
						EndSpell(SpellNames.mimic);
					}
					else if (currentSpell == SpellNames.shadowSink && CheckMana(shadowSinkPrefab.manaCost))
					{
						if (currentShadowSinkObject == null && gameObject.GetComponent<Rigidbody2D>().velocity.x == 0f)
						{
							currentShadowSinkObject =
								Instantiate(shadowSinkPrefab, transform.position, transform.rotation);
							DeductMana(currentShadowSinkObject.manaCost);
							hidden = true;
							costingMana = true;
							manaDeductTick = currentShadowSinkObject.manaDeductTick;
							manaTickCost = currentShadowSinkObject.manaTickCost;
							nextManaDeductTime = Time.time + manaDeductTick;
						}

						EndSpell(SpellNames.mimic);
					}
					else if (currentSpell == SpellNames.mimic && CheckMana(mimicPrefab.manaCost)
					                                          && !GetComponent<PlayerInfo>().isSeen)
					{
						if (currentMimicObject == null)
						{
							Vector3 mimicPosition =
								transform.position + new Vector3(5f * transform.localScale.x, 0f, 0f);
							currentMimicObject = Instantiate(mimicPrefab, mimicPosition, transform.rotation, transform);
						}

						EndSpell(SpellNames.shadowSink);
					}
					else if (currentSpell == SpellNames.hellfireBlast &&
					         CheckMana(hellfireBlastPrefab.manaCost))
					{
						GameObject hellfireBlastObject = Instantiate(hellfireBlastPrefab.gameObject, firePoint.position,
							Quaternion.LookRotation(Vector3.forward * (transform.localScale.x / Mathf.Abs(transform.localScale.x))));
						Destroy(hellfireBlastObject, 10f);
						DeductMana(hellfireBlastPrefab.manaCost);
						EndSpell(SpellNames.shadowSink);
						EndSpell(SpellNames.mimic);
					}
					else if (currentSpell == SpellNames.extremeForce && CheckMana(extremeForcePrefab.manaCost))
					{
						Quaternion spawnRotation = Quaternion.LookRotation(Vector3.forward
						                                                   * (transform.localScale.x
						                                                      / Mathf.Abs(transform.localScale.x)));
						GameObject extremeForceObject = Instantiate(extremeForcePrefab.gameObject, firePoint.position,
							spawnRotation);
						Destroy(extremeForceObject, 3f);

						DeductMana(extremeForcePrefab.manaCost);
						EndSpell(SpellNames.shadowSink);
						EndSpell(SpellNames.mimic);
					}
					else if (currentSpell == SpellNames.stasis && CheckMana(stasisPrefab.manaCost))
					{
						currentStasisObject = Instantiate(stasisPrefab,
							transform.position + new Vector3(5f * transform.localScale.x, 2.5f, 0f),
							transform.rotation);
						EndSpell(SpellNames.shadowSink);
						EndSpell(SpellNames.mimic);
					}
					else if (currentSpell == SpellNames.traitor && CheckMana(traitorPrefab.manaCost))
					{
						if (currentTraitorObject == null)
						{
							currentTraitorObject = Instantiate(traitorPrefab,
								transform.position + new Vector3(5f * transform.localScale.x, 0f, 0f),
								transform.rotation, transform);
						}

						EndSpell(SpellNames.shadowSink);
						EndSpell(SpellNames.mimic);
					}
				}

				if (Input.GetAxis("Use Item") == 0f)
				{
					triggerReleased = true;
					if (currentSpell == SpellNames.mimic && currentMimicObject != null
					                                     && !(currentMimicObject as Mimic).disguised)
					{
						EndSpell(SpellNames.mimic);
						Destroy(currentMimicObject.gameObject);
					}
					else if (currentSpell == SpellNames.traitor && currentTraitorObject != null)
					{
						EndSpell(SpellNames.traitor);
						Destroy(currentTraitorObject.gameObject);
					}
				}
			}
		}

		bool CheckMana(int manaCost)
		{
			return manaCost <= currentMana;
		}

		void DeductMana(int manaCost)
		{
			currentMana -= manaCost;
			nextManaRestoreTime = Time.time + manaRestoreBegin;
		}

		public void EndSpell(SpellNames spell)
		{
			if (spell == SpellNames.translocation && currentTranslocationObject != null)
			{
				if (currentTranslocationObject.translocationOccured)
				{
					DeductMana(currentTranslocationObject.manaCost);
				}

				Destroy(currentTranslocationObject.gameObject);
			}

			if (spell == SpellNames.shadowSink && currentShadowSinkObject != null)
			{
				currentShadowSinkObject.EndShadowSink();
				hidden = false;
				costingMana = false;
				manaDeductTick = 0f;
				manaTickCost = 0;
			}

			if (spell == SpellNames.mimic && currentMimicObject != null)
			{
				currentMimicObject.EndMimic();
				costingMana = false;
				manaDeductTick = 0f;
				manaTickCost = 0;
			}

			if (spell == SpellNames.traitor && currentTraitorObject != null)
			{
				currentTraitorObject.EndTraitor();
			}
		}

		public void SetCanSpellcast(bool _canSpellcast)
		{
			if (!_canSpellcast)
			{
				EndSpell(currentSpell);
			}

			canSpellcast = _canSpellcast;
		}

		public void Disguised()
		{
			DeductMana(currentMimicObject.manaCost);
			costingMana = true;
			manaDeductTick = currentMimicObject.manaDeductTick;
			manaTickCost = currentMimicObject.manaTickCost;
			nextManaDeductTime = Time.time + manaDeductTick;
		}

		public void StasisOccured()
		{
			DeductMana(currentStasisObject.manaCost);
		}

		public void Corrupted()
		{
			DeductMana(currentTraitorObject.manaCost);
		}
	}
}
