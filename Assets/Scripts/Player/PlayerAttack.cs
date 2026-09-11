using AI.Guard;
using DamageSystem;
using UnityEngine;

namespace Player
{
	public class PlayerAttack : MonoBehaviour
	{
		public int Damage;

		public Transform AttackPos;
		public float AttackRange;

		public float AttackCooldown;
		public LayerMask WhatAreEnemies;

		private PlayerController _playerController;

		private float _currentAttackCooldown;
		private bool _seesBackside = false;

		private bool _attackButtonPressed = true;

		private void Start()
		{
			_playerController = GetComponent<PlayerController>();
		}

		private void Update()
		{
			HandleAttack();
			
			HandleAttackCooldown();

			HandleAttackButtonReleased();
		}

		private void HandleAttack()
		{
			if (!_playerController.CanAttack)
			{
				return;
			}

			CheckForBackside();

			PerformAttack();
		}

		private void CheckForBackside()
		{
			// Calculate the raycast to check for a Backside
			Vector2 playerPosition = transform.position;
			Vector2 attackDirection = transform.right * (transform.localScale.x / Mathf.Abs(transform.localScale.x));

			//Draw and perform the raycast
			Debug.DrawRay(playerPosition, attackDirection * AttackRange, Color.magenta);
			RaycastHit2D playerRayHit =
				Physics2D.Raycast(transform.position, attackDirection, AttackRange, WhatAreEnemies);

			// Negative check for the Backside
			if (playerRayHit.collider == null || !playerRayHit.collider.name.Equals("Backside"))
			{
				// If the Backside isn't seen when it previously was, unload the Attack Icons
				if (_seesBackside)
				{
					_playerController.LoadAttackIcons(false);
					_seesBackside = false;
				}

				return;
			}

			// If the Backside is seen when it previously wasn't, load the Attack Icons
			if (!_seesBackside)
			{
				_playerController.LoadAttackIcons(true);
				_seesBackside = true;
			}

			// Handle player subduing the enemy
			if (Input.GetButtonDown("Subdue"))
			{
				_playerController.PlaySound("Swipe");
				playerRayHit.collider.GetComponentInParent<Guard>().OnGuardUnconscious();
				_playerController.Attack(1);

				return;
			}

			// Handle player attacking the enemy
			// NOTE: It's purposeful that this attack ignores the cooldown. If the player has the enemy's backside, it should be a free kill.
			if (Input.GetAxis("Attack") >= 1f)
			{
				_playerController.PlaySound("Swipe");
				playerRayHit.collider.GetComponentInParent<LivingEntity>().TakeHit(1000);
				_playerController.Attack(0);
				_attackButtonPressed = false;
				_currentAttackCooldown = AttackCooldown;
			}
		}

		private void HandleAttackCooldown()
		{
			// Handle ticking down the attack cooldown
			if (_currentAttackCooldown <= 0f)
			{
				return;
			}
			
			_currentAttackCooldown -= Time.deltaTime;
		}

		private void HandleAttackButtonReleased()
		{
			// If the Attack button is fully pressed, record it
			if (Input.GetAxis("Attack") >= 1f)
			{
				_attackButtonPressed = true;
				return;
			}
			
			// If the Attack button is a good deal un-pressed, reset the variable
			if (Input.GetAxis("Attack") <= 0.8f)
			{
				_attackButtonPressed = false;
				return;
			}
		}

		private void PerformAttack()
		{
			// Check if the attack button is active
			if (Input.GetAxis("Attack") < 1f)
			{
				return;
			}

			// Check if the cooldown has expired
			if (_currentAttackCooldown > 0f)
			{
				return;
			}
			
			// Check if the button was released since last attack was consummated
			if (_attackButtonPressed)
			{
				return;
			}
			
			// Find enemy colliders in attack area
			Collider2D[] enemiesToDamage = Physics2D.OverlapCircleAll(AttackPos.position, AttackRange, WhatAreEnemies);
			
			// Iterate through the colliders and apply damage
			for (int enemyColliderIndex = 0; enemyColliderIndex < enemiesToDamage.Length; enemyColliderIndex++)
			{
				// Ignore the Backside and Player colliders that might be found in the list
				if (enemiesToDamage[enemyColliderIndex].name.Equals("Backside") || enemiesToDamage[enemyColliderIndex].name.Equals("Player"))
				{
					continue;
				}
					
				enemiesToDamage[enemyColliderIndex].GetComponent<LivingEntity>().TakeHit(Damage);
			}
			
			// Perform attack audio and animations
			_playerController.PlaySound("Swipe");
			_playerController.Attack(0);
			
			// Set attack limiters
			_currentAttackCooldown = AttackCooldown;
			_attackButtonPressed = true;
		}

		/*private void OnDrawGizmosSelected()
		{
			// Optional function to draw the Attack Sphere for debugging
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(AttackPos.position, AttackRange);
		}*/
	}
}
