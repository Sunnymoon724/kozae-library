using System;
using UnityEngine;

namespace KZLib.Utilities
{
	[AttributeUsage(AttributeTargets.Class)]
	public class SingletonConfigAttribute : Attribute
	{
		public bool AutoCreate { get; set; } = false;
		public bool DontDestroy { get; set; } = false;
		public string PrefabPath { get; set; } = "";
	}

	public abstract class SingletonMB<TBehaviour> : MonoBehaviour where TBehaviour : MonoBehaviour
	{
		protected static TBehaviour? s_instance = null;
		private static SingletonConfigAttribute? s_config = null;

		private static SingletonConfigAttribute SingletonConfig
		{
			get
			{
				if(s_config == null)
				{
					var attribute = Attribute.GetCustomAttribute(typeof(TBehaviour),typeof(SingletonConfigAttribute));

					if(attribute != null)
					{
						s_config = attribute as SingletonConfigAttribute;
					}

					s_config ??= new SingletonConfigAttribute();
				}

				return s_config;
			}
		}

		public static TBehaviour In
		{
			get
			{
				if(s_instance == null)
				{
					s_instance = FindObjectOfType<TBehaviour>();

					if(s_instance == null)
					{
						var config = SingletonConfig;

						if(config.AutoCreate)
						{
							var prefabPath = config.PrefabPath;

							if(string.IsNullOrEmpty(prefabPath))
							{
								var instance = new GameObject(typeof(TBehaviour).Name);

								s_instance = instance.AddComponent<TBehaviour>();
							}
							else
							{
								var instance = Resources.Load<TBehaviour>(prefabPath);

								if(instance != null)
								{
									s_instance = Instantiate(instance);
									s_instance.name = instance.name;
								}
							}
						}
					}

					if(s_instance == null)
					{
						throw new NullReferenceException($"{typeof(TBehaviour)} is not exist.");
					}
				}

				return s_instance;
			}
		}

		public static bool HasInstance => s_instance != null;

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