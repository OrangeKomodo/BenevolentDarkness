using Player;
using UnityEngine;

namespace Spells
{
	public class Spell : MonoBehaviour
	{
		public virtual void Init(PlayerController playerController, SpellCasting spellCaster)
		{
			PlayerController = playerController;
			SpellCaster = spellCaster;
		}

		protected PlayerController PlayerController;
		protected SpellCasting SpellCaster;

		public int ManaCost;
	}
}
