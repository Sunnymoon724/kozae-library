using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace KZLib.Utilities
{
	public class LuaManager : Singleton<LuaManager>
	{
		private Script m_luaScript = null!;

		private readonly LazyRegistry<string,DynValue> m_registry = new();

		private bool m_disposed = false;

		private bool m_initialized = false;

		protected override void _Release(bool disposing)
		{
			if(m_disposed)
			{
				return;
			}

			if(disposing)
			{
				m_registry.Release();
			}

			m_disposed = true;

			base._Release(disposing);
		}

		public void LoadLuaScript(string[] luaTextArray)
		{
			var textHashSet = new HashSet<string>();

			m_luaScript = new Script();

			m_registry.Release();

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

		public T InvokeFunction<T>(string functionName,params object[] argumentArray)
		{
			if(!m_initialized)
			{
				throw new InvalidOperationException("Lua script is empty.");
			}

			var function = m_registry.Fetch(functionName,_FindFunction);

			var result = m_luaScript.Call(function,_ConvertToValueArray(argumentArray));

			_CheckVoid(result,functionName);

			return result.ToObject<T>();
		}

		public void InvokeFunction(string functionName,params object[] argumentArray)
		{
			if(!m_initialized)
			{
				throw new InvalidOperationException("Lua script is empty.");
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
			function = m_luaScript!.Globals.Get(functionName);

			if(function == null)
			{
				return false;
			}

			if(function.Type != DataType.Function)
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
	}
}