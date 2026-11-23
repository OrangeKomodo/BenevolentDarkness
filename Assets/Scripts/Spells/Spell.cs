using Player;
using UnityEngine;

namespace Spells
{
	// AI - Note: This is a small base class and doesn't require regions.
	public class Spell : MonoBehaviour
	{
		protected PlayerController PlayerController;
		protected SpellCasting SpellCaster;

		public int ManaCost;

		// AI - Initialize the spell with references to the player controller and spell casting system.
		public virtual void Init(PlayerController playerController, SpellCasting spellCaster)
		{
			PlayerController = playerController;
			SpellCaster = spellCaster;
		}
	}
}
