using System.Collections;
using GameManager;
using Spells;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
	public class InGameManagement : MonoBehaviour
	{

		AudioManager audioManager;
		MenuSwitcher menuSwitcher;
		SpellCasting spellCaster;
		Transform equippedItems;
		Transform controlIcons;
		Text missionText;

		int currentSpellNumber = 0;
		int spellCount;

		bool inCorountine = false;
		bool usingController;

		float nextChangeTime;

		void Start()
		{
			audioManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<AudioManager>();
			menuSwitcher = GameObject.FindGameObjectWithTag("GameController").GetComponent<MenuSwitcher>();
			spellCaster = FindObjectOfType<SpellCasting>();
			spellCount = 3 + (spellCaster.spellLevel - 1) * 2;
			equippedItems = transform.GetChild(1);
			controlIcons = transform.GetChild(2);
			missionText = transform.GetChild(3).GetComponent<Text>();
			usingController = Input.GetJoystickNames().Length > 0;

			ApplyItemsColor(Color.clear);
			for (int i = 0; i < controlIcons.childCount; i++)
			{
				controlIcons.GetChild(i).GetComponent<Image>().color = Color.clear;
				controlIcons.GetChild(i).GetChild(0).GetComponent<Text>().color = Color.clear;
			}

			string sceneName = SceneManager.GetActiveScene().name;
			if (sceneName.Equals("Level 1"))
			{
				LoadMissionText("Find the Ledger");
			}
			if (sceneName.Equals("Level 2"))
			{
				LoadMissionText("Find the Golden Chalice");
			}
			if (sceneName.Equals("Level 3"))
			{
				LoadMissionText("Break out of Prison");
			}

			if (usingController)
			{
				controlIcons.GetChild(0).GetChild(0).GetComponent<Text>().text = "X";
				controlIcons.GetChild(1).GetChild(0).GetComponent<Text>().text = "RB";
				controlIcons.GetChild(2).GetChild(0).GetComponent<Text>().text = "RT";
			}
		}

		void Update()
		{
			if (Input.GetButtonDown("Change Item"))
			{
				StartCoroutine(Transition(2, Color.clear, Color.white));
			}
			else if (Input.GetButton("Change Item"))
			{
				float controllerX = Input.GetAxis("Mouse X");
				if (nextChangeTime <= Time.time && Mathf.Abs(controllerX) > 0.19f)
				{
					audioManager.PlaySound("Swish");
					currentSpellNumber = (spellCount + (currentSpellNumber + (int)(controllerX / Mathf.Abs(controllerX)))) % spellCount;
					ChangeOutline(currentSpellNumber);
					nextChangeTime = Time.time + 0.15f;
				}
			}
			else if (Input.GetButtonUp("Change Item"))
			{
				StartCoroutine(Transition(2, Color.white, Color.clear));
				spellCaster.EquipSpell(currentSpellNumber);
			}

			if (Input.GetButtonDown("Cancel"))
			{
				audioManager.PlaySound("Select");
				audioManager.PauseSound("Alarm", true);
				if (!usingController)
				{
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
				}

				missionText.color = Color.clear;
				Time.timeScale = 0;
				menuSwitcher.LoadMenu(1);
			}
		}

		public void ChangeOutline(int newSpell)
		{
			for (int i = 0; i < equippedItems.childCount; i++)
			{
				equippedItems.GetChild(i).GetComponent<Outline>().enabled = newSpell == i;
			}

			if (!inCorountine && !usingController)
			{
				StartCoroutine(ItemsAnimation(0, 1f));
			}
		}

		public void LoadUseIcon(bool load)
		{
			if (load)
			{
				StartCoroutine(Transition(0, Color.clear, Color.white));
			}
			else
			{
				StartCoroutine(Transition(0, Color.white, Color.clear));
			}
		}

		public void LoadAttackIcons(bool load)
		{
			if (load)
			{
				StartCoroutine(Transition(1, Color.clear, Color.white));
			}
			else
			{
				StartCoroutine(Transition(1, Color.white, Color.clear));
			}
		}

		public void LoadMissionText(string newMission)
		{
			missionText.text = newMission;
			StartCoroutine(ItemsAnimation(1, 3f));
		}

		void ApplyItemsColor(Color color)
		{
			for (int i = 0; i < equippedItems.childCount; i++)
			{
				equippedItems.GetChild(i).GetComponent<Image>().color = color;
				equippedItems.GetChild(i).GetChild(0).GetComponent<Image>().color = color;
			}
		}

		IEnumerator Transition(int group, Color start, Color end)
		{
			float startTime = Time.time;
			float transitionTime = 0.1f;
			float percent = 0;
			while (percent < 1)
			{
				percent = (Time.time - startTime) / transitionTime;
				if (group == 0)
				{
					controlIcons.GetChild(0).GetComponent<Image>().color = Color.Lerp(start, end, percent);
					controlIcons.GetChild(0).GetChild(0).GetComponent<Text>().color = Color.Lerp(start, end, percent);
				}
				else if (group == 1)
				{
					controlIcons.GetChild(1).GetComponent<Image>().color = Color.Lerp(start, end, percent);
					controlIcons.GetChild(1).GetChild(0).GetComponent<Text>().color = Color.Lerp(start, end, percent);
					controlIcons.GetChild(2).GetComponent<Image>().color = Color.Lerp(start, end, percent);
					controlIcons.GetChild(2).GetChild(0).GetComponent<Text>().color = Color.Lerp(start, end, percent);
				}
				else if (group == 2)
				{
					ApplyItemsColor(Color.Lerp(start, end, percent));
				}

				yield return null;
			}
		}

		IEnumerator ItemsAnimation(int group, float waitTime)
		{
			if (group != 1)
			{
				inCorountine = true;
			}
			
			float percent = 0f;
			float transitionTime = 0.1f;
			float startTime = Time.time;
			while (percent < 1)
			{
				percent = (Time.time - startTime) / transitionTime;
				if (group == 0)
				{
					ApplyItemsColor(Color.Lerp(Color.clear, Color.white, percent));
				}
				else if (group == 1)
				{
					missionText.color = Color.Lerp(Color.clear, Color.white, percent);
				}
				
				yield return null;
			}

			yield return new WaitForSeconds(waitTime);
			startTime = Time.time;
			while (percent > 0)
			{
				percent = 1 - (Time.time - startTime) / transitionTime;
				if (group == 0)
				{
					ApplyItemsColor(Color.Lerp(Color.clear, Color.white, percent));
				}
				else if (group == 1)
				{
					missionText.color = Color.Lerp(Color.clear, Color.white, percent);
				}
				
				yield return null;
			}

			inCorountine = false;
		}
	}
}
