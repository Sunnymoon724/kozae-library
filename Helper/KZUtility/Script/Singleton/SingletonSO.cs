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
				if(!s_instance)
				{
					s_instance = Resources.Load<TScriptable>(Path.Combine("ScriptableObjects",typeof(TScriptable).Name));
				}

				return s_instance ?? throw new NullReferenceException($"{typeof(TScriptable)} is not exist.");
			}
		}

		protected void Awake()
		{
#if UNITY_EDITOR
			if(!m_initialize)
			{
				m_initialize = true;

				Initialize();

				EditorUtility.SetDirty(this);

				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();

				return;
			}
#endif
			Open();
		}

		/// <summary>
		/// Start
		/// </summary>
		protected virtual void Open() { }

#if UNITY_EDITOR
		[SerializeField,HideInInspector]
		private bool m_initialize = false;

		/// <summary>
		/// Only Create
		/// </summary>
		protected virtual void Initialize() { }
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