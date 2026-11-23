using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Player.Components
{
	// AI - Manages player health, regeneration, and health bar display.
	public class PlayerHealthManager : MonoBehaviour
	{
		#region Fields

		[SerializeField] private float _startingHealth = 100f;
		[SerializeField] private Image _healthBar;

		private const float HealthRegenTime = 3f;
		private const float HealthRegenTick = 0.5f;
		private const float HealthRegenPerTick = 2f;

		private float _currentHealth;
		private float _healthRegenBegin;
		private bool _isDead = false;

		private Coroutine _regenCoroutine;

		#endregion

		#region Properties

		public float CurrentHealth => _currentHealth;
		public float StartingHealth => _startingHealth;
		public bool IsDead => _isDead;

		#endregion

		#region Initialization Functions

		private void Start()
		{
			_currentHealth = _startingHealth;
			UpdateHealthBar();
		}

		#endregion

		#region Health Management Functions

		// AI - Apply damage to the player and restart regeneration timer.
		public void TakeDamage(float damage)
		{
			if (_isDead)
			{
				return;
			}

			_currentHealth -= damage;
			_currentHealth = Mathf.Max(_currentHealth, 0f);
			UpdateHealthBar();
			_healthRegenBegin = Time.time + HealthRegenTime;

			if (_currentHealth <= 0f)
			{
				_isDead = true;
			}
		}

		// AI - Heal the player up to maximum health.
		public void Heal(float healAmount)
		{
			if (_isDead)
			{
				return;
			}

			_currentHealth = Mathf.Min(_currentHealth + healAmount, _startingHealth);
			UpdateHealthBar();
		}

		// AI - Start the automatic health regeneration coroutine.
		public void StartRegeneration()
		{
			if (_regenCoroutine != null)
			{
				StopCoroutine(_regenCoroutine);
			}

			_regenCoroutine = StartCoroutine(RegenerateHealth());
		}

		// AI - Stop the automatic health regeneration.
		public void StopRegeneration()
		{
			if (_regenCoroutine != null)
			{
				StopCoroutine(_regenCoroutine);
				_regenCoroutine = null;
			}
		}

		// AI - Coroutine that gradually restores health over time.
		private IEnumerator RegenerateHealth()
		{
			while (!_isDead)
			{
				// AI - Wait for the regeneration delay after last damage.
				if (Time.time < _healthRegenBegin)
				{
					yield return new WaitForSeconds(HealthRegenTick);
					continue;
				}

				// AI - If not at full health, regenerate.
				if (_currentHealth < _startingHealth)
				{
					Heal(HealthRegenPerTick);
				}

				yield return new WaitForSeconds(HealthRegenTick);
			}
		}

		// AI - Revive the player (used for quick load).
		public void Revive(float health)
		{
			_isDead = false;
			_currentHealth = health;
			UpdateHealthBar();
			StartRegeneration();
		}

		#endregion

		#region UI Functions

		// AI - Update the health bar fill amount.
		private void UpdateHealthBar()
		{
			if (_healthBar != null)
			{
				_healthBar.fillAmount = _currentHealth / _startingHealth;
			}
		}

		#endregion
	}
}
