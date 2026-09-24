using System;
using UnityEngine;

namespace GameManager
{
	public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		protected Singleton() : base()
		{
			if (this as T == null)
			{
				string className = GetType().Name;
				throw new InvalidCastException($"Class {className} must inherit from Singleton<{className}>.");
			}
		}

		public static T Instance { get; private set; }

		protected enum DuplicateActions
		{
			Ignore,
			DestroyScript,
			DestroyGameObject,
			ThrowException
		}

		protected enum SingletonLifespan
		{
			Global,
			Scene
		}

		protected enum StartingActions
		{
			None,
			DisableAfterStart
		}

		protected static DuplicateActions ActionOnDuplicate = DuplicateActions.DestroyGameObject;
		protected static SingletonLifespan Lifespan = SingletonLifespan.Scene;
		protected static StartingActions ActionOnStart = StartingActions.None;

		protected virtual void Awake()
		{
			if (Instance != null)
			{
				switch (ActionOnDuplicate)
				{
					case DuplicateActions.Ignore:
						return;
					case DuplicateActions.DestroyScript:
						DestroyImmediate(this);
						return;
					case DuplicateActions.DestroyGameObject:
						DestroyImmediate(gameObject);
						return;
					case DuplicateActions.ThrowException:
					default:
						Type type = GetType().BaseType;
						while (!type.Name.StartsWith("Singleton"))
						{
							type = type.BaseType;
						}

						throw new NotSupportedException(
							$"There can only be one instance of {type.GetGenericArguments()[0].Name}.");
				}
			}

			Instance = this as T;

			if (Lifespan == SingletonLifespan.Global)
			{
				DontDestroyOnLoad(gameObject);
			}

			if (ActionOnStart == StartingActions.DisableAfterStart)
			{
				gameObject.SetActive(false);
			}
		}

		protected virtual void OnDestroy()
		{
			if (Instance == this as T)
			{
				Instance = null;
			}
		}
	}
}
