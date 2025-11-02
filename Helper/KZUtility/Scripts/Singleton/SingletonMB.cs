using System;
using UnityEngine;

namespace KZLib.KZUtility
{
	/// <summary>
	/// Not exist in scene -> error
	/// </summary>
	public abstract class SingletonMB<TBehaviour> : MonoBehaviour where TBehaviour : MonoBehaviour
	{
		protected static TBehaviour? s_instance = null;

		public static TBehaviour In => s_instance ?? throw new NullReferenceException($"{typeof(TBehaviour)} is not exist.");

		protected void Awake()
		{
			//! Check only one
			if(s_instance)
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

			s_instance = gameObject.GetComponent<TBehaviour>();

			if(!s_instance)
			{
				throw new NullReferenceException($"{typeof(TBehaviour)} is not exist.");
			}

			Initialize();
		}

		protected virtual void Initialize() { }

		protected void OnDestroy()
		{
			Release();

			if(s_instance)
			{
				if(Application.isPlaying)
				{
					Destroy(gameObject);
				}
				else
				{
					DestroyImmediate(gameObject);
				}

				s_instance = null;
			}
		}

		protected virtual void Release() { }

		public static bool HasInstance => s_instance;
	}

	/// <summary>
	/// Not exist in scene -> auto create
	/// </summary>
	public class AutoSingletonMB<TBehaviour> : MonoBehaviour where TBehaviour : MonoBehaviour
	{
		protected static TBehaviour? s_instance;

		public static TBehaviour In
		{
			get
			{
				if(!s_instance)
				{
					s_instance = FindObjectOfType<TBehaviour>();

					if(!s_instance)
					{
						s_instance = new GameObject(typeof(TBehaviour).Name).AddComponent<TBehaviour>();
					}
				}

				return s_instance ?? throw new NullReferenceException($"{typeof(TBehaviour)} is not exist.");
			}
		}

		protected void Awake()
		{
			DontDestroyOnLoad(this);

			Initialize();
		}

		protected virtual void Initialize() { }

		protected void OnDestroy()
		{
			Release();

			s_instance = null;
		}

		protected virtual void Release() { }

		public static bool HasInstance => s_instance;
	}

	/// <summary>
	/// Load in resources folder.
	/// </summary>
	public class LoadSingletonMB<TBehaviour> : AutoSingletonMB<TBehaviour> where TBehaviour : MonoBehaviour
	{
		public static new TBehaviour In
		{
			get
			{
				if(!s_instance)
				{
					s_instance = FindObjectOfType<TBehaviour>();

					if(!s_instance)
					{
						var instance = Resources.Load<TBehaviour>($"Prefab/{typeof(TBehaviour).Name}");

						s_instance = Instantiate(instance);
						s_instance.name = instance.name;
					}
				}

				return s_instance ?? throw new NullReferenceException($"{typeof(TBehaviour)} is not exist.");
			}
		}
	}
}