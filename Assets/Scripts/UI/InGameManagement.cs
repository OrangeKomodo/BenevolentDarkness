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
		
		[SerializeField] private string StartingMissionText;
		[SerializeField] private AbilitiesAssetSet[] AbilitiesAssets;
		[SerializeField] private ActionsAssetSet UseAssets;
		[SerializeField] private ActionsAssetSet SubdueAssets;
		[SerializeField] private ActionsAssetSet KillAssets;
		[SerializeField] private Text MissionText;
		
		private SpellCasting _spellCaster;

		private int _currentSpellNumber = 0;
		private int _spellCount;

		private bool _inCorountine = false;
		private bool _usingController;

		private float _nextChangeTime;

		private void Start()
		{
			_spellCaster = GameObject.FindGameObjectWithTag("Player").GetComponent<SpellCasting>();
			_spellCount = new int[] { 3, 5, 7 }[_spellCaster.SpellLevel - 1];
			_usingController = Input.GetJoystickNames().Length > 0;

			ApplyAbilitiesColor(Color.clear);
			ApplyActionsColor(UseAssets, Color.clear);
			ApplyActionsColor(SubdueAssets, Color.clear);
			ApplyActionsColor(KillAssets, Color.clear);
			
			LoadMissionText(StartingMissionText);

			if (_usingController)
			{
				UseAssets.Text.text = "X";
				SubdueAssets.Text.text = "RB";
				KillAssets.Text.text = "RT";
			}
		}

		private void Update()
		{
			if (Input.GetButtonDown("Change Item"))
			{
				StartCoroutine(Transition(2, Color.clear, Color.white));
			}
			else if (Input.GetButton("Change Item"))
			{
				float controllerX = Input.GetAxis("Mouse X");
				if (_nextChangeTime <= Time.time && Mathf.Abs(controllerX) > 0.19f)
				{
					AudioManager.Instance.PlaySound("Swish");
					_currentSpellNumber = (_spellCount + _currentSpellNumber + (int)(controllerX / Mathf.Abs(controllerX))) % _spellCount;
					ChangeOutline(_currentSpellNumber);
					_nextChangeTime = Time.time + 0.15f;
				}
			}
			else if (Input.GetButtonUp("Change Item"))
			{
				StartCoroutine(Transition(2, Color.white, Color.clear));
				_spellCaster.EquipSpell(_currentSpellNumber);
			}

			if (Input.GetButtonDown("Cancel"))
			{
				AudioManager.Instance.PlaySound("Select");
				AudioManager.Instance.PauseSound("Alarm", true);
				if (!_usingController)
				{
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
				}

				MissionText.color = Color.clear;
				Time.timeScale = 0;
				MenuSwitcher.Instance.LoadMenu(1);
			}
		}

		public void ChangeOutline(int newSpell)
		{
			for (int i = 0; i < AbilitiesAssets.Length; i++)
			{
				AbilitiesAssets[i].Outline.enabled = newSpell == i;
			}

			if (!_inCorountine && !_usingController)
			{
				StartCoroutine(AbilitiesAnimation(0, 1f));
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

		public void LoadMissionText(string newMissionText)
		{
			if (string.IsNullOrWhiteSpace(newMissionText))
			{
				return;
			}
			
			MissionText.text = newMissionText;
			StartCoroutine(AbilitiesAnimation(1, 3f));
		}

		private void ApplyAbilitiesColor(Color color)
		{
			for (int i = 0; i < _spellCount; i++)
			{
				AbilitiesAssets[i].Image.color = color;
				AbilitiesAssets[i].BackgroundImage.color = color;
			}
		}

		private void ApplyActionsColor(ActionsAssetSet actionAssetSet, Color color)
		{
			actionAssetSet.Image.color = color;
			actionAssetSet.Text.color = color;
		}

		private IEnumerator Transition(int group, Color start, Color end)
		{
			float startTime = Time.time;
			float transitionTime = 0.1f;
			float percent = 0;
			while (percent < 1)
			{
				percent = (Time.time - startTime) / transitionTime;
				
				Color lerpedColor = Color.Lerp(start, end, percent);
				
				if (group == 0)
				{
					ApplyActionsColor(UseAssets, lerpedColor);
				}
				else if (group == 1)
				{
					ApplyActionsColor(SubdueAssets, lerpedColor);
					ApplyActionsColor(KillAssets, lerpedColor);
				}
				else if (group == 2)
				{
					ApplyAbilitiesColor(lerpedColor);
				}

				yield return null;
			}
		}

		private IEnumerator AbilitiesAnimation(int group, float waitTime)
		{
			if (group != 1)
			{
				_inCorountine = true;
			}
			
			float percent = 0f;
			float transitionTime = 0.1f;
			float startTime = Time.time;
			while (percent < 1)
			{
				percent = (Time.time - startTime) / transitionTime;
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

			yield return new WaitForSeconds(waitTime);
			
			startTime = Time.time;
			while (percent > 0)
			{
				percent = 1 - (Time.time - startTime) / transitionTime;
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

			_inCorountine = false;
		}
	}
}
