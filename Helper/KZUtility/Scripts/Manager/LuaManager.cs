using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace KZLib.KZUtility
{
	public class LuaManager : Singleton<LuaManager>
	{
		private Script m_luaScript = null!;

		private readonly Dictionary<string,DynValue> m_functionDict = new();

		private bool m_disposed = false;

		private bool m_initialized = false;

		protected override void Release(bool disposing)
		{
			if(m_disposed)
			{
				return;
			}

			if(disposing)
			{
				m_functionDict.Clear();
			}

			m_disposed = true;

			base.Release(disposing);
		}

		public void LoadLuaScript(string[] luaTextArray)
		{
			var textHashSet = new HashSet<string>();

			m_luaScript = new Script();
			m_functionDict.Clear();

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

			var function = _GetFunctionOrThrow(functionName);

			var valueArray = argumentArray?.Length > 0 ? Array.ConvertAll(argumentArray,x => DynValue.FromObject(m_luaScript,x)) : Array.Empty<DynValue>();
			var result = m_luaScript.Call(function,valueArray);

			_CheckVoid(result,functionName);

			return result.ToObject<T>();
		}

		public void InvokeFunction(string functionName,params object[] argumentArray)
		{
			if(!m_initialized)
			{
				throw new InvalidOperationException("Lua script is empty.");
			}

			var function = _GetFunctionOrThrow(functionName);

			var valueArray = argumentArray?.Length > 0 ? Array.ConvertAll(argumentArray,x => DynValue.FromObject(m_luaScript,x)) : Array.Empty<DynValue>();

			m_luaScript.Call(function,valueArray);
		}

		private DynValue _GetFunctionOrThrow(string functionName)
		{
			if(!m_functionDict.TryGetValue(functionName,out var function))
			{
				function = m_luaScript!.Globals.Get(functionName);

				if(function.Type != DataType.Function)
				{
					throw new MissingMethodException($"{functionName} was not found in the Lua script.");
				}

				m_functionDict.Add(functionName,function);
			}

			return function;
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