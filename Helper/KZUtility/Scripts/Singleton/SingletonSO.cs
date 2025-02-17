using UnityEngine;
using System;
using System.IO;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace KZLib.KZUtility
{
	public abstract class SingletonSO<TScriptable> : ScriptableObject where TScriptable : ScriptableObject
	{
		protected static TScriptable? s_instance = null;

		public static TScriptable In
		{
			get
			{
				var typeName = typeof(TScriptable).Name;

				if(!s_instance)
				{
					var path = Path.Combine("ScriptableObject",typeof(TScriptable).Name);

					s_instance = Resources.Load<TScriptable>(path);
				}

#if UNITY_EDITOR
				if(!s_instance)
				{
					s_instance = CreateInstance<TScriptable>();

					if(s_instance is SingletonSO<TScriptable> singleton)
					{
						singleton.OnCreate();
					}

					var assetPath = Path.Combine("Assets","Resources","ScriptableObject");

					Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(),assetPath));

					AssetDatabase.CreateAsset(s_instance,Path.Combine(assetPath,$"{typeName}.asset"));
					AssetDatabase.Refresh();
				}
#endif

				return s_instance ?? throw new NullReferenceException($"{typeof(TScriptable)} is not exist.");
			}
		}

		protected void Awake()
		{
			Initialize();
		}

		protected virtual void Initialize() { }

#if UNITY_EDITOR
		/// <summary>
		/// Only Create
		/// </summary>
		protected virtual void OnCreate() { }
#endif

		public void OnDestroy()
		{
			Release();

			s_instance = null;
		}

		protected virtual void Release() { }

		public static bool HasInstance => s_instance;
	}
}