using AI.Guard;
using DamageSystem;
using UnityEngine;

namespace Player
{
	public class PlayerAttack : MonoBehaviour
	{
		#region Fields

		public int Damage;

		public Transform AttackPos;
		public float AttackRange;

		public float AttackCooldown;
		public LayerMask WhatAreEnemies;

		private PlayerController _playerController;

		private float _currentAttackCooldown;
		private bool _seesBackside = false;

		private bool _attackButtonPressed = true;

		#endregion

		#region Initialization Functions

		private void Start()
		{
			_playerController = GetComponent<PlayerController>();
		}

		#endregion

		#region Update Functions

		private void Update()
		{
			HandleAttack();
			HandleAttackCooldown();
			HandleAttackButtonReleased();
		}

		#endregion

		#region Attack Functions

		// AI - Process player attack logic including backside detection.
		private void HandleAttack()
		{
			if (!_playerController.CanAttack)
			{
				return;
			}

			CheckForBackside();
			PerformAttack();
		}

		// AI - Raycast to detect if the player can see an enemy's backside for a stealth attack.
		private void CheckForBackside()
		{
			Vector2 playerPosition = transform.position;
			Vector2 attackDirection = transform.right * (transform.localScale.x / Mathf.Abs(transform.localScale.x));

			Debug.DrawRay(playerPosition, attackDirection * AttackRange, Color.magenta);
			RaycastHit2D playerRayHit = Physics2D.Raycast(playerPosition, attackDirection, AttackRange, WhatAreEnemies);

			if (playerRayHit.collider == null || !playerRayHit.collider.name.Equals(GameManager.GameConstants.BACKSIDE_NAME))
			{
				HandleBacksideNotSeen();
				return;
			}

			HandleBacksideSeen(playerRayHit);
		}

		// AI - Hide attack icons when the player no longer sees an enemy's backside.
		private void HandleBacksideNotSeen()
		{
			if (!_seesBackside)
			{
				return;
			}

			_playerController.LoadAttackIcons(false);
			_seesBackside = false;
		}

		// AI - Show attack icons and handle stealth attack inputs when seeing an enemy's backside.
		private void HandleBacksideSeen(RaycastHit2D playerRayHit)
		{
			if (!_seesBackside)
			{
				_playerController.LoadAttackIcons(true);
				_seesBackside = true;
			}

			if (Input.GetButtonDown("Subdue"))
			{
				ExecuteSubdue(playerRayHit);
				return;
			}

			// AI - Note: This backstab attack intentionally ignores cooldown. Stealth kills should be instant.
			if (Input.GetAxis("Attack") >= 1f)
			{
				ExecuteBackstab(playerRayHit);
			}
		}

		// AI - Knock out an enemy from behind.
		private void ExecuteSubdue(RaycastHit2D playerRayHit)
		{
			_playerController.PlaySound("Swipe");
			playerRayHit.collider.GetComponentInParent<Guard>().OnGuardUnconscious();
			_playerController.Attack(1);
		}

		// AI - Instantly kill an enemy from behind.
		private void ExecuteBackstab(RaycastHit2D playerRayHit)
		{
			_playerController.PlaySound("Swipe");
			playerRayHit.collider.GetComponentInParent<LivingEntity>().TakeHit(GameManager.GameConstants.INSTANT_KILL_DAMAGE);
			_playerController.Attack(0);
			_attackButtonPressed = false;
			_currentAttackCooldown = AttackCooldown;
		}

		// AI - Execute a normal frontal attack on nearby enemies.
		private void PerformAttack()
		{
			if (Input.GetAxis("Attack") < 1f)
			{
				return;
			}

			if (_currentAttackCooldown > 0f)
			{
				return;
			}
			
			if (_attackButtonPressed)
			{
				return;
			}
			
			Collider2D[] enemiesToDamage = Physics2D.OverlapCircleAll(AttackPos.position, AttackRange, WhatAreEnemies);
			
			for (int enemyColliderIndex = 0; enemyColliderIndex < enemiesToDamage.Length; enemyColliderIndex++)
			{
				// AI - Ignore Backside and Player colliders that might be found.
				if (enemiesToDamage[enemyColliderIndex].name.Equals(GameManager.GameConstants.BACKSIDE_NAME) || 
				    enemiesToDamage[enemyColliderIndex].tag.Equals(GameManager.GameConstants.PLAYER_TAG))
				{
					continue;
				}
					
				enemiesToDamage[enemyColliderIndex].GetComponent<LivingEntity>().TakeHit(Damage);
			}
			
			_playerController.PlaySound("Swipe");
			_playerController.Attack(0);
			
			_currentAttackCooldown = AttackCooldown;
			_attackButtonPressed = true;
		}

		#endregion

		#region Cooldown Functions

		// AI - Tick down the attack cooldown timer.
		private void HandleAttackCooldown()
		{
			if (_currentAttackCooldown <= 0f)
			{
				return;
			}
			
			_currentAttackCooldown -= Time.deltaTime;
		}

		#endregion

		#region Input Tracking Functions

		// AI - Track when the attack button is released to prevent holding for continuous attacks.
		private void HandleAttackButtonReleased()
		{
			if (Input.GetAxis("Attack") >= 1f)
			{
				_attackButtonPressed = true;
				return;
			}
			
			if (Input.GetAxis("Attack") <= 0.8f)
			{
				_attackButtonPressed = false;
				return;
			}
		}

		#endregion

		#region Debug Functions

		// AI - Optional: Draw the attack sphere for debugging.
		/*private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(AttackPos.position, AttackRange);
		}*/

		#endregion
	}
}
