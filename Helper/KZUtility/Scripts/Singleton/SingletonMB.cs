using System;
using UnityEngine;

namespace KZLib.Utilities
{
	/// <summary>
	/// Optional configuration for <see cref="SingletonMB{TBehaviour}"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class SingletonMBConfigAttribute : Attribute
	{
		public bool AutoCreate { get; set; } = false;
		public bool DontDestroy { get; set; } = false;
		public string PrefabPath { get; set; } = "";
	}

	/// <summary>
	/// Singleton base for <see cref="MonoBehaviour"/> types. Access via <see cref="In"/>.
	/// Unity main thread only.
	/// </summary>
	public abstract class SingletonMB<TBehaviour> : MonoBehaviour where TBehaviour : MonoBehaviour
	{
		private static readonly object s_syncRoot = new();
		private static TBehaviour? s_instance = null;
		private static SingletonMBConfigAttribute? s_config = null;

		private static SingletonMBConfigAttribute SingletonConfig
		{
			get
			{
				if(s_config == null)
				{
					var attribute = Attribute.GetCustomAttribute(typeof(TBehaviour),typeof(SingletonMBConfigAttribute));

					if(attribute != null)
					{
						s_config = attribute as SingletonMBConfigAttribute;
					}

					s_config ??= new SingletonMBConfigAttribute();
				}

				return s_config;
			}
		}

		/// <summary>
		/// Returns the singleton instance, finding it in the scene or creating it when configured.
		/// </summary>
		public static TBehaviour In
		{
			get
			{
				if(s_instance == null)
				{
					lock(s_syncRoot)
					{
						if(s_instance == null)
						{
							s_instance = _FindOrCreateInstance();
						}
					}
				}

				return s_instance ?? throw new InvalidOperationException($"{typeof(TBehaviour).Name} singleton is not available.");
			}
		}

		public static bool HasInstance => s_instance != null;

		private static TBehaviour? _FindOrCreateInstance()
		{
			var instance = _FindInstanceInScene();

			if(instance != null)
			{
				return instance;
			}

			var config = SingletonConfig;

			if(!config.AutoCreate)
			{
				return null;
			}

			if(string.IsNullOrEmpty(config.PrefabPath))
			{
				var gameObject = new GameObject(typeof(TBehaviour).Name);

				return gameObject.AddComponent<TBehaviour>();
			}

			var prefab = Resources.Load<TBehaviour>(config.PrefabPath);

			if(prefab == null)
			{
				return null;
			}

			var created = Instantiate(prefab);
			created.name = prefab.name;

			return created;
		}

		private static TBehaviour? _FindInstanceInScene()
		{
#if UNITY_2023_1_OR_NEWER
			return FindFirstObjectByType<TBehaviour>();
#else
			return FindObjectOfType<TBehaviour>();
#endif
		}

		protected virtual void Awake()
		{
			if(s_instance != null && s_instance != this)
			{
				if(Application.isPlaying)
				{
					Destroy(gameObject);
				}
				else
				{
					DestroyImmediate(gameObject);
				}

				return;
			}

			s_instance = this as TBehaviour;

			if(SingletonConfig.DontDestroy)
			{
				DontDestroyOnLoad(gameObject);
			}

			_Initialize();
		}

		protected virtual void _Initialize() { }

		protected virtual void OnDestroy()
		{
			if(s_instance == this)
			{
				_Release();
				s_instance = null;
			}
		}

		protected virtual void _Release() { }
	}
}
