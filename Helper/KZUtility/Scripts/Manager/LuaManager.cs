using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace KZLib.Utilities
{
	/// <summary>
	/// MoonSharp Lua script host. Load scripts with <see cref="LoadLuaScript"/> and invoke global functions by name.
	/// Not thread-safe; use from a single thread (Unity main thread).
	/// </summary>
	public class LuaManager : Singleton<LuaManager>
	{
		private Script m_luaScript = null!;

		private readonly LazyRegistry<string,DynValue> m_registry = new();

		private bool m_initialized = false;

		private LuaManager() { }

		protected override void _Release(bool disposing)
		{
			if(disposing)
			{
				m_registry.Release();
				m_luaScript = null!;
				m_initialized = false;
			}

			base._Release(disposing);
		}

		/// <summary>
		/// Replaces the current script with <paramref name="luaTextArray"/> entries executed in order.
		/// Duplicate strings in the array are skipped.
		/// </summary>
		public void LoadLuaScript(string[] luaTextArray)
		{
			_ThrowIfDisposed();

			if(luaTextArray == null)
			{
				throw new ArgumentNullException(nameof(luaTextArray));
			}

			if(luaTextArray.Length == 0)
			{
				throw new ArgumentException("Lua text array must contain at least one script.",nameof(luaTextArray));
			}

			var textHashSet = new HashSet<string>();

			m_initialized = false;
			m_registry.Release();
			m_luaScript = new Script();

			foreach(var luaText in luaTextArray)
			{
				if(string.IsNullOrEmpty(luaText))
				{
					throw new ArgumentException($"Lua text cannot be null or empty. [{nameof(luaText)}]");
				}

				if(textHashSet.Contains(luaText))
				{
					continue;
				}

				m_luaScript.DoString(luaText);
				textHashSet.Add(luaText);
			}

			m_initialized = true;
		}

		/// <summary>
		/// Invokes a global Lua function and converts the return value to <typeparamref name="T"/>.
		/// </summary>
		public T InvokeFunction<T>(string functionName,params object[] argumentArray)
		{
			_EnsureReady();

			if(string.IsNullOrEmpty(functionName))
			{
				throw new ArgumentException("Function name cannot be null or empty.",nameof(functionName));
			}

			var function = m_registry.Fetch(functionName,_FindFunction);

			var result = m_luaScript.Call(function,_ConvertToValueArray(argumentArray));

			_CheckVoid(result,functionName);

			return result.ToObject<T>();
		}

		/// <summary>
		/// Invokes a global Lua function with no return value.
		/// </summary>
		public void InvokeFunction(string functionName,params object[] argumentArray)
		{
			_EnsureReady();

			if(string.IsNullOrEmpty(functionName))
			{
				throw new ArgumentException("Function name cannot be null or empty.",nameof(functionName));
			}

			var function = m_registry.Fetch(functionName,_FindFunction);

			m_luaScript.Call(function,_ConvertToValueArray(argumentArray));
		}
		
		private DynValue[] _ConvertToValueArray(object[] argumentArray)
		{
			DynValue _ConvertToValue(object argument)
			{
				return DynValue.FromObject(m_luaScript,argument);
			}

			return argumentArray?.Length > 0 ? Array.ConvertAll(argumentArray,_ConvertToValue) : Array.Empty<DynValue>();
		}

		private bool _FindFunction(string functionName,out DynValue function)
		{
			function = m_luaScript.Globals.Get(functionName);

			if(function.IsNil() || function.Type != DataType.Function)
			{
				return false;
			}

			return true;
		}

		private void _CheckVoid(DynValue result,string functionName)
		{
			if(result.Type == DataType.Void)
			{
				throw new InvalidOperationException($"{functionName} is void function.");
			}
		}

		/// <summary>
		/// After <see cref="Dispose"/>, further use of the same instance throws <see cref="ObjectDisposedException"/>.
		/// Use <see cref="Singleton{TClass}.In"/> again to obtain a new instance.
		/// </summary>
		private void _EnsureReady()
		{
			_ThrowIfDisposed();

			if(!m_initialized || m_luaScript == null)
			{
				throw new InvalidOperationException("Lua script is not loaded. Call LoadLuaScript first.");
			}
		}
	}
}
