using UnityEngine;
using System;
using System.IO;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace KZLib.Utilities
{
	/// <summary>
	/// Singleton base for <see cref="ScriptableObject"/> assets loaded from <see cref="ResourcesPath"/>.
	/// In the editor, <see cref="In"/> creates the asset when missing.
	/// </summary>
	public abstract class SingletonSO<TScriptable> : ScriptableObject where TScriptable : SingletonSO<TScriptable>
	{
		protected static TScriptable? s_instance = null;

		/// <summary>
		/// Resources load path relative to a <c>Resources</c> folder.
		/// Default: <c>ScriptableObject/{typeName}</c>.
		/// </summary>
		protected virtual string ResourcesPath => $"ScriptableObject/{GetType().Name}";

		/// <summary>
		/// Returns the singleton asset from <c>Resources/{ResourcesPath}</c>.
		/// In the editor, the first access also creates and saves
		/// <c>Assets/Resources/{ResourcesPath}.asset</c> when the asset does not exist.
		/// This side effect is intentional.
		/// </summary>
		public static TScriptable In
		{
			get
			{
				if(!s_instance)
				{
					var resourcesPath = _GetResourcesPath();

					s_instance = Resources.Load<TScriptable>(resourcesPath);
				}

#if UNITY_EDITOR
				if(!s_instance)
				{
					s_instance = CreateInstance<TScriptable>();

					var resourcesPath = ((SingletonSO<TScriptable>)s_instance).ResourcesPath;
					var assetFilePath = $"Assets/Resources/{resourcesPath}.asset";
					var assetDirectory = Path.GetDirectoryName(Path.Combine(Directory.GetCurrentDirectory(),assetFilePath));

					if(!string.IsNullOrEmpty(assetDirectory))
					{
						Directory.CreateDirectory(assetDirectory);
					}

					AssetDatabase.CreateAsset(s_instance,assetFilePath);
					AssetDatabase.Refresh();
				}
#endif

				return s_instance ?? throw new InvalidOperationException($"{typeof(TScriptable).Name} singleton is not available.");
			}
		}

		protected void Awake()
		{
			OnCreate();
		}

		/// <summary>
		/// Called when the asset is created in the editor, and when Unity loads the ScriptableObject.
		/// </summary>
		protected virtual void OnCreate() { }

		public void OnDestroy()
		{
			Release();

			s_instance = null;
		}

		protected virtual void Release() { }

		public static bool HasInstance => s_instance != null;

		private static string _GetResourcesPath()
		{
			if(s_instance is SingletonSO<TScriptable> singleton)
			{
				return singleton.ResourcesPath;
			}

			var temp = CreateInstance<TScriptable>();
			var resourcesPath = ((SingletonSO<TScriptable>)temp).ResourcesPath;

#if UNITY_EDITOR
			DestroyImmediate(temp);
#else
			Destroy(temp);
#endif

			return resourcesPath;
		}
	}
}
