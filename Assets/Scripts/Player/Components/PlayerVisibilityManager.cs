using System.Collections.Generic;
using Events;
using Interfaces;
using Items;
using UnityEngine;

namespace Player.Components
{
	// AI - Manages player visibility calculation based on nearby light sources.
	public class PlayerVisibilityManager : MonoBehaviour, IDetectable
	{
		#region Fields

		[SerializeField] private float _baseVisibilityFactor = 0.1f;

		private List<LightArea> _lights = new List<LightArea>();
		private float _lightTotals = 0f;
		private float _rawVisibilityFactor = 0.1f;
		private bool _hidden = false;

		#endregion

		#region Properties

		public float VisibilityFactor { get; set; } = 0.1f;
		public bool IsHidden => _hidden;
		public float RawVisibilityFactor => _rawVisibilityFactor;
		public float LightTotals => _lightTotals;

		#endregion

		#region Update Functions

		// AI - Calculate visibility based on nearby light sources.
		public void UpdateVisibility(bool forceHidden = false)
		{
			CalculateLightInfluence();
			_rawVisibilityFactor = _baseVisibilityFactor + _lightTotals;
			VisibilityFactor = (forceHidden || _hidden) ? 0f : _rawVisibilityFactor;
		}

		// AI - Sum the influence of all nearby light sources.
		private void CalculateLightInfluence()
		{
			_lightTotals = 0f;
			for (int i = 0; i < _lights.Count; i++)
			{
				if (_lights[i] == null)
				{
					continue;
				}

				float radius = _lights[i].Collider2D.radius;
				float distance = Vector2.Distance(transform.position, _lights[i].transform.position);
				_lightTotals += radius * (radius - distance) / 50f;
			}
		}

		#endregion

		#region Light Management Functions

		// AI - Register a light source that affects the player.
		public void AddLight(LightArea light)
		{
			if (light == null || _lights.Contains(light))
			{
				return;
			}

			_lights.Add(light);
		}

		// AI - Unregister a light source.
		public void RemoveLight(LightArea light)
		{
			if (light == null || !_lights.Contains(light))
			{
				return;
			}

			_lights.Remove(light);
		}

		#endregion

		#region State Management Functions

		// AI - Set whether the player is hidden by a spell or ability.
		public void SetHidden(bool hidden)
		{
			_hidden = hidden;
			PlayerEvents.TriggerVisibilityChanged(VisibilityFactor);
		}

		#endregion
	}
}
