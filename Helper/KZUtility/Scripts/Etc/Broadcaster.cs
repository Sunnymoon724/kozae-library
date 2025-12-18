using System;
using System.Collections.Generic;

namespace KZLib.KZUtility
{
	public static class Broadcaster
	{
		private static readonly Dictionary<string,Delegate> m_listenerDict = new();

		public static void EnableListener(string eventName,Action onAction)
		{
			_EnableListener(eventName,onAction);
		}

		public static void EnableListener<TParam>(string eventName,Action<TParam> onAction)
		{
			_EnableListener(eventName,onAction);
		}

		private static void _EnableListener(string eventName,Delegate callback)
		{
			if(callback == null)
			{
				return;
			}

			if(m_listenerDict.TryGetValue(eventName,out var listener) && listener != null)
			{
				_ValidateType(eventName,callback,listener);

				var listenerArray = listener.GetInvocationList();

				bool _IsEqual(Delegate x)
				{
					return x == callback;
				}

				// If the callback is already in the listener, do not add it again
				if(Array.Exists(listenerArray,_IsEqual))
				{
					return;
				}

				m_listenerDict[eventName] = Delegate.Combine(listener,callback);
			}
			else
			{
				m_listenerDict[eventName] = callback;
			}
		}

		public static void DisableListener(string eventName,Action onAction)
		{
			_DisableListener(eventName,onAction);
		}

		public static void DisableListener<TParam>(string eventName,Action<TParam> onAction)
		{
			_DisableListener(eventName,onAction);
		}

		private static void _DisableListener(string eventName,Delegate callback)
		{
			if(callback == null || !m_listenerDict.TryGetValue(eventName,out var listener))
			{
				return;
			}

			_ValidateType(eventName,callback,listener);
			
			var newListener = Delegate.Remove(listener,callback);

			if(newListener == null)
			{
				m_listenerDict.Remove(eventName); 
			}
			else
			{
				m_listenerDict[eventName] = newListener;
			}
		}

		public static void SendEvent(string eventName)
		{
			if(m_listenerDict.TryGetValue(eventName,out var listener) && listener is Action onAction)
			{
				onAction?.Invoke();
			}
		}

		public static void SendEvent<TParam>(string eventName,TParam param)
		{
			if(m_listenerDict.TryGetValue(eventName,out var listener) && listener is Action<TParam> onAction)
			{
				onAction?.Invoke(param);
			}
		}

		private static void _ValidateType(string eventName,Delegate callback,Delegate listener)
		{
			var listenerType = listener.GetType();
			var callbackType = callback.GetType();

			if(listenerType != callbackType)
			{
				throw new InvalidOperationException($"{listenerType.Name} != {callbackType.Name} in {eventName}");
			}
		}
	}
}