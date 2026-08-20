using AI.Guard;
using DamageSystem;
using UnityEngine;

namespace Player
{
	public class PlayerAttack : MonoBehaviour
	{
		public int damage;

		public Transform attackPos;
		public float attackRange;

		public float startTimeBetweenAttacks;
		public LayerMask whatAreEnemies;

		PlayerInfo playerInfo;

		float timeBetweenAttacks;
		bool seesBackside = false;

		bool triggerReleased = true;

		void Start()
		{
			playerInfo = GetComponent<PlayerInfo>();
		}

		void FixedUpdate()
		{
			if (playerInfo.canAttack)
			{
				RaycastHit2D playerRayHit;
				Debug.DrawRay(transform.position, transform.right * (transform.localScale.x / Mathf.Abs(transform.localScale.x)), Color.magenta);
				playerRayHit = Physics2D.Raycast(transform.position, transform.right * (transform.localScale.x / Mathf.Abs(transform.localScale.x)), 1f, whatAreEnemies);
			
				if (playerRayHit.collider != null && playerRayHit.collider.name.Equals("Backside"))
				{
					if (!seesBackside && !playerInfo.disguisedAsGuard)
					{
						transform.GetComponent<PlayerInfo>().LoadAttackIcons(true);
						seesBackside = true;
					}

					if (Input.GetAxis("Attack") == 1f)
					{
						playerInfo.PlaySound("Swipe");
						playerRayHit.collider.GetComponentInParent<LivingEntity>().TakeHit(1000);
						playerInfo.Attack(0);
					}
					else if (Input.GetButtonDown("Subdue"))
					{
						playerInfo.PlaySound("Swipe");
						playerRayHit.collider.GetComponentInParent<Guard>().OnGuardUnconscious();
						playerInfo.Attack(1);
					}
				}
				else
				{
					if (seesBackside)
					{
						transform.GetComponent<PlayerInfo>().LoadAttackIcons(false);
						seesBackside = false;
					}

					if (timeBetweenAttacks <= 0)
					{
						if (Input.GetAxis("Attack") == 1f && triggerReleased)
						{
							playerInfo.PlaySound("Swipe");
							Collider2D[] enemiesToDamage = Physics2D.OverlapCircleAll(attackPos.position, attackRange, whatAreEnemies);
						
							for (int i = 0; i < enemiesToDamage.Length; i++)
							{
								if (!enemiesToDamage[i].name.Equals("Backside") &&
								    !enemiesToDamage[i].name.Equals("Player"))
								{
									enemiesToDamage[i].GetComponent<LivingEntity>().TakeHit(damage);
								}
							}
						
							timeBetweenAttacks = startTimeBetweenAttacks;
							playerInfo.Attack(0);
							triggerReleased = false;
						}
					}
					else
					{
						timeBetweenAttacks -= Time.deltaTime;
					}
				}
			}

			if (Input.GetAxis("Attack") == 1 && triggerReleased)
			{
				triggerReleased = false;
			}

			if (Input.GetAxis("Attack") == 0 && !triggerReleased)
			{
				triggerReleased = true;
			}
		}

		void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(attackPos.position, attackRange);
		}
	}
}
