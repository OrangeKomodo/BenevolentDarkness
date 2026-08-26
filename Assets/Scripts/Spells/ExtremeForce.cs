using UnityEngine;

namespace Spells
{
	public class ExtremeForce : Spell
	{
		public BoxCollider2D BoxCollider;
		public AreaEffector2D AreaEffector2D;

		private void Start()
		{
			PlayerController.PlaySound("Extreme Force");

			float wallDistance = Physics2D.Raycast(transform.position, transform.right, 200f, LayerMask.GetMask("Walls"))
				.distance - 3f;
			BoxCollider.size = new Vector2(wallDistance, BoxCollider.size.y);
			BoxCollider.offset = new Vector2(wallDistance / 2f, BoxCollider.offset.y);

			AreaEffector2D.forceAngle = transform.rotation.y == 1f ? 180 : 0;
		}
	}
}
