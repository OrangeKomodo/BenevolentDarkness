using UnityEngine;

namespace Spells
{
	public class ExtremeForce : Spell
	{
		public BoxCollider2D BoxCollider;
		public AreaEffector2D AreaEffector2D;

		public float MaxDistance = 200f;
		public float GapBeforeWall = 3f;
		public LayerMask WhatIsWall;

		private void Start()
		{
			PlayerController.PlaySound("Extreme Force");

			// Stretches the Area Effector to fill the room in front of the Player
			RaycastHit2D forceHit = Physics2D.Raycast(transform.position, transform.right, MaxDistance, WhatIsWall);
			float wallDistance = forceHit.distance - GapBeforeWall;
			BoxCollider.size = new Vector2(wallDistance, BoxCollider.size.y);
			BoxCollider.offset = new Vector2(wallDistance / 2f, BoxCollider.offset.y);

			// Set the force angle to face away from the Player
			AreaEffector2D.forceAngle = transform.rotation.y == 1f ? 180 : 0;
		}
	}
}
