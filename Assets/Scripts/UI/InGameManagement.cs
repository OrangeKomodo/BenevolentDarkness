using System;
using System.Collections;
using GameManager;
using Spells;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class InGameManagement : Singleton<InGameManagement>
	{
		#region Structs

		[Serializable]
		private struct AbilitiesAssetSet
		{
			public Image Image;
			public Image BackgroundImage;
			public Outline Outline;
		}
		
		[Serializable]
		private struct ActionsAssetSet
		{
			public Image Image;
			public Text Text;
		}

		#endregion
		
		#region Fields

		[SerializeField] private string StartingMissionText;
		[SerializeField] private AbilitiesAssetSet[] AbilitiesAssets;
		[SerializeField] private ActionsAssetSet UseAssets;
		[SerializeField] private ActionsAssetSet SubdueAssets;
		[SerializeField] private ActionsAssetSet KillAssets;
		[SerializeField] private Text MissionText;
		
		private SpellCasting _spellCaster;

		private int _currentSpellNumber = 0;
		private int _spellCount;

		private bool _inCoroutine = false;
		private float _nextChangeTime;

		#endregion

		#region Initialization Functions

		private void Start()
		{
			_spellCaster = GameObject.FindGameObjectWithTag(GameConstants.PLAYER_TAG).GetComponent<SpellCasting>();
			_spellCount = new int[] { 3, 5, 7 }[_spellCaster.SpellLevel - 1];

			ApplyAbilitiesColor(Color.clear);
			ApplyActionsColor(UseAssets, Color.clear);
			ApplyActionsColor(SubdueAssets, Color.clear);
			ApplyActionsColor(KillAssets, Color.clear);
			
			LoadMissionText(StartingMissionText);
		}

		#endregion

		#region Update Functions

		private void Update()
		{
			HandleSpellSelection();
			HandlePauseInput();
		}

		// AI - Handle spell cycling with controller/gamepad.
		private void HandleSpellSelection()
		{
			if (Input.GetButtonDown("Change Item"))
			{
				StartCoroutine(Transition(2, Color.clear, Color.white));
			}
			else if (Input.GetButton("Change Item"))
			{
				HandleSpellCycling();
			}
			else if (Input.GetButtonUp("Change Item"))
			{
				StartCoroutine(Transition(2, Color.white, Color.clear));
				_spellCaster.EquipSpell(_currentSpellNumber);
			}

			// AI - Show abilities animation when not in coroutine and not using gamepad.
			if (!_inCoroutine && !InputManager.UsingGamepad)
			{
				StartCoroutine(AbilitiesAnimation(0, 1f));
			}
		}

		// AI - Cycle through spells based on controller input.
		private void HandleSpellCycling()
		{
			float controllerX = Input.GetAxis("Mouse X");
			if (_nextChangeTime > Time.time || Mathf.Abs(controllerX) <= 0.19f)
			{
				return;
			}

			AudioManager.Instance.PlaySound("Swish");
			int direction = (int)(controllerX / Mathf.Abs(controllerX));
			_currentSpellNumber = (_spellCount + _currentSpellNumber + direction) % _spellCount;
			ChangeOutline(_currentSpellNumber);
			_nextChangeTime = Time.time + 0.15f;
		}

		// AI - Handle pause button input.
		private void HandlePauseInput()
		{
			if (!Input.GetButtonDown("Cancel"))
			{
				return;
			}

			AudioManager.Instance.PlaySound("Select");
			AudioManager.Instance.PauseSound("Alarm", true);
			
			if (!InputManager.UsingGamepad)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}

			MissionText.color = Color.clear;
			Time.timeScale = 0;
			MenuSwitcher.Instance.LoadMenu(1);
		}

		#endregion

		#region UI Update Functions

		// AI - Change which spell is outlined in the UI.
		public void ChangeOutline(int newSpell)
		{
			for (int i = 0; i < AbilitiesAssets.Length; i++)
			{
				AbilitiesAssets[i].Outline.enabled = newSpell == i;
			}
		}

		// AI - Show or hide the use icon.
		public void LoadUseIcon(bool load)
		{
			Color startColor = load ? Color.clear : Color.white;
			Color endColor = load ? Color.white : Color.clear;
			StartCoroutine(Transition(0, startColor, endColor));
		}

		// AI - Show or hide attack icons.
		public void LoadAttackIcons(bool load)
		{
			Color startColor = load ? Color.clear : Color.white;
			Color endColor = load ? Color.white : Color.clear;
			StartCoroutine(Transition(1, startColor, endColor));
		}

		// AI - Display mission text with fade animation.
		public void LoadMissionText(string newMissionText)
		{
			if (string.IsNullOrWhiteSpace(newMissionText))
			{
				return;
			}
			
			MissionText.text = newMissionText;
			StartCoroutine(AbilitiesAnimation(1, 3f));
		}

		#endregion

		#region Color Management Functions

		// AI - Apply a color to all ability UI elements.
		private void ApplyAbilitiesColor(Color color)
		{
			for (int i = 0; i < _spellCount; i++)
			{
				AbilitiesAssets[i].Image.color = color;
				AbilitiesAssets[i].BackgroundImage.color = color;
			}
		}

		// AI - Apply a color to an action asset set.
		private void ApplyActionsColor(ActionsAssetSet actionAssetSet, Color color)
		{
			actionAssetSet.Image.color = color;
			actionAssetSet.Text.color = color;
		}

		#endregion

		#region Animation Functions

		// AI - Transition UI element colors from start to end.
		private IEnumerator Transition(int group, Color start, Color end)
		{
			float startTime = Time.time;
			float transitionTime = 0.1f;
			float percent = 0;
			
			while (percent < 1)
			{
				percent = (Time.time - startTime) / transitionTime;
				Color lerpedColor = Color.Lerp(start, end, percent);
				
				switch (group)
				{
					case 0:
						ApplyActionsColor(UseAssets, lerpedColor);
						break;
					case 1:
						ApplyActionsColor(SubdueAssets, lerpedColor);
						ApplyActionsColor(KillAssets, lerpedColor);
						break;
					case 2:
						ApplyAbilitiesColor(lerpedColor);
						break;
				}

				yield return null;
			}
		}

		// AI - Animate abilities or mission text with fade in, wait, fade out.
		private IEnumerator AbilitiesAnimation(int group, float waitTime)
		{
			if (group != 1)
			{
				_inCoroutine = true;
			}
			
			// AI - Fade in
			yield return FadeElement(group, 0f, 1f, 0.1f);

			// AI - Wait
			yield return new WaitForSeconds(waitTime);
			
			// AI - Fade out
			yield return FadeElement(group, 1f, 0f, 0.1f);

			_inCoroutine = false;
		}

		// AI - Fade an element from start to end percent over duration.
		private IEnumerator FadeElement(int group, float startPercent, float endPercent, float duration)
		{
			float startTime = Time.time;
			float elapsed = 0f;
			
			while (elapsed < duration)
			{
				elapsed = Time.time - startTime;
				float percent = Mathf.Lerp(startPercent, endPercent, elapsed / duration);
				
				if (group == 0)
				{
					ApplyAbilitiesColor(Color.Lerp(Color.clear, Color.white, percent));
				}
				else if (group == 1)
				{
					MissionText.color = Color.Lerp(Color.clear, Color.white, percent);
				}
				
				yield return null;
			}
		}

		#endregion
	}
}
